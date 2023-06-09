using ClimateDataAPI.Models;
using ClimateDataAPI.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;

namespace ClimateDataAPI.Repository
{
	public class MongoUserRepository : IUserRepository
	{
		private readonly IMongoCollection<UserAccount> _users;

		public MongoUserRepository(MongoConnection connection)
		{
			_users = connection.GetDatabase().GetCollection<UserAccount>("UserAccounts");
		}

		#region POST AND PUT REQUESTS


		public bool CreateUser(UserAccount user)
		{
			// creates a fiter using input email address
			var filter = Builders<UserAccount>.Filter.Eq(c => c.Email, user.Email);

			// either returns a matching email or creates a new user
			var existingUser = _users.Find(filter).FirstOrDefault();

			// if existing user is found --> back out
			if (existingUser != null)
			{
				return false;
			}

			// sets an API key for the created user
			user.ApiKey = Guid.NewGuid().ToString();

			// updates their last accessed value
			user.LastAccess = DateTime.UtcNow;

			// inserts the user to the database and returns true
			_users.InsertOne(user);
			return true;
		}

		// This method is used in updating the lastAccess field with each successful login
		public void UpdateLoginTime(string apiKey, DateTime loginDate)
		{
			// finds the user
			var filter = Builders<UserAccount>.Filter.Eq(c => c.ApiKey, apiKey);

			// changes the last access date
			var update = Builders<UserAccount>.Update.Set(c => c.LastAccess, loginDate);

			// submits the updated info
			var result = _users.UpdateOne(filter, update);
		}


		public OperationResult<UserAccount> UpdateRole(DataFilter datafilter, string newRole)
		{
			var filter = ProcessFilter(datafilter);

			var result = _users.UpdateMany(filter, Builders<UserAccount>.Update.Set(c => c.Role, newRole));

			if (result.ModifiedCount >= 1)
			{
				return new OperationResult<UserAccount>
				{
					Message = "Authorisation level(s) updated successfully",
					Success = true,
					RecordsAffected = Convert.ToInt32(result.ModifiedCount)
				};
			}
			else
			{
				return new OperationResult<UserAccount>
				{
					Message = "No changes",
					Success = false,
					RecordsAffected = 0
				};
			}
		}


		#endregion

		#region DELETE REQUESTS

		public OperationResult<UserAccount> DeleteUser(string id)
		{
			// converts hashed ID value to readable ID
			ObjectId objId = ObjectId.Parse(id);

			// filters and finds matching ID value
			var filter = Builders<UserAccount>.Filter.Eq(c => c._id, objId);

			// deletes record
			var result = _users.DeleteOne(filter);

			if (result.DeletedCount == 1)
			{
				return new OperationResult<UserAccount>
				{
					Message = "Account deleted successfully",
					Success = true,
					RecordsAffected = Convert.ToInt32(result.DeletedCount)
				};
			}
			else
			{
				return new OperationResult<UserAccount>
				{
					Message = "No accounts deleted",
					Success = false,
					RecordsAffected = 0
				};
			}
		}

		public OperationResult<UserAccount> DeleteManyUsers(DateTime lastAccess)
		{
			var userFilter = Builders<UserAccount>.Filter.Lt(c => c.LastAccess, lastAccess);
			userFilter = Builders<UserAccount>.Filter.Eq(c => c.Role, "Student");

			var result = _users.DeleteMany(userFilter);

			if (result.DeletedCount >= 1)
			{
				return new OperationResult<UserAccount>
				{
					Message = "Accounts deleted successfully",
					Success = true,
					RecordsAffected = Convert.ToInt32(result.DeletedCount)
				};
			}
			else
			{
				return new OperationResult<UserAccount>
				{
					Message = "No accounts deleted",
					Success = false,
					RecordsAffected = 0
				};
			}
		}

		#endregion


		public UserAccount AuthenticateUser(string apiKey, UserRole requiredAccessLevel)
		{
			// creating a filter to search for user with a specific API key
			var filter = Builders<UserAccount>.Filter.Eq(c => c.ApiKey, apiKey);

			// searches using the filter
			var user = _users.Find(filter).FirstOrDefault();

			// user not suitable or NULL
			if (user == null || !IsSuitableRole(user.Role, requiredAccessLevel))
			{
				return null;
			}

			return user;
		}

		// DETERMINES THE LEVEL OF AUTHORISATION OF A USER
		private bool IsSuitableRole(string userRole, UserRole requiredRole)
		{
			// The TryParse will result to false if there is no matching role in the enum for the users' roles
			if (!Enum.TryParse(userRole, out UserRole userRoleNumber))
			{
				// No role matches the user's role - not authorised
				return false;
			}

			// extract the integer value associated with the enum string
			int userRoleIndicator = (int)userRoleNumber;

			// extract the integer value associated with the enum string
			int requiredRoleIndicator = (int)requiredRole;

			// if the user's enum number value is less than or equal to the required value
			// then the user will be authenticated

			return userRoleIndicator <= requiredRoleIndicator;
		}

		private FilterDefinition<UserAccount> ProcessFilter(DataFilter dataFilter)
		{
			// this line defines a filterDefinitionBuilder 
			var builder = Builders<UserAccount>.Filter;

			// creates an empty filter that will return all results -- the FIND()  method requires a filter
			var filter = builder.Empty;

			if (dataFilter?.CreatedFrom != null)
			{
				filter &= builder.Gte(c => c.LastAccess, dataFilter.CreatedFrom.Value);
			}

			if (dataFilter?.CreatedTo != null)
			{
				filter &= builder.Lte(c => c.LastAccess, dataFilter.CreatedTo.Value);
			}

			return filter;
		}

	}
}
