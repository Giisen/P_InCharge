using InCharge.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InCharge.DataAccess.Contexts
{
    public class MongoDb
    {
        private readonly string _AtlasConnectionString;
        private readonly string _databaseName = "InCharge";
        private readonly string _evChargeDailyCollectionName = "EVChargeDaily";
        private readonly string _hourlyRateCollectionName = "HourlyRate";
        private readonly string _dailyRateCollectionName = "DailyRate";
        private readonly string _dailyCostCollectionName = "DailyCost";
        private readonly string _monthlyCostCollectionName = "MonthyCost";
        private readonly string _yearlyCostCollectionName = "YearlyCost";

        public IMongoCollection<EvChargeDaily> EvChargeDailyCollection { get; set; }
        public IMongoCollection<HourlyRateModel> HourlyRateCollection { get; set; } 
        public IMongoCollection<DailyRateModel> DailyRateCollection { get; set; } 
        public IMongoCollection<EvDailyCostModel> DailyCostCollection { get; set; } 
        public IMongoCollection<EvMonthlyCostModel> MonthlyCostCollection { get; set; } 
        public IMongoCollection<EvYearlyCostModel> YearlyCostCollection { get; set; } 
        public IMongoDatabase db { get; set; }
        public MongoClient MongoClient { get; set; }

        public MongoDb(IConfiguration configuration)
        {
            _AtlasConnectionString = configuration.GetSection("AtlasConnectionString").Value;
            MongoClient = new MongoClient(_AtlasConnectionString);
            db = MongoClient.GetDatabase(_databaseName);
            EvChargeDailyCollection = db.GetCollection<EvChargeDaily>(_evChargeDailyCollectionName);
            HourlyRateCollection = db.GetCollection<HourlyRateModel>(_hourlyRateCollectionName); 
            DailyRateCollection = db.GetCollection<DailyRateModel>(_dailyRateCollectionName); 
            DailyCostCollection = db.GetCollection<EvDailyCostModel>(_dailyCostCollectionName); 
            MonthlyCostCollection = db.GetCollection<EvMonthlyCostModel>(_monthlyCostCollectionName); 
            YearlyCostCollection = db.GetCollection<EvYearlyCostModel>(_yearlyCostCollectionName); 
        }
    }
}