using InCharge.DataAccess.Models;
using InCharge.DataAccess.Respositories;
using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Cost;

namespace InCharge.Server.Services;

public class EvCostServices : IEvCostHourly<EvCostHourlyDto>, IEvCostDaily<EvCostDailyDto>,
    IEvCostMonthly<EvCostMonthlyDto>, IEvCostYearly<EvCostYearlyDto>
{
    private readonly EvChargeServices _evChargeServices;
    private readonly RateServices _rateServices;
    private readonly MongoDbDailyCostRepository _dbDailyCostRepository;
    private readonly MongoDbMonthlyCostRepository _dbMonthlyCostRepository;
    private readonly MongoDbYearlyCostRepository _dbMongoDbYearlyCostRepository;

    public EvCostServices(EvChargeServices evChargeServices, RateServices rateServices,
        MongoDbDailyCostRepository dbDailyCostRepository, MongoDbMonthlyCostRepository dbMonthlyCostRepository,
        MongoDbYearlyCostRepository dbMongoDbYearlyCostRepository)
    {
        _evChargeServices = evChargeServices;
        _rateServices = rateServices;
        _dbDailyCostRepository = dbDailyCostRepository;
        _dbMonthlyCostRepository = dbMonthlyCostRepository;
        _dbMongoDbYearlyCostRepository = dbMongoDbYearlyCostRepository;
    }

    public async Task<EvCostHourlyDto> CalculateCostForOneHourEvChargeFromDb(DateTime date)
    {
        var hourCharge = await _evChargeServices.GetOneHourlyChargeFromDb(date);
        var totHourCharge = hourCharge.h1b + hourCharge.h2b + hourCharge.h3b;
        var stringDate = date.Date.ToString("yyyy-MM-dd");
        var dailyRate = await _rateServices.GetDailyRateFromDb(stringDate);
        var hour = dailyRate.hours.FirstOrDefault(x => x.time_start.ToLocalTime() == date);
        var cost = new EvCostHourlyDto();
        cost.DateTimeHour = date.ToLocalTime();
        cost.HourlyCost = Convert.ToDecimal(hour.SEK_per_kWh) * totHourCharge;
        return cost;
    }

    public async Task<EvCostDailyDto> CalculateCostForOneDayEvChargeFromDb(string date)
    {
        var dailyCharge = await _evChargeServices.GetOneDailyChargeFromDb(date);
        var dailyRate = await _rateServices.GetDailyRateFromDb(date);

        decimal totHourlyGridCharge = 0;
        decimal totHourlySolarCharge = 0;
        decimal totDailyGridCharge = 0;
        decimal totDailySolarCharge = 0;
        decimal totHourlyCost = 0;
        decimal totDailyCost = 0;

        foreach (var hourCharge in dailyCharge.hours)
        {
            totHourlyGridCharge = Decimal.Round((hourCharge.h1b + hourCharge.h2b + hourCharge.h3b) / 3600000m, 2);
            totHourlySolarCharge = Decimal.Round((hourCharge.h1d + hourCharge.h2d + hourCharge.h3d) / 3600000m, 2);
            totDailyGridCharge += totHourlyGridCharge;
            totDailySolarCharge += totHourlySolarCharge;
            int hourChargeIndex = dailyCharge.hours.IndexOf(hourCharge);

            foreach (var hourRate in dailyRate.hours)
            {
                var hourRateIndex = dailyRate.hours.IndexOf(hourRate);
                var rate = (decimal)hourRate.SEK_per_kWh;
                var transferFee = 0.255m;
                var energyTax = 0.428m;
                var effectFee = 0.475m;
                var VAT = 1.25m;
                var totCostPerKwh = (rate + transferFee + energyTax + effectFee) * VAT;

                if (hourChargeIndex == hourRateIndex)
                {
                    totHourlyCost = Decimal.Round(totHourlyGridCharge * totCostPerKwh, 2);
                    totDailyCost += totHourlyCost;
                    break;
                }
            }
        }

        var cost = new EvCostDailyDto();
        cost.Date = date;
        cost.Cost = Math.Round(totDailyCost);
        cost.GridCharge = Math.Round(totDailyGridCharge);
        cost.SolarCharge = Math.Round(totDailySolarCharge);

        return cost;
    }

    public async Task<EvCostDailyDto> GetCostForOneDayEvChargeFromDb(string date)
    {
        try
        {
            var result = await _dbDailyCostRepository.GetDailyCost(date);
            return await ConvertEvDailyCostToDto(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<string>> GetDatesInMonth(string year, string month)
    {
        var datesInMonth = new List<string>();
        var intYear = int.Parse(year);
        var intMonth = int.Parse(month);

        var daysInMonth = DateTime.DaysInMonth(intYear, intMonth);

        for (int i = 1; i <= daysInMonth; i++)
        {
            DateTime date = new DateTime(intYear, intMonth, i);
            datesInMonth.Add(date.ToString("yyyy-MM-dd"));
        }

        return datesInMonth;
    }

    public async Task<EvCostYearlyDto> CalculateCostForOneYearEvCharge(string year)
    {
        var yearlyCost = new EvCostYearlyDto();
        yearlyCost.Year = year;
        yearlyCost.Cost = 0;
        yearlyCost.GridCharge = 0;
        yearlyCost.SolarCharge = 0;
        string currentYear = DateTime.Now.Year.ToString();
        int currentMonth = DateTime.Now.Month;
        int numberOfMonths = (year == currentYear) ? currentMonth : 12;

        for (int i = 1; i <= numberOfMonths; i++)
        {
            var month = await GetCostForOneMonthEvCharge(year, i.ToString("00"));
            yearlyCost.Cost += month.Cost;
            yearlyCost.GridCharge += month.GridCharge;
            yearlyCost.SolarCharge += month.SolarCharge;
        }

        return yearlyCost;
    }

    public async Task<EvCostMonthlyDto> GetCostForOneMonthEvCharge(string year, string month)
    {
        var result = await _dbMonthlyCostRepository.GetMonthlyCost(year, month);
        return await ConvertEvMonthlyCostToDto(result);
    }

    public async Task<bool> SaveCostForOneMonthEvChargeToDb(string year, string month)
    {
        var listOfDatesInMonth = await _dbDailyCostRepository.GetDatesInMonthFromCostDb(year, month);

        var monthlyCost = new EvMonthlyCostModel();
        monthlyCost.Date = $"{year}-{month}";
        monthlyCost.Cost = 0;
        monthlyCost.GridCharge = 0;
        monthlyCost.SolarCharge = 0;
        foreach (var dates in listOfDatesInMonth)
        {
            monthlyCost.Cost += dates.Cost;
            monthlyCost.GridCharge += dates.GridCharge;
            monthlyCost.SolarCharge += dates.SolarCharge;
        }

        var monthlyCostInDb = await _dbMonthlyCostRepository.GetMonthlyCost(year, month);
        if (monthlyCostInDb is null)
        {
            await _dbMonthlyCostRepository.InsertOneMonthlyCost(monthlyCost);
        }
        else
        {
            await _dbMonthlyCostRepository.UpDateOneMonthlyCost(monthlyCost);
        }

        return true;
    }

    public async Task<bool> SaveCostForOneDayEvChargeToDb(string date)
    {
        var dailyDto = await CalculateCostForOneDayEvChargeFromDb(date);
        try
        {
            await _dbDailyCostRepository.InsertDailyCost(await ConvertEvDailyCostToModel(dailyDto));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private async Task<EvDailyCostModel> ConvertEvDailyCostToModel(EvCostDailyDto dto)
    {
        var model = new EvDailyCostModel();
        model.Date = dto.Date;
        model.Cost = dto.Cost;
        model.GridCharge = dto.GridCharge;
        model.SolarCharge = dto.SolarCharge;
        return model;
    }

    private async Task<EvCostDailyDto> ConvertEvDailyCostToDto(EvDailyCostModel model)
    {
        var dto = new EvCostDailyDto();
        dto.Date = model.Date;
        dto.Cost = model.Cost;
        dto.GridCharge = model.GridCharge;
        dto.SolarCharge = model.SolarCharge;
        return dto;
    }

    private async Task<EvCostMonthlyDto> ConvertEvMonthlyCostToDto(EvMonthlyCostModel model)
    {
        var dto = new EvCostMonthlyDto();
        dto.Date = model.Date;
        dto.Cost = model.Cost;
        dto.GridCharge = model.GridCharge;
        dto.SolarCharge = model.SolarCharge;
        return dto;
    }

    public async Task<bool> SaveDailyCostToDbBasedOnOneMonthsDates(string year, string month)
    {
        var listofdates = await GetDatesInMonth(year, month);
        try
        {
            foreach (var day in listofdates)
            {
                await SaveCostForOneDayEvChargeToDb(day);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<EvCostYearlyDto> GetCostForOneYearEvCharge(string year)
    {
        var result = await _dbMongoDbYearlyCostRepository.GetYearlyCost(year);
        var dto = new EvCostYearlyDto();
        dto.Year = year;
        dto.GridCharge = result.GridCharge;
        dto.SolarCharge = result.SolarCharge;
        dto.Cost = result.Cost;
        return dto;
    }

    public async Task<bool> SaveCostForOneYearEvChargeToDb(string year)
    {
        var dto = new EvCostYearlyDto();
        dto = await CalculateCostForOneYearEvCharge(year);
        var model = new EvYearlyCostModel();
        model.Year = year;
        model.Cost = dto.Cost;
        model.GridCharge = dto.GridCharge;
        model.SolarCharge = dto.SolarCharge;

        var yearlyCostInDb = await _dbMongoDbYearlyCostRepository.GetYearlyCost(year);
        try
        {
            if (yearlyCostInDb is null)
            {
                await _dbMongoDbYearlyCostRepository.InsertOneYearlyCost(model);
                return true;
            }
            else
            {
                await _dbMongoDbYearlyCostRepository.UpDateOneYearlyCost(model);
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}