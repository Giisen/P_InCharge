using InCharge.Shared.DTOs;

namespace InCharge.Shared.Interfaces.Charge;

public interface IEvChargeDaily<T>
{
    Task<bool> GetOneDayByTheHourAndSaveToDb(string date);
    Task<EvChargeDailyDto> GetOneDailyChargeFromDb(string date);
    Task<string> GetLatestDateInChargeDb();

    Task<bool> SaveMissingDaysToDb(List<string> days);
}