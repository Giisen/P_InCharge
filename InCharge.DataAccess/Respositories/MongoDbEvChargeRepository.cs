using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Models;
using MongoDB.Driver;

namespace InCharge.DataAccess.Respositories;

public class MongoDbEvChargeRepository
{
    private readonly MongoDb _db;

    public MongoDbEvChargeRepository(MongoDb db)
    {
        _db = db;
    }

    public async Task InsertOneEvChargeDaily(EvChargeDaily evChargeDaily)
    {
        await _db.EvChargeDailyCollection.InsertOneAsync(evChargeDaily);
    }
   
    public async Task<EvChargeDaily> GetDailyCharge(string date)
    {
        var result = new EvChargeDaily();
        var filter = Builders<EvChargeDaily>.Filter.Where(x => x.day == date);
        result = await _db.EvChargeDailyCollection.Find(filter).FirstOrDefaultAsync();
        return result;
    }

    public async Task<EvChargeHourly> GetHourlyCharge(DateTime daytime)
    {
        var date = daytime.Date.ToString("yyyy-MM-dd");
        var result = new EvChargeDaily();
        var filter = Builders<EvChargeDaily>.Filter.Where(x => x.day == date);
        result = await _db.EvChargeDailyCollection.Find(filter).FirstOrDefaultAsync();
        var hour = result.hours.FirstOrDefault(x => x.LocalTime == daytime);
        return hour;
    }
    
    public async Task<string> GetLatestDateInChargeDb()
    {
        var filter = Builders<EvChargeDaily>.Filter.Empty;
        var sort = Builders<EvChargeDaily>.Sort.Descending("day");
        var result =  await _db.EvChargeDailyCollection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        string latestDate = result.day;
        return latestDate;
    }

}