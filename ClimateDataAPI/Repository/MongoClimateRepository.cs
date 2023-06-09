using ClimateDataAPI.Models;
using ClimateDataAPI.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ClimateDataAPI.Repository
{
	public class MongoClimateRepository : IClimateRepository
	{

		private readonly IMongoCollection<ClimateReading> _climateReadings;

		// connecting to the climatedata collection in the database
		public MongoClimateRepository(MongoConnection connection)
		{
			_climateReadings = connection.GetDatabase().GetCollection<ClimateReading>("ClimateData");
		}

		#region GET REQUESTS

		public ClimateReading GetRecordById(string id)
		{
			// converts the hashed id to a readable id
			ObjectId objId = ObjectId.Parse(id);

			// filters for the searched id
			var filter = Builders<ClimateReading>.Filter.Eq(c => c._id, objId);

			// returns the results
			return _climateReadings.Find(filter).FirstOrDefault();
		}

		public ClimateReading GetMaxPrecipitation()
		{
			// Calculate the date five months ago from the current date
			var fiveMonthsAgo = DateTime.UtcNow.AddMonths(-50);

			// Retrieve the climate reading with the highest precipitation from the last 5 months
			var maxPrecip = _climateReadings
				.Find(c => c.Time >= fiveMonthsAgo)
				.SortByDescending(c => c.Precipitation)
				.FirstOrDefault();

			return maxPrecip;
		}

		public ClimateReading GetValuesByTimeAndPlace(DateTime time, string deviceName)
		{
			var searchRecord = _climateReadings
				.Find(c => c.DeviceName == deviceName && c.Time == time)
				.FirstOrDefault();
			return searchRecord;
		}

		public IEnumerable<NameTimeTempDTO> GetMaxTemperatures(DataFilter dataFilter)
		{
			var filter = ProcessFilter(dataFilter);
			var dataResults = _climateReadings.Aggregate().Match(filter).SortByDescending(c => c.Temperature).Group(c => c.DeviceName, c => new NameTimeTempDTO
			{
				DeviceName = c.Key,
				Time = c.First().Time,
				Temperature = c.First().Temperature
			});

			return dataResults.ToList();
		}


		#endregion

		#region POST & PUT REQUESTS

		public void CreateRecord(RecordCreatedDTO newRecord)
		{
			_climateReadings.InsertOne(new ClimateReading
			{
				DeviceName = newRecord.DeviceName,
				Precipitation = newRecord.Precipitation,
				Time = DateTime.UtcNow,
				Latitude = newRecord.Latitude,
				Longitude = newRecord.Longitude,
				Temperature = newRecord.Temperature,
				AtmosphericPressure = newRecord.AtmosphericPressure,
				MaxWindSpeed = newRecord.MaxWindSpeed,
				SolarRadiation = newRecord.SolarRadiation,
				VaporPressure = newRecord.VaporPressure,
				Humidity = newRecord.Humidity,
				WindDirection = newRecord.WindDirection
			}); ;
		}

		public OperationResult<ClimateReading> UpdateRecord(string id, ClimateReading updatedReading)
		{
			// converts the hashed id to a readable id
			ObjectId objId = ObjectId.Parse(id);

			// filters for the searched id
			var filter = Builders<ClimateReading>.Filter.Eq(c => c._id, objId);

			// this is an important line that assigns the old object ID to the replaced input
			// without this, the next object will have an id of 0000000000000000000000000 and will error
			updatedReading._id = objId;

			// replaces the selected 
			var result = _climateReadings.ReplaceOne(filter, updatedReading);

			if (result.ModifiedCount == 1)
			{
				// Using our Operation Result object
				return new OperationResult<ClimateReading>
				{
					Message = "Record replaced successfully",
					Success = true,
					RecordsAffected = Convert.ToInt32(result.ModifiedCount)
				};
			}
			else
			{
				return new OperationResult<ClimateReading>
				{
					Message = "No records replaced",
					Success = false,
					RecordsAffected = 0
				};
			}
		}

		public void CreateManyRecords(List<RecordCreatedDTO> records, string deviceName)
		{
			var recordList = records.Select(c => new ClimateReading
			{
				DeviceName = deviceName,
				Precipitation = c.Precipitation,
				Time = c.Time,
				Latitude = c.Latitude,
				Longitude = c.Longitude,
				Temperature = c.Temperature,
				AtmosphericPressure = c.AtmosphericPressure,
				MaxWindSpeed = c.MaxWindSpeed,
				SolarRadiation = c.SolarRadiation,
				VaporPressure = c.VaporPressure,
				Humidity = c.Humidity,
				WindDirection = c.WindDirection
			});
			_climateReadings.InsertMany(recordList);
		}


		public OperationResult<ClimateReading> UpdatePrecipitation(string id, double precipitation)
		{
			try
			{
				var filter = Builders<ClimateReading>.Filter.Eq("_id", new ObjectId(id));
				var update = Builders<ClimateReading>.Update.Set("Precipitation mm/h", precipitation);

				var result = _climateReadings.UpdateOne(filter, update);

				if (result.ModifiedCount == 1)
				{
					return new OperationResult<ClimateReading>
					{
						Message = "Precipitation value updated successfully",
						Success = true,
						RecordsAffected = Convert.ToInt32(result.ModifiedCount)
					};
				}
				else
				{
					return new OperationResult<ClimateReading>
					{
						Message = "No values updated",
						Success = false,
						RecordsAffected = 0
					};
				}
			}
			catch (Exception ex)
			{
				return new OperationResult<ClimateReading>
				{
					Message = ex.Message,
					Success = false,
					RecordsAffected = 0
				};
			}
		}



		#endregion



		private FilterDefinition<ClimateReading> ProcessFilter(DataFilter dataFilter)
		{
			// this line defines a filterDefinitionBuilder 
			var builder = Builders<ClimateReading>.Filter;

			// creates an empty filter that will return all results -- the FIND()  method requires a filter
			var filter = builder.Empty;

			if (dataFilter?.CreatedFrom != null)
			{
				filter &= builder.Gte(c => c.Time, dataFilter.CreatedFrom.Value);
			}

			if (dataFilter?.CreatedTo != null)
			{
				filter &= builder.Lte(c => c.Time, dataFilter.CreatedTo.Value);
			}

			return filter;
		}
	}
}
