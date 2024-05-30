using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Models;
using MongoDB.Driver;

namespace InCharge.DataAccess.Respositories;

public class MongoDbDailyCostRepository
{
    private readonly MongoDb _db;

    public MongoDbDailyCostRepository(MongoDb db)
    {
        _db = db;
    }

    public async Task InsertDailyCost(EvDailyCostModel day)
    {
        await _db.DailyCostCollection.InsertOneAsync(day);
    }

    public async Task<EvDailyCostModel> GetDailyCost(string day)
    {
        var filter = Builders<EvDailyCostModel>.Filter.Where(x => x.Date == day);
        var result = await _db.DailyCostCollection.Find(filter).FirstOrDefaultAsync();
        return result;
    }

    
    public async Task<List<EvDailyCostModel>> GetDatesInMonthFromCostDb(string year, string month)
    {
        var listOfDates = new List<EvDailyCostModel>();
        var filter = Builders<EvDailyCostModel>.Filter.Where(x=>x.Date.Contains($"{year}-{month}"));

        var result = await _db.DailyCostCollection.Find(filter).ToListAsync();
        foreach (var day in result)
        {
            listOfDates.Add(day);
        }

        return listOfDates;
    }
}