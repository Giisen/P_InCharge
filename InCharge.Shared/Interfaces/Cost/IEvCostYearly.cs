using InCharge.Shared.DTOs;

namespace InCharge.Shared.Interfaces.Cost;

public interface IEvCostYearly<T>
{
    Task<EvCostYearlyDto> CalculateCostForOneYearEvCharge(string year);
    Task<EvCostYearlyDto> GetCostForOneYearEvCharge(string year);
    Task<bool> SaveCostForOneYearEvChargeToDb(string year);
}