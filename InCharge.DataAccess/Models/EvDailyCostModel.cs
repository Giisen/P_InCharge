using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InCharge.DataAccess.Models;

public class EvDailyCostModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string Date { get; set; }
    public decimal Cost { get; set; }
    public decimal GridCharge { get; set; }
    public decimal SolarCharge { get; set; }
}