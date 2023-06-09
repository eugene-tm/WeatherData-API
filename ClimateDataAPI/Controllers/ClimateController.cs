using ClimateDataAPI.Models;
using ClimateDataAPI.Repository;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Linq;

namespace ClimateDataAPI.Controllers
{
	[EnableCors("GooglePolicy")]
	[Route("api/[controller]")]
	[ApiController]
	public class ClimateController : ControllerBase
	{
		private readonly IClimateRepository _climateRepository;
		private readonly IUserRepository _userRepository;

		public ClimateController(IClimateRepository climateRepository, IUserRepository userRepository)
		{
			_climateRepository = climateRepository;
			_userRepository = userRepository;
		}


		/// <summary>
		/// Get Records by ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint retrieves a single document with the matching ID value entered. 
		/// </remarks>
		[HttpGet("{id}")]
		public ActionResult<ClimateReading> GetRecordById(string id)
		{
			var record = _climateRepository.GetRecordById(id);
			if (record == null)
			{
				return NotFound();
			}
			return Ok(record);
		}

		/// <summary>
		/// Get Highest Precipitation in last five months
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint returns the document that contains the highest precipitation value of the last 5 months.
		/// It doesn't retreieve the entire document, just device name, time, temp, and precipitation.
		/// </remarks>
		[HttpGet("GetMaxPrecipitation")]
		public IActionResult GetMaxPrecipitation()
		{
			var maxPrecipitation = _climateRepository.GetMaxPrecipitation();

			if (maxPrecipitation == null)
			{
				return NotFound();
			}

			var result = new
			{
				DeviceName = maxPrecipitation.DeviceName,
				Time = maxPrecipitation.Time,
				Temperature = maxPrecipitation.Temperature,
				Precipitation = maxPrecipitation.Precipitation
			};

			return Ok(result);
		}

		/// <summary>
		/// Get Values By DateTime and Location
		/// </summary>
		/// <param name="time"></param>
		/// <param name="deviceName"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint returns temperature, atmospheric pressure, solar raditiation, and precipitation values
		/// based on the device name and time input provided
		/// </remarks>
		[HttpGet("GetValuesByTimeAndPlace")]
		public IActionResult GetValuesByTimeAndPlace(DateTime time, string deviceName)
		{
			var maxTemp = _climateRepository.GetValuesByTimeAndPlace(time, deviceName);

			if (maxTemp == null)
			{
				return NotFound();
			}

			var result = new
			{
				Temperature = maxTemp.Temperature,
				AtmosphericPressure = maxTemp.AtmosphericPressure,
				SolarRadiation = maxTemp.SolarRadiation,
				Precipitation = maxTemp.Precipitation
			};

			return Ok(result);
		}

		/// <summary>
		/// Find highest temperatures for all sensors
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint finds the highest temperature reading of the last four months for each sensor.
		/// It returns the device name, time and temperature values for each sensor. 
		/// </remarks>
		[HttpGet("GetMaxTemperatures")]
		public IEnumerable<NameTimeTempDTO> GetMaxTemperatures([FromQuery] DataFilter filter)
		{

			var maxTemps = _climateRepository.GetMaxTemperatures(filter);

			return maxTemps;
		}



		/// <summary>
		/// Post a new record
		/// </summary>
		/// <param name="newRecord"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint allows users to insert new entries into the database.
		/// </remarks>
		[HttpPost]
		public ActionResult PostRecord([FromBody] RecordCreatedDTO newRecord, string apiKey)
		{
			try
			{
				if (!IsAuthenticated(apiKey, UserRole.Teacher))
				{
					return Unauthorized();
				}

				_climateRepository.CreateRecord(newRecord);
				return CreatedAtAction("PostRecord", null);
			}
			catch (Exception ex)
			{
				return Problem(detail: ex.Message, statusCode: 500);
			}
		}


		/// <summary>
		/// Post a batch of records
		/// </summary>
		/// <param name="newRecordList"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint allows for records to be entered in bulk.
		/// </remarks>
		[HttpPost]
		[Route("CreateManyRecords")]
		public ActionResult PostManyRecords([FromBody] List<RecordCreatedDTO> newRecordList, string deviceName, string apiKey)
		{
			try
			{
				if (!IsAuthenticated(apiKey, UserRole.Teacher))
				{
					return Unauthorized();
				}


				_climateRepository.CreateManyRecords(newRecordList, deviceName);
				return CreatedAtAction("PostManyRecords", null);
			}
			catch (Exception ex)
			{
				return Problem(detail: ex.Message, statusCode: 500);
			}
		}


		/// <summary>
		/// Alter an existing record (Rewrite the document)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="climateReading"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint takes an existing record by Id, and rewrites the entire document to whatever
		/// information the user gives it. 
		/// </remarks>
		[HttpPut("{id}")]
		public ActionResult UpdateRecord(string id, [FromBody] ClimateReading climateReading, string apiKey)
		{
			try
			{
				if (!IsAuthenticated(apiKey, UserRole.Teacher))
				{
					return Unauthorized();
				}

				var result = _climateRepository.UpdateRecord(id, climateReading);
				return result.Success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return Problem(detail: ex.Message, statusCode: 500);
			}
		}

		/// <summary>
		/// Alter precipitation value by Id
		/// </summary>
		/// <param name="id"></param>
		/// <param name="precipitation"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint allows users to retrieve a record by Id and alter the precipitation value.
		/// </remarks>
		[HttpPatch("{id}")]
		public ActionResult PatchPrecipitation(string id, [FromForm] double precipitation, string apiKey)
		{
			try
			{
				if (!IsAuthenticated(apiKey, UserRole.Teacher))
				{
					return Unauthorized();
				}

				var result = _climateRepository.UpdatePrecipitation(id, precipitation);
				return result.Success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return Problem(detail: ex.Message, statusCode: 500);
			}
		}

		private bool IsAuthenticated(string apiKey, UserRole role)
		{
			if (_userRepository.AuthenticateUser(apiKey, role) == null)
			{
				return false;
			}
			_userRepository.UpdateLoginTime(apiKey, DateTime.UtcNow);
			return true;
		}

	}
}
