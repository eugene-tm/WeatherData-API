namespace ClimateDataAPI.Models
{
    public class OperationResult<T>
    {
        public string Message { get; set; } = "";
        public bool Success { get; set; } = true;
        public T? Value { get; set; }
        public int RecordsAffected { get; set; }

    }
}
