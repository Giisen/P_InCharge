using System.Net.Http.Json;
using InCharge.Shared.DTOs;
using MongoDB.Driver.Linq;
using MudBlazor;

namespace InCharge.Client.Pages;

partial class Home
{
    bool showYear = false;
    bool showMonth = false;
    bool showDay = false;
    public DateTime? datePicker = DateTime.Today;
    public string inputyear = "";
    public string inputmonth = "";
    public string inputday = "";
    public List<string> listofyears = new() { "", "2023", "2024" };
    List<string> listofMonths = new() { "", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };

    List<string> listofDays = new()
    {
        "", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
        "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
        "31"
    };

    private EvCostDailyDto dailyCost = new();
    private EvCostMonthlyDto monthlyCost = new();
    private EvCostYearlyDto yearlyCost = new();
    private string latestDateInChargeDb;
    private string latestDateInRateDb;
    private string today = DateTime.Today.ToString("yyyy-MM-dd");
    private List<string> listOfChargeDateToSaveToDb = new();
    private List<string> listOfRateDateToSaveToDb = new();
    private CurrentRateDto currentRate = new();
    private List<ChartSeries> hourlyRateSeries = new();
    private string[] xAxeln = new string[] { };
    private readonly ChartOptions _options = new();


    protected override async Task OnInitializedAsync()
    {
        await GetDatesToSaveData();
        if (listOfChargeDateToSaveToDb.Count > 0 || listOfRateDateToSaveToDb.Count > 0)
        {
            await SaveMissingChargeDaysToDb(listOfChargeDateToSaveToDb);
            await SaveMissingRateDaysToDb(listOfRateDateToSaveToDb);
            await SaveDailyCostToDb();
            await SaveMonthlyCostToDb();
        }
        await GetCurrentRate();
        _options.YAxisTicks = 1;
    }

    private async Task GetDatesToSaveData()
    {
        var response = await Http.GetAsync($"/latestdateinchargedb");
        latestDateInChargeDb = await response.Content.ReadAsStringAsync();
        latestDateInRateDb = await (await Http.GetAsync($"/latestdateinratedb")).Content.ReadAsStringAsync();
        listOfChargeDateToSaveToDb = GetDateList(latestDateInChargeDb);
        listOfRateDateToSaveToDb = GetDateList(latestDateInRateDb);
        StateHasChanged();
    }

    private async Task SaveMonthlyCostToDb()
    {
        string currentYear = DateTime.Today.Year.ToString();
        string currentMonth = DateTime.Today.Month < 10
            ? "0" + DateTime.Today.Month.ToString()
            : DateTime.Today.Month.ToString();
        var response = await Http.PostAsync($"/savemonthlyevcost/{currentYear}/{currentMonth}", null);
        var result = response.StatusCode;
    }

    public List<string> GetDateList(string lastDateInDb)
    {
        List<string> dateList = new List<string>();

        DateTime startDate = DateTime.Parse(lastDateInDb);
        DateTime endDate = DateTime.Parse(today);

        for (DateTime date = startDate.AddDays(1); date <= endDate.AddDays(-1); date = date.AddDays(1))
        {
            dateList.Add(date.ToString("yyyy-MM-dd"));
        }

        return dateList;
    }

    private async Task<bool> SaveMissingChargeDaysToDb(List<string> days)
    {
        try
        {
            await Http.PostAsJsonAsync($"/savemissingchargedaystodb", days);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private async Task<bool> SaveMissingRateDaysToDb(List<string> days)
    {
        try
        {
            await Http.PostAsJsonAsync($"/savemissingratedaystodb", days);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private async Task<bool> SaveDailyCostToDb()
    {
        try
        {
            foreach (var day in listOfRateDateToSaveToDb)
            {
                var response = await Http.PostAsync($"/savedailyevcost/{day}", null);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to save daily cost for {day}. Status code: {response.StatusCode}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
            throw;
        }
    }


    private async Task GetData()
    {
        CheckDateInput(inputyear, inputmonth, inputday);
    }

    private async void CheckDateInput(string year, string month, string day)
    {
        if (!string.IsNullOrWhiteSpace(year) && string.IsNullOrWhiteSpace(month) && string.IsNullOrWhiteSpace(day))
        {
            await GetYearlyCost(year);
            showYear = true;
            StateHasChanged();
        }
        else if (!string.IsNullOrWhiteSpace(year) && !string.IsNullOrWhiteSpace(month) &&
                 string.IsNullOrWhiteSpace(day))
        {
            await GetMonthlyCost(year, month);
            showMonth = true;
            StateHasChanged();
        }
        else if (!string.IsNullOrWhiteSpace(year) && !string.IsNullOrWhiteSpace(month) &&
                 !string.IsNullOrWhiteSpace(day))
        {
            await GetDailyCost($"{year}-{month}-{day}");
            showDay = true;
            StateHasChanged();
        }
    }


    private async Task GetYearlyCost(string year)
    {
        if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(year))
        {
            yearlyCost = new EvCostYearlyDto();
            return;
        }

        var result = await Http.GetFromJsonAsync<EvCostYearlyDto>($"/yearlyevcost/{year}");
        yearlyCost = result;
        StateHasChanged();
    }

    private async Task GetMonthlyCost(string year, string month)
    {
        if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(month))
        {
            monthlyCost = new EvCostMonthlyDto();
            return;
        }

        var result = await Http.GetFromJsonAsync<EvCostMonthlyDto>($"/monthlyevcost/{year}/{month}");
        monthlyCost = result;
        StateHasChanged();
    }

    private async Task GetDailyCost(string date)
    {
        var date2 = (date.Length < 6) ? "2024-03-07" : date;
        var result = await Http.GetFromJsonAsync<EvCostDailyDto>($"/dailyevcost/{date2}");
        dailyCost = result;
        StateHasChanged();
    }

    public async Task<List<string>> GetDaysInMonth()
    {
        var datesInMonth = new List<string>();
        var intYear = int.Parse(inputyear);
        var intMonth = int.Parse(inputmonth);

        var daysInMonth = DateTime.DaysInMonth(intYear, intMonth);

        for (int i = 1; i <= daysInMonth; i++)
        {
            DateTime date = new DateTime(intYear, intMonth, i);
            datesInMonth.Add(date.ToString("dd"));
        }

        return datesInMonth;
    }

    private async Task GetCurrentRate()
    {
        currentRate = await Http.GetFromJsonAsync<CurrentRateDto>($"/currentrate");
        //double[] sekPerHour = currentRate.hourRate.Select(h => Math.Round(h.SEK_per_kWh, 2)).ToArray();
        decimal[] sekPerHourDec = currentRate.hourRate.Select(h => Convert.ToDecimal(Math.Round(h.SEK_per_kWh, 2))).ToArray();
        double[] sekPerHour = sekPerHourDec.Select(h => Convert.ToDouble(Math.Round(h, 2))).ToArray();

        // ChartSeries cost = new ChartSeries();
        // cost.Data = sekPerHour;
        // hourlyRateSeries.Add(cost);
        // cost.Name = "Kr";
        // string[] timma = currentRate.hourRate.Select(h => h.time_start.ToString("HH")).ToArray();
        // xAxeln = timma;
        ChartSeries cost = new ChartSeries
        {
            Data = sekPerHour.Select(v => Convert.ToDouble(v.ToString("F2"))).ToArray(),
            Name = "Kr"
        };
        hourlyRateSeries.Add(cost);
        string[] timma = currentRate.hourRate.Select(h => h.time_start.ToString("HH")).ToArray();
        xAxeln = timma;

        StateHasChanged();

    }
}