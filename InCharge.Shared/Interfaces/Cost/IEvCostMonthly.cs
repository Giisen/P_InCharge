namespace InCharge.Shared.Interfaces;

public interface IEvCostMonthly<T>
{
    //Task<T> CalculateCostForOneMonthEvCharge(string year, string month);
    Task<T> GetCostForOneMonthEvCharge(string year, string month);
    Task<bool> SaveCostForOneMonthEvChargeToDb(string year, string month);
}