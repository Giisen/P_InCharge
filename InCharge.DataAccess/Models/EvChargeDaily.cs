using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InCharge.DataAccess.Models;

public class EvChargeDaily
{
   
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string day { get; set; }
        public List<EvChargeHourly> hours { get; set; }
    
}