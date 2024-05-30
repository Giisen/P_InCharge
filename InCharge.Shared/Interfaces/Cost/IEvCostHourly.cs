using InCharge.Shared.DTOs;

namespace InCharge.Shared.Interfaces;

public interface IEvCostHourly<EvChargeHourlyDto>
{
    Task<EvChargeHourlyDto> CalculateCostForOneHourEvChargeFromDb(DateTime date);
    
}