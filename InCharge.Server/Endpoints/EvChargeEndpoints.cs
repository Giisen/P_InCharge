using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Charge;

namespace InCharge.Server.Endpoints;

public static class EvChargeEndpoints
{
    public static WebApplication MapEvChargeEndpoints(this WebApplication app)
    {
        app.MapGet("/dayhour/{date}", async (IEvChargeHourly<EvChargeHourlyDto> repo, string date) =>
        {
            var response = await repo.GetOneDayByTheHourFromMyEnergyAPI(date);
            return response;
        });
        
        app.MapGet("/dailycharge/{date}", async (IEvChargeDaily<EvChargeDailyDto> repo, string date) =>
        {
            var response = await repo.GetOneDailyChargeFromDb(date);
            return response;
        });
        
        app.MapGet("/latestdateinchargedb", async (IEvChargeDaily<string> repo) =>
        {
            var response = await repo.GetLatestDateInChargeDb();
            return response;
        });
        
        app.MapGet("/dailychargetodb/{date}", async (IEvChargeDaily<bool> repo, string date) =>
        {
            var response = await repo.GetOneDayByTheHourAndSaveToDb(date);
            return response;
        });
        
        app.MapPost("/savemissingchargedaystodb", async (IEvChargeDaily<bool> repo, List<string> dates) =>
        {
            var response = await repo.SaveMissingDaysToDb(dates);
            return response;
        });
        
        return app;
    }
}