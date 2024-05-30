using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Models;
using MongoDB.Driver;

namespace InCharge.DataAccess.Respositories;

public class MongoDbYearlyCostRepository
{
    private readonly MongoDb _db;

    public MongoDbYearlyCostRepository(MongoDb db)
    {
        _db = db;
    }
    
    public async Task InsertOneYearlyCost(EvYearlyCostModel year)
    {
        await _db.YearlyCostCollection.InsertOneAsync(year);
    }
    
    public async Task UpDateOneYearlyCost(EvYearlyCostModel date)
    {
        var year = date.Year;
        var filter = Builders<EvYearlyCostModel>.Filter.Eq("Year", year);
        await _db.YearlyCostCollection.DeleteOneAsync(filter);
        await InsertOneYearlyCost(date);
    }
    
    public async Task<EvYearlyCostModel> GetYearlyCost(string year)
    {
        var filter = Builders<EvYearlyCostModel>.Filter.Where(x => x.Year == year);
        var result = await _db.YearlyCostCollection.Find(filter).FirstOrDefaultAsync();
        return result;
    }
}