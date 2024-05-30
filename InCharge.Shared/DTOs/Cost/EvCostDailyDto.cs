namespace InCharge.Shared.DTOs;

public class EvCostDailyDto
{
    public string Date { get; set; }
    public decimal Cost { get; set; }
    public decimal GridCharge { get; set; }
    public decimal SolarCharge { get; set; }
    
}