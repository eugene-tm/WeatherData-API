using ClimateDataAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ClimateDataAPI.Repository
{
    public interface IUserRepository
    {
        bool CreateUser(UserAccount user); 
        UserAccount AuthenticateUser(string apiKey, UserRole requiredAccessLevel);
        void UpdateLoginTime(string apiKey, DateTime loginDate);

        public OperationResult<UserAccount>DeleteUser(string id);
        public OperationResult<UserAccount> DeleteManyUsers(DateTime lastAccess);
        public OperationResult<UserAccount> UpdateRole(DataFilter datafilter, string newRole);
    }
}
