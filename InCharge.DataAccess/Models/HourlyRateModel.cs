namespace InCharge.DataAccess.Models;

public class HourlyRateModel
{
    public double SEK_per_kWh { get; set; }
    public double EUR_per_kWh { get; set; }
    public double EXR { get; set; }
    public DateTime time_start { get; set; }
    public DateTime time_end { get; set; }
}