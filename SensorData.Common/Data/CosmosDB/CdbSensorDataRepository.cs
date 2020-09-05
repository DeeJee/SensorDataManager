using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using NLog;
using SensorDataCommon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MySensorData.Common.Data.CosmosDB
{
    public class CdbSensorDataRepository : ICdbSensorDataRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;
        //private SensorDataSqlContext db;
        Database database;

        CosmosClient cosmosClient;

        public CdbSensorDataRepository(IConfiguration configuration)
        {
            var connectionstring = configuration.GetConnectionString("CosmosConnectionstring");
            cosmosClient = new CosmosClient(connectionstring);
            database = cosmosClient.GetDatabase("denniscosmosdb");
        }

        public IList<SensorData> Get(string deviceId, string vanDatum, string totDatum)
        {
            var queryText = $"select * from c where c.DeviceId='{deviceId}'";

            string vanDateTime;
            if (string.IsNullOrEmpty(vanDatum))
            {
                vanDateTime = DateTime.Now.Date.ToString("s", CultureInfo.InvariantCulture);
            }
            else
            {
                vanDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(vanDatum, "yyyy-MM-dd", nederland), info).ToString("s", CultureInfo.InvariantCulture);
            }

            string totDateTime;
            if (string.IsNullOrEmpty(totDatum))
            {
                totDateTime = DateTime.Now.Date.AddDays(1).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
            }
            else
            {
                totDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(totDatum, "yyyy-MM-dd", nederland), info).ToString("s", CultureInfo.InvariantCulture);
            }
            queryText = queryText += $" and c.TimeStamp >= '{vanDateTime}' and c.TimeStamp < '{totDateTime}'";

            //execute query
            var results = GetSensorData(queryText, deviceId);
            foreach (var result in results)
            {
                result.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(result.TimeStamp, info);
            }

            logger.Info($"{results.Count()} items retrieved");
            return results;
        }

        public IList<SensorData> Get()
        {
            return GetSensorData($"select top 1000 * from c", null);
        }

        public SensorData MostRecent(string id)
        {
            var data = GetSensorData($"select top 1 * from c where c.DeviceId='{id}' order by c.DeviceId asc", id).SingleOrDefault();

            //var data = db.SensorData.Where(w => w.DeviceId == id).OrderByDescending(o => o.Id).Take(1).SingleOrDefault();
            if (data != null)
            {
                data.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(data.TimeStamp, info);
            }
            return data;
        }

        public void blaat()
        {
            var items = new List<DataSource>();
            items.Add(new DataSource { DeviceId = "1C" });
            items.Add(new DataSource { DeviceId = "4F" });
            items.Add(new DataSource { DeviceId = "72" });
            items.Add(new DataSource { DeviceId = "85" });
            items.Add(new DataSource { DeviceId = "9C" });
            items.Add(new DataSource { DeviceId = "A8" });
            items.Add(new DataSource { DeviceId = "E9" });
            ///items.Add(new KnownDataSource { Name = "signalR" });

            var container = database.GetContainer("SensorData");

            foreach (var item in items)
            {
                item.id = Guid.NewGuid().ToString();
                ItemResponse<DataSource> response = container.CreateItemAsync(item, new PartitionKey(item.DeviceId)).Result;
            }
        }

        private object List<T>()
        {
            throw new NotImplementedException();
        }

        public IList<SensorData> MostRecent()
        {
            var data = GetSensorData("select top 50 * from c order by c.TimeStamp", null);
            var resultset = new List<SensorData>();
            foreach (var group in data.GroupBy(g => g.DeviceId))
            {
                var max = group.OrderByDescending(o => o.TimeStamp).First();
                resultset.Add(max);
            }
            return resultset;
        }

        public int Count(string id)
        {
            var query = new QueryDefinition($"SELECT value count(1) FROM c where c.DeviceId='{id}'");
            var container = database.GetContainer("SensorData");
            var queryresultIterator = container.GetItemQueryIterator<int>(query);
            var resultset = queryresultIterator.ReadNextAsync().Result;
            return resultset.Single();
        }

        public void DeleteSensorData(string deviceId)
        {
            //TODO: omschrijven naar efficiente code
            //db.SensorData.Where(w => w.DeviceId == deviceId).Delete();
            //var toBeDeleted = db.SensorData.Where(w => w.DeviceId == deviceId);
            //db.SensorData.RemoveRange(toBeDeleted);
            //db.SaveChanges();
        }

        public void PostSensorData(DataModel sensorData)
        {
            var item = new SensorData
            {
                Id = Guid.NewGuid().ToString(),
                DeviceId = sensorData.DeviceId,
                TimeStamp = DateTime.Now,
                Payload = sensorData.Payload.ToString()
            };
            var container = database.GetContainer("SensorData");
            ItemResponse<SensorData> response = container.CreateItemAsync(item, new PartitionKey(item.DeviceId)).Result;

        }

        private List<SensorData> GetSensorData(string queryText, string deviceId)
        {
            var results = new List<SensorData>();
            var container = database.GetContainer("SensorData");

            QueryRequestOptions requestOptions;
            if (deviceId == null)
            {
                requestOptions = new QueryRequestOptions();
            }
            else
            {
                requestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(deviceId) };
            }
            var queryresultIterator = container.GetItemQueryIterator<SensorData>(
                queryText: queryText,
                requestOptions: requestOptions);
            while (queryresultIterator.HasMoreResults)
            {
                var resultset = queryresultIterator.ReadNextAsync().Result;
                Debug.WriteLine($"RequestCharge: {resultset.RequestCharge}");
                results.AddRange(resultset);
            }
            return results;
        }


        private IList<DataSource> GetDataSource(string queryText)
        {
            var results = new List<DataSource>();
            var query = new QueryDefinition(queryText);
            var container = database.GetContainer("DataSource");
            var queryresultIterator = container.GetItemQueryIterator<DataSource>(query);
            while (queryresultIterator.HasMoreResults)
            {
                var resultset = queryresultIterator.ReadNextAsync().Result;
                results.AddRange(resultset);
            }
            return results;
        }
    }
}
