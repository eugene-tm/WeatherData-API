using ClimateDataAPI.Models;
using ClimateDataAPI.Repository;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ClimateDataAPI.Controllers
{
	[EnableCors("GooglePolicy")]
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserRepository _userRepository;

		public UserController(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		/// <summary>
		/// Creates a new user account
		/// </summary>
		/// <param name="apiKey"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint creates a new user. Only users with the access level of "Teacher" can use this endpoint.
		/// Ensure emails are unique. Duplicate emails are not allowed. 
		/// </remarks>
		[HttpPost("CreateUser")]
		public ActionResult PostUser(string apiKey, UserAccount user)
		{
			if (!IsAuthenticated(apiKey, UserRole.Teacher))
			{
				return Unauthorized();
			}

			_userRepository.CreateUser(user);
			return CreatedAtAction("PostUser", null);
		}


		/// <summary>
		/// Delete user account
		/// </summary>
		/// <param name="id"></param>
		/// <param name="apiKey"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint deletes a user account by ID. Only users with the access level of "Teacher" can use this endpoint.
		/// </remarks>
		[HttpDelete("{id}")]
		public ActionResult DeleteUser([FromRoute] string id, [FromHeader] string apiKey)
		{
			try
			{
				if (!IsAuthenticated(apiKey, UserRole.Teacher))
				{
					return Unauthorized();
				}

				var result = _userRepository.DeleteUser(id);
				return result.Success ? Ok(result) : BadRequest(result);
			}

			catch (Exception e)
			{
				return Problem(detail: e.Message, statusCode: 500);
			}
		}

		/// <summary>
		/// Delete users inactive for more than 30 days
		/// </summary>
		/// <param name="apiKey"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint finds all users with a last access date that is older than 30 days.
		/// It deletes all student accounts it finds. 
		/// </remarks>
		[HttpDelete("DeleteInactiveUsers")]
		public ActionResult DeleteInactiveUsers([FromHeader] string apiKey)
		{
			if (!IsAuthenticated(apiKey, UserRole.Teacher))
			{
				return Unauthorized();
			}

			DateTime lastAccess = DateTime.Now.AddDays(-30);

			try
			{
				var result = _userRepository.DeleteManyUsers(lastAccess);
				return result.Success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return Problem(detail: ex.Message, statusCode: 500);
			}
		}

		/// <summary>
		/// Change Account Roles
		/// </summary>
		/// <param name="datafilter"></param>
		/// <param name="newRole"></param>
		/// <returns></returns>
		/// <remarks>
		/// This endpoint allows access levels to be updated in bulk. The user provides a date range and a new role.
		/// All accounts with last access values between the date range will have their "role" field updated to the new role.
		/// </remarks>
		[HttpPatch("UpdateRole")]
		public ActionResult UpdateRole([FromQuery] DataFilter dataFilter, string newRole, string apiKey)
		{
			try
			{
				if (!IsAuthenticated(apiKey, UserRole.Teacher))
				{
					return Unauthorized();
				}

				var result = _userRepository.UpdateRole(dataFilter, newRole);
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
