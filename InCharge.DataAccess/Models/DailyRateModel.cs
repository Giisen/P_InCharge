using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InCharge.DataAccess.Models;

public class DailyRateModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string day { get; set; }
    public List<HourlyRateModel> hours { get; set; }
}