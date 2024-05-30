using InCharge.Shared.DTOs;

namespace InCharge.Shared.Interfaces.Cost;

public interface IEvCostDaily<EvCostDailyDto>
{
    Task<EvCostDailyDto> GetCostForOneDayEvChargeFromDb(string date);
    Task<bool> SaveCostForOneDayEvChargeToDb(string date);
    Task<bool> SaveDailyCostToDbBasedOnOneMonthsDates(string year, string month);
}