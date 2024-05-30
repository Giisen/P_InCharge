using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Rate;

namespace InCharge.Server.Endpoints;

public static class RateEndpoints
{
    public static WebApplication MapRateEndpointExtensions(this WebApplication app)
    {
        app.MapGet("/hourlyrate/{date}", async (IRate<HourlyRateDto> repo, string date) =>
        {
            var response = await repo.GetAndSaveHourlyRate(date);
            return response;
        });
        
        app.MapGet("/dailyrate/{date}", async (IRate<DailyRateDto> repo, string date) =>
        {
            var response = await repo.GetDailyRateFromDb(date);
            return response;
        });
        
        app.MapGet("/currentrate", async (IRate<CurrentRateDto> repo) =>
        {
            var response = await repo.GetCurrentRate();
            return response;
        });
        
        app.MapGet("/alldailyrates", async (IRate<List<DailyRateDto>> repo) =>
        {
            var response = await repo.GetAllDailyRates();
            return response;
        });
        
        app.MapGet("/latestdateinratedb", async (IRate<DailyRateDto> repo) =>
        {
            var response = await repo.GetLatestDateInRateDb();
            return response;
        });
        
        app.MapPost("/savemissingratedaystodb", async (IRate<bool> repo, List<string> dates) =>
        {
            var response = await repo.SaveMissingDaysToDb(dates);
            return response;
        });

        
        return app;
    }
}