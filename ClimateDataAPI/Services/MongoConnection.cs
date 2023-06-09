using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ClimateDataAPI.Settings;

namespace ClimateDataAPI.Services
{
    public class MongoConnection
    {
        private readonly IOptions<DefaultMongoConnection> _options;

        public MongoConnection(IOptions<DefaultMongoConnection> options)
        {
            _options = options;
        }

        public IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(_options.Value.ConnectionString);
            return client.GetDatabase(_options.Value.DatabaseName);
        }

        public IMongoDatabase GetDatabase(string databaseName)
        {
            var client = new MongoClient(_options.Value.ConnectionString);
            return client.GetDatabase(databaseName);
        }

        public IMongoDatabase GetDatabase(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(databaseName);
        }
    }
}
