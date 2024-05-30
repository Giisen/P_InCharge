namespace InCharge.Shared.Interfaces;

public interface IDailyRate<T>
{
    Task<List<T>> GetDailyRate(string date);
}