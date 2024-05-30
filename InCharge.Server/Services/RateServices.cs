using InCharge.DataAccess.Models;
using InCharge.DataAccess.Respositories;
using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Rate;

namespace InCharge.Server.Services;

public class RateServices : IRate<HourlyRateDto>, IRate<DailyRateDto>, IRate<List<DailyRateDto>>, IRate<bool>,
    IRate<CurrentRateDto>
{
    private readonly MongoDbHourlyRateRepository _dbHourly;
    private readonly MongoDbDailyRateRepository _dbDaily;

    private Uri _baseUrl = new("https://www.elprisetjustnu.se/api/v1/prices/");

    public RateServices(MongoDbHourlyRateRepository dbHourly, MongoDbDailyRateRepository dbDaily)
    {
        _dbHourly = dbHourly;
        _dbDaily = dbDaily;
    }

    public async Task<List<HourlyRateDto>> GetAndSaveHourlyRate(string date)
    {
        var result = await GetHourlyRate(date);
        var modelList = await ConvertToDailyModel(result);
        await SaveDailyToDb(modelList);
        return result;
    }

    public async Task<string> GetLatestDateInRateDb()
    {
        return await _dbDaily.GetLatestDateInChargeDb();
    }


    public async Task<List<HourlyRateDto>> GetHourlyRate(string date)
    {
        var year = date.Substring(0, 4);
        var month = date.Substring(5, 2);
        var day = date.Substring(8, 2);

        using (var client = new HttpClient())
        {
            string apiUrl = year + "/" + month + "-" + day + "_SE3.json";
            var result = await client.GetFromJsonAsync<List<HourlyRateDto>>(_baseUrl + apiUrl);
            return result;
        }
    }

    public async Task<bool> SaveMissingDaysToDb(List<string> days)
    {
        try
        {
            foreach (var d in days)
            {
                var listofhours = await GetHourlyRate(d);
                var day = await ConvertToDailyModel(listofhours);
                await SaveDailyToDb(day);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<CurrentRateDto> GetCurrentRate()
    {
        var currentRate = new CurrentRateDto();
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

        var year = today.Substring(0, 4);
        var month = today.Substring(5, 2);
        var day = today.Substring(8, 2);
        var tomorrowDayOnly = tomorrow.Substring(8, 2);

        try
        {
            using (var client = new HttpClient())
            {
                string todayApiUrl = year + "/" + month + "-" + day + "_SE3.json";
                var todayResult = await client.GetFromJsonAsync<List<HourlyRateDto>>(_baseUrl + todayApiUrl);
                if (todayResult != null)
                {
                    currentRate.hourRate.AddRange(todayResult);
                }

                string tomorrowApiUrl = year + "/" + month + "-" + tomorrowDayOnly + "_SE3.json";
                var tomorrowResult = await client.GetFromJsonAsync<List<HourlyRateDto>>(_baseUrl + tomorrowApiUrl);

                if (tomorrowResult != null)
                {
                    currentRate.hourRate.AddRange(tomorrowResult);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return currentRate;
    }


    private async Task<DailyRateModel> ConvertToDailyModel(List<HourlyRateDto> result)
    {
        DailyRateModel dailyModel = new DailyRateModel();
        var firstHour = result.FirstOrDefault();
        dailyModel.day = (firstHour?.time_end ?? DateTime.MinValue).ToString("yyyy-MM-dd");
        dailyModel.hours = new List<HourlyRateModel>();
        foreach (var dto in result)
        {
            var model = new HourlyRateModel
            {
                SEK_per_kWh = dto.SEK_per_kWh,
                EUR_per_kWh = dto.EUR_per_kWh,
                EXR = dto.EXR,
                time_start = dto.time_start,
                time_end = dto.time_end
            };

            dailyModel.hours.Add(model);
        }

        return dailyModel;
    }

    private async Task<DailyRateDto> ConvertToDailyDto(DailyRateModel result)
    {
        DailyRateDto dailyDto = new DailyRateDto();
        dailyDto.day = result.day;
        dailyDto.hours = new List<HourlyRateDto>();
        dailyDto.id = result.Id;
        foreach (var model in result.hours)
        {
            var dto = new HourlyRateDto
            {
                SEK_per_kWh = model.SEK_per_kWh,
                EUR_per_kWh = model.EUR_per_kWh,
                EXR = model.EXR,
                time_start = model.time_start,
                time_end = model.time_end
            };

            dailyDto.hours.Add(dto);
        }

        return dailyDto;
    }


    public async Task<DailyRateDto> GetDailyRateFromDb(string date)
    {
        var result = await _dbDaily.GetDailyRate(date);
        return await ConvertToDailyDto(result);
    }

    public async Task<List<DailyRateDto>> GetAllDailyRates()
    {
        List<DailyRateDto> list = new List<DailyRateDto>();
        var result = await _dbDaily.GetAllDailyRates();
        foreach (var dto in result)
        {
            await ConvertToDailyDto(dto);
        }

        return list;
    }


    private async Task SaveDailyToDb(DailyRateModel day)
    {
        await _dbDaily.InsertDailyRate(day);
    }
}