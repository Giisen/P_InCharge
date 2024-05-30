using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Models;


namespace InCharge.DataAccess.Respositories;

public class MongoDbHourlyRateRepository
{
    private readonly MongoDb _db;

    public MongoDbHourlyRateRepository(MongoDb db)
    {
        _db = db;
    }
    
    public async Task InsertManyHourlyRate(List<HourlyRateModel> HourlyList)
    {
        await _db.HourlyRateCollection.InsertManyAsync(HourlyList);
    }
}