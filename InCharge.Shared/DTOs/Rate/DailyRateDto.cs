using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InCharge.Shared.DTOs;

public class DailyRateDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string day { get; set; }
    public List<HourlyRateDto> hours { get; set; }
}