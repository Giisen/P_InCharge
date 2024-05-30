// using InCharge.Shared.DTOs;
//
// namespace InCharge.Server.Services;
//
// public class FetchDataBackgroundService : BackgroundService
// {
//     private readonly HttpClient _httpClient;
//     private readonly EvCostServices _costServices;
//     private bool _taskCompleted;
//     private List<EvChargeHourlyDto> ChargingDataList;
//     private List<HourlyRateDto> RateDataList;
//     private DailyRateDto dailyRateData;
//     private List<DailyRateDto> allRateData;
//     private List<string> allDates;
//
//     private EvChargeDailyDto dailyCharge;
//     private EvCostDailyDto dailyCost = new();
//     private EvCostMonthlyDto monthlyCost = new();
//     private string latestDateInChargeDb;
//     private string latestDateInRateDb;
//     private string today = DateTime.Today.ToString("yyyy-MM-dd");
//     private string currentYear = DateTime.Today.Year.ToString("yyyy");
//     private string currentMonth = DateTime.Today.Month.ToString("MM");
//     private List<string> listOfChargeDateToSaveToDb = new();
//     private List<string> listOfRateDateToSaveToDb = new();
//
//     public FetchDataBackgroundService(HttpClient httpClient)
//     {
//         _httpClient = httpClient;
//     }
//
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             if (DateTime.Now.Hour == 16)
//             {
//                 //await GetDatesToSaveData();
//                 // await SaveMissingChargeDaysToDb(listOfChargeDateToSaveToDb);
//                 // await SaveMissingRateDaysToDb(listOfRateDateToSaveToDb);
//                 // await SaveDailyCostToDb();
//                 // await SaveMonthlyCostToDb();
//             }
//
//             await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//         }
//     }
//
//     private async Task SaveMonthlyCostToDb()
//     {
//         var year = currentYear;
//         var month = currentMonth;
//         await _httpClient.GetAsync($"/savedailyevcostforonemonth/{year}/{month}");
//     }
//
//     private async Task GetDatesToSaveData()
//     {
//         var response = await _httpClient.GetAsync("/latestdateinchargedb");
//         latestDateInChargeDb = await response.Content.ReadAsStringAsync();
//         latestDateInRateDb = await (await _httpClient.GetAsync("/latestdateinratedb")).Content.ReadAsStringAsync();
//         listOfChargeDateToSaveToDb = GetDateList(latestDateInChargeDb);
//         listOfRateDateToSaveToDb = GetDateList(latestDateInRateDb);
//     }
//
//     private List<string> GetDateList(string lastDateInDb)
//     {
//         List<string> dateList = new List<string>();
//
//         DateTime startDate = DateTime.Parse(lastDateInDb);
//         DateTime endDate = DateTime.Parse(today);
//
//         for (DateTime date = startDate.AddDays(1); date <= endDate.AddDays(-1); date = date.AddDays(1))
//         {
//             dateList.Add(date.ToString("yyyy-MM-dd"));
//         }
//
//         return dateList;
//     }
//
//     private async Task<bool> SaveMissingChargeDaysToDb(List<string> days)
//     {
//         try
//         {
//             await _httpClient.PostAsJsonAsync("/savemissingchargedaystodb", days);
//             return true;
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine(e);
//             return false;
//         }
//     }
//
//     private async Task<bool> SaveMissingRateDaysToDb(List<string> days)
//     {
//         try
//         {
//             await _httpClient.PostAsJsonAsync("/savemissingratedaystodb", days);
//             return true;
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine(e);
//             return false;
//         }
//     }
//
//     private async Task<bool> SaveDailyCostToDb()
//     {
//         try
//         {
//             foreach (var day in listOfRateDateToSaveToDb)
//             {
//                 var response = await _httpClient.PostAsync($"/savedailyevcost/{day}", null);
//                 if (!response.IsSuccessStatusCode)
//                 {
//                     Console.WriteLine($"Failed to save daily cost for {day}. Status code: {response.StatusCode}");
//                     return false;
//                 }
//             }
//
//             return true;
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine($"An error occurred: {e.Message}");
//             throw;
//         }
//     }
// }