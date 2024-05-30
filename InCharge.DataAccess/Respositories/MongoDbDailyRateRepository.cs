using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Models;
using MongoDB.Driver;

namespace InCharge.DataAccess.Respositories;

public class MongoDbDailyRateRepository
{
    private readonly MongoDb _db;

    public MongoDbDailyRateRepository(MongoDb db)
    {
        _db = db;
    }

    public async Task<string> GetLatestDateInChargeDb()
    {
        var date = "";
        var filter = Builders<DailyRateModel>.Filter.Empty;
        var sort = Builders<DailyRateModel>.Sort.Descending("day");
        var result = await _db.DailyRateCollection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        return result.day;
    }

    public async Task InsertDailyRate(DailyRateModel day)
    {
        await _db.DailyRateCollection.InsertOneAsync(day);
    }


    public async Task<DailyRateModel> GetDailyRate(string date)
    {
        var result = new DailyRateModel();
        var filter = Builders<DailyRateModel>.Filter.Where(x => x.day == date);
        result = await _db.DailyRateCollection.Find(filter).FirstOrDefaultAsync();
        return result;
    }


    public async Task<List<DailyRateModel>> GetAllDailyRates()
    {
        var result = new List<DailyRateModel>();
        result = await _db.DailyRateCollection.Find(_ => true).ToListAsync();
        return result;
    }




}