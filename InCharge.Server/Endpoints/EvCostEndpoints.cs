﻿using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Cost;

namespace InCharge.Server.Endpoints;

public static class EvCostEndpoints
{
    public static WebApplication MapEvCostEndpoints(this WebApplication app)
    {
        app.MapGet("/hourlyevcost/{date}", async (IEvCostHourly<EvCostHourlyDto> repo, DateTime date) =>
        {
            var response = await repo.CalculateCostForOneHourEvChargeFromDb(date);
            return response;
        });
        
        app.MapGet("/dailyevcost/{date}", async (IEvCostDaily<EvCostDailyDto> repo, string date) =>
        {
            //var response = await repo.GetCostForOneDayEvCharge(date);
            var response = await repo.GetCostForOneDayEvChargeFromDb(date);
            return response;
        });
        
        app.MapGet("/monthlyevcost/{year}/{month}", async (IEvCostMonthly<EvCostMonthlyDto> repo, string year, string month) =>
        {
            // var response = await repo.CalculateCostForOneMonthEvCharge(year, month);
            var response = await repo.GetCostForOneMonthEvCharge(year, month);
            return response;
        });
        app.MapGet("/yearlyevcost/{year}", async (IEvCostYearly<EvCostYearlyDto> repo, string year) =>
        {
            //var response = await repo.CalculateCostForOneYearEvCharge(year);
            var response = await repo.GetCostForOneYearEvCharge(year);
            return response;
        });
        
        app.MapPost("/savedailyevcost/{date}", async (IEvCostDaily<EvCostDailyDto> repo, string date) =>
        {
            var response = await repo.SaveCostForOneDayEvChargeToDb(date);
            return response;
        });
        
        app.MapPost("/savedailyevcostforonemonth/{year}/{month}", async (IEvCostDaily<EvCostDailyDto> repo, string year, string month) =>
        {
            var response = await repo.SaveDailyCostToDbBasedOnOneMonthsDates(year, month);
            return response;
        });
        
        app.MapPost("/savemonthlyevcost/{year}/{month}", async (IEvCostMonthly<EvCostMonthlyDto> repo, string year, string month) =>
        {
            var response = await repo.SaveCostForOneMonthEvChargeToDb(year, month);
            return response;
        });
        
        app.MapPost("/saveyearlyevcost/{year}", async (IEvCostYearly<EvCostYearlyDto> repo, string year) =>
        {
            var response = await repo.SaveCostForOneYearEvChargeToDb(year);
            return response;
        });
        
        return app;
    }
    
}