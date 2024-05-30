namespace InCharge.Shared.DTOs;

public class EvCostYearlyDto
{
    public string Year { get; set; }
    public decimal Cost { get; set; }
    public decimal GridCharge { get; set; }
    public decimal SolarCharge { get; set; }
}