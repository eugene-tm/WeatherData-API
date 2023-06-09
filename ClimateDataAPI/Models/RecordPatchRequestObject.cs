namespace ClimateDataAPI.Models
{
    public class RecordPatchRequestObject
    {
        public DataFilter Filter { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }
}
