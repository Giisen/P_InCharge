﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InCharge.Shared.DTOs;

public class EvChargeDailyDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string day { get; set; }
    public List<EvChargeHourlyDto> hours { get; set; }
}