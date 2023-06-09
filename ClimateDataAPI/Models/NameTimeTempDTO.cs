namespace ClimateDataAPI.Models
{
    public class NameTimeTempDTO
    {
        public string DeviceName { get; set; }
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
    }
}
