using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InCharge.DataAccess.Models;

public class EvChargeHourly
{
    public int yr { get; set; }
    public int mon { get; set; }
    public int dom { get; set; }
    public int hr { get; set; } = 0;
    public int? imp { get; set; }
    public int? exp { get; set; }
    public int h1b { get; set; } = 0;
    public int h2b { get; set; } = 0;
    public int h3b { get; set; } = 0;
    public int h1d { get; set; } = 0;
    public int h2d { get; set; } = 0;
    public int h3d { get; set; } = 0;
    public string dow { get; set; }
    
    [BsonRepresentation(BsonType.DateTime)]
    public DateTimeOffset LocalTime { get; set; }
}