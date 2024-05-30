using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InCharge.DataAccess.Models;
using InCharge.DataAccess.Respositories;
using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Charge;

namespace InCharge.Server.Services;

public class EvChargeServices : IEvChargeHourly<EvChargeHourlyDto>, IEvChargeDaily<EvChargeDailyDto>,
    IEvChargeDaily<bool>, IEvChargeDaily<string>
{
    private readonly MongoDbEvChargeRepository _db;

    public EvChargeServices(MongoDbEvChargeRepository db)
    {
        _db = db;
    }

    static IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    private string _url = configuration.GetSection("Authentication:Url").Value;
    private string Username = configuration.GetSection("Authentication:Username").Value;
    private string Password = configuration.GetSection("Authentication:Password").Value;
    private string _zappi = configuration.GetSection("Authentication:Zappi").Value;
    private Uri _baseUrl = new("https://s18.myenergi.net");

    public async Task<string> GetLatestDateInChargeDb()
    {
        return await _db.GetLatestDateInChargeDb();
    }
    #region use to get data
    // private async Task<decimal> SumDailyGridCharge(List<EvChargeHourlyDto> dtoHours)
    // {
    //     decimal sumGridCharge = 0;
    //     foreach (var hour in dtoHours)
    //     {
    //         sumGridCharge += hour.h1b + hour.h2b + hour.h3b;
    //     }

    //     return Math.Round(sumGridCharge / 3600000m, 2);
    // }

    // private async Task<decimal> SumDailySolarCharge(List<EvChargeHourlyDto> dtoHours)
    // {
    //     decimal sumSolarCharge = 0;
    //     foreach (var hour in dtoHours)
    //     {
    //         sumSolarCharge += hour.h1d + hour.h2d + hour.h3d;
    //     }

    //     return Math.Round(sumSolarCharge / 3600000m, 2);
    // }

    // public async Task<bool> GetOneDayByTheHourAndSaveToDbLoopingAMonth(string date)
    // {
    //     var startDate = new DateTime(2024, 3, 1);
    //     var endDate = new DateTime(2024, 3, DateTime.DaysInMonth(2024, 3));
    //     try
    //     {
    //         for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
    //         {
    //             date = currentDate.ToString("yyyy-MM-dd");

    //             var listofhours = await GetOneDayByTheHourFromMyEnergyAPI(date);
    //             var day = await CreateDayFromHours(listofhours);
    //             await SaveOneDayToDb(await ConvertEvChargeDailyDtoToModel(day));
    //         }

    //         return true;
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         return false;
    //     }
    // }

    #endregion

    public async Task<bool> GetOneDayByTheHourAndSaveToDb(string date)
    {
        try
        {
            var listofhours = await GetOneDayByTheHourFromMyEnergyAPI(date);
            var day = await CreateDayFromHours(listofhours);
            await SaveOneDayToDb(await ConvertEvChargeDailyDtoToModel(day));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> SaveMissingDaysToDb(List<string> days)
    {

        try
        {
            foreach (var d in days)
            {
                var listofhours = await GetOneDayByTheHourFromMyEnergyAPI(d);
                var day = await CreateDayFromHours(listofhours);
                await SaveOneDayToDb(await ConvertEvChargeDailyDtoToModel(day));
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }




    private async Task SaveOneDayToDb(EvChargeDaily day)
    {
        await _db.InsertOneEvChargeDaily(day);
    }

    public async Task<EvChargeDailyDto> GetOneDailyChargeFromDb(string day)
    {
        var model = await _db.GetDailyCharge(day);
        return await ConvertEvChargeDailyModelToDto(model);
    }

    public async Task<EvChargeHourlyDto> GetOneHourlyChargeFromDb(DateTime daytime)
    {
        var model = await _db.GetHourlyCharge(daytime);
        return await ConvertEvChargeHourlyModelToDto(model);
    }


    public async Task<List<EvChargeHourlyDto>> GetOneDayByTheHourFromMyEnergyAPI(string date)
    {
        using (var client = new HttpClient())
        {
            string apiUrl = _url + date;
            // Initial request without credentials
            var initialRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUrl, apiUrl));
            var initialResponse = await client.SendAsync(initialRequest);


            // Get the Digest header value from the response
            var digestHeader = initialResponse.Headers.GetValues("WWW-Authenticate").FirstOrDefault();

            // Parse the digestHeader to get the nonce and other values
            string nonce = "", realm = "", qop = "", opaque = "";
            var digestParts = digestHeader.Split(',');
            foreach (var part in digestParts)
            {
                if (part.Contains("nonce"))
                {
                    nonce = part.Split('=')[1].Trim('\"');
                }
                else if (part.Contains("realm"))
                {
                    realm = part.Split('=')[1].Trim('\"');
                }
                else if (part.Contains("qop"))
                {
                    qop = part.Split('=')[1].Trim('\"');
                }
                else if (part.Contains("opaque"))
                {
                    opaque = part.Split('=')[1].Trim('\"');
                }
            }

            // Compute the HA1 hash
            string ha1 = ComputeHash($"{Username}:{realm}:{Password}");

            // Compute the HA2 hash
            string ha2 = ComputeHash($"GET:{apiUrl}");

            // Compute the response hash
            string responseHash = ComputeHash($"{ha1}:{nonce}:{ha2}");

            // Create the Digest authorization header
            var digestAuthHeader = new AuthenticationHeaderValue("Digest",
                $"username=\"{Username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{apiUrl}\", response=\"{responseHash}\"");

            //Create the final request with Digest authorization header
            var finalRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUrl, apiUrl));
            finalRequest.Headers.Authorization = digestAuthHeader;

            // Send the request with the Digest authorization header
            var finalResponse = await client.SendAsync(finalRequest);
            finalResponse.EnsureSuccessStatusCode();

            // Read the response content as a string
            var responseBody = await finalResponse.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(responseBody);

            var zappiElement = json.RootElement.GetProperty(_zappi);

            var zappiInfoArray = JsonSerializer.Deserialize<List<EvChargeHourlyDto>>(zappiElement.GetRawText());

            var zappiInfoArray2 = PopulateDateTimeOffset(zappiInfoArray);

            return zappiInfoArray2;
        }
    }

    private static string ComputeHash(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    private List<EvChargeHourlyDto> PopulateDateTimeOffset(List<EvChargeHourlyDto> zappiInfoArray)
    {
        foreach (var hour in zappiInfoArray)
        {
            var utcTime = new DateTimeOffset(hour.yr, hour.mon, hour.dom, hour.hr, 0, 0, TimeSpan.Zero);
            hour.LocalTime = utcTime.ToLocalTime();
        }

        return zappiInfoArray;
    }

    public async Task<EvChargeDailyDto> CreateDayFromHours(List<EvChargeHourlyDto> zappiInfoArray)
    {
        var day = new EvChargeDailyDto();
        day.hours = new List<EvChargeHourlyDto>();
        var firstHour = zappiInfoArray.FirstOrDefault();
        day.day = $"{firstHour.yr}-{firstHour.mon:D2}-{firstHour.dom:D2}";
        foreach (var hour in zappiInfoArray)
        {
            day.hours.Add(hour);
        }

        return day;
    }

    public async Task<EvChargeDaily> ConvertEvChargeDailyDtoToModel(EvChargeDailyDto dto)
    {
        var model = new EvChargeDaily
        {
            day = dto.day,
            hours = new List<EvChargeHourly>()
        };
        foreach (var hourDto in dto.hours)
        {
            var hourModel = new EvChargeHourly
            {
                yr = hourDto.yr,
                mon = hourDto.mon,
                dom = hourDto.dom,
                hr = hourDto.hr,
                imp = hourDto.imp,
                exp = hourDto.exp,
                h1b = hourDto.h1b,
                h2b = hourDto.h2b,
                h3b = hourDto.h3b,
                h1d = hourDto.h1d,
                h2d = hourDto.h2d,
                h3d = hourDto.h3d,
                dow = hourDto.dow,
                LocalTime = hourDto.LocalTime
            };

            model.hours.Add(hourModel);
        }

        return model;
    }

    public async Task<EvChargeDailyDto> ConvertEvChargeDailyModelToDto(EvChargeDaily model)
    {
        var dto = new EvChargeDailyDto()
        {
            day = model.day,
            hours = new List<EvChargeHourlyDto>()
        };
        foreach (var hourmodel in model.hours)
        {
            var hourdto = new EvChargeHourlyDto
            {
                yr = hourmodel.yr,
                mon = hourmodel.mon,
                dom = hourmodel.dom,
                hr = hourmodel.hr,
                imp = hourmodel.imp,
                exp = hourmodel.exp,
                h1b = hourmodel.h1b,
                h2b = hourmodel.h2b,
                h3b = hourmodel.h3b,
                h1d = hourmodel.h1d,
                h2d = hourmodel.h2d,
                h3d = hourmodel.h3d,
                dow = hourmodel.dow,
                LocalTime = hourmodel.LocalTime.ToLocalTime()
            };

            dto.hours.Add(hourdto);
        }

        return dto;
    }

    public async Task<EvChargeHourlyDto> ConvertEvChargeHourlyModelToDto(EvChargeHourly model)
    {
        EvChargeHourlyDto dto = new EvChargeHourlyDto
        {
            yr = model.yr,
            mon = model.mon,
            dom = model.dom,
            hr = model.hr,
            imp = model.imp,
            exp = model.exp,
            h1b = model.h1b,
            h2b = model.h2b,
            h3b = model.h3b,
            h1d = model.h1d,
            h2d = model.h2d,
            h3d = model.h3d,
            dow = model.dow,
            LocalTime = model.LocalTime
        };

        return dto;
    }
}