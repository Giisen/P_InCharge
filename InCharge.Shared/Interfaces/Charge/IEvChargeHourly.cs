namespace InCharge.Shared.Interfaces;

public interface IEvChargeHourly<T>
{
    Task<List<T>> GetOneDayByTheHourFromMyEnergyAPI(string date);
}