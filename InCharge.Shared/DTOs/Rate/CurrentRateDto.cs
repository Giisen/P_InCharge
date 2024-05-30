namespace InCharge.Shared.DTOs
{
    public class CurrentRateDto
    {
        public List<HourlyRateDto> hourRate { get; set; }
        
        public CurrentRateDto()
        {
            hourRate = new List<HourlyRateDto>();
        }
    }
}