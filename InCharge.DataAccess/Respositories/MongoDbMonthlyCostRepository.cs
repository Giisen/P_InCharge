using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Models;
using MongoDB.Driver;

namespace InCharge.DataAccess.Respositories;

public class MongoDbMonthlyCostRepository
{
    private readonly MongoDb _db;

    public MongoDbMonthlyCostRepository(MongoDb db)
    {
        _db = db;
    }
    public async Task InsertOneMonthlyCost(EvMonthlyCostModel date)
    {
        await _db.MonthlyCostCollection.InsertOneAsync(date);
    }
    public async Task UpDateOneMonthlyCost(EvMonthlyCostModel date)
    {
        var year = date.Date.Substring(0, 4);
        var month = date.Date.Substring(5, 2);
        var yearMonth = $"{year}-{month}";

        var filter = Builders<EvMonthlyCostModel>.Filter.Eq("Date", yearMonth);
        await _db.MonthlyCostCollection.DeleteOneAsync(filter);
        await InsertOneMonthlyCost(date);
    }
    
    public async Task<EvMonthlyCostModel> GetMonthlyCost(string year, string month)
    {
        var yearMonth = $"{year}-{month}";
        var filter = Builders<EvMonthlyCostModel>.Filter.Where(x => x.Date == yearMonth);
        var result = await _db.MonthlyCostCollection.Find(filter).FirstOrDefaultAsync();
        return result;
    }
    
}