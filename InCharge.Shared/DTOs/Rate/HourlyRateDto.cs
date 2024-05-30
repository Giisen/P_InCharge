namespace InCharge.Shared.DTOs;

public class HourlyRateDto
{
    public double SEK_per_kWh { get; set; }
    public double EUR_per_kWh { get; set; }
    public double EXR { get; set; }
    public DateTime time_start { get; set; }
    public DateTime time_end { get; set; }
}