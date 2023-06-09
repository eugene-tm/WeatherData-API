using ClimateDataAPI.Models;
using System.Linq;
namespace ClimateDataAPI.Repository
{
    // It's smart to update the IClimateRepository always, BEFORE updating the mongoNoteRepository

    // 
    public interface IClimateRepository
    {
        // public IEnumerable<ClimateReading> GetRecords(DataFilter filter);
        public ClimateReading GetRecordById(string id);
        public void CreateRecord(RecordCreatedDTO newRecord);
        public OperationResult<ClimateReading> UpdateRecord(string id, ClimateReading climateReading);
        public void CreateManyRecords(List<RecordCreatedDTO> records, string deviceName);
        public ClimateReading GetMaxPrecipitation();
        public ClimateReading GetValuesByTimeAndPlace(DateTime time, string deviceName);
        public IEnumerable<NameTimeTempDTO> GetMaxTemperatures(DataFilter dataFilter);
        public OperationResult<ClimateReading> UpdatePrecipitation(string id, double precipitation);

    }
}
