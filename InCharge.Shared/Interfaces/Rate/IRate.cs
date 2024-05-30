using InCharge.Shared.DTOs;

namespace InCharge.Shared.Interfaces.Rate;

public interface IRate<T>
{
    Task<List<HourlyRateDto>> GetAndSaveHourlyRate(string date);
    Task<DailyRateDto> GetDailyRateFromDb(string date);
    Task<List<DailyRateDto>> GetAllDailyRates();
    Task<string> GetLatestDateInRateDb();
    Task<bool> SaveMissingDaysToDb(List<string> days);

    Task<CurrentRateDto> GetCurrentRate();

}