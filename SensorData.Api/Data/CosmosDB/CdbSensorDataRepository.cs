using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using NLog;
using SensorData.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbSensorDataRepository : CdbRepository, ISensorDataRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;

        public CdbSensorDataRepository(IConfiguration configuration, IMapper mapper) : base(configuration, mapper)
        {
        }

        public IEnumerable<SensorDataModel> Get(string deviceId, string vanDatum, string totDatum)
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

            return results.Select(item => mapper.Map<SensorDataModel>(item)).ToList();
        }

        public IEnumerable<SensorDataModel> Get()
        {
            var result = GetSensorData($"select top 1000 * from c", null);

            return result.Select(s=>mapper.Map<SensorDataModel>(s)).AsEnumerable();
        }

        public SensorDataModel MostRecent(string id)
        {
            var item = GetSensorData($"select top 1 * from c where c.DeviceId='{id}' and not is_defined(c.EntityType) order by c.DeviceId desc", id).SingleOrDefault();
            if (item != null)
            {
                item.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(item.TimeStamp, info);
            }

            return mapper.Map<SensorDataModel>(item);
        }

        public void blaat()
        {
            var items = new List<CdbDataSource>();
            items.Add(new CdbDataSource { DeviceId = "1C" });
            items.Add(new CdbDataSource { DeviceId = "4F" });
            items.Add(new CdbDataSource { DeviceId = "72" });
            items.Add(new CdbDataSource { DeviceId = "85" });
            items.Add(new CdbDataSource { DeviceId = "9C" });
            items.Add(new CdbDataSource { DeviceId = "A8" });
            items.Add(new CdbDataSource { DeviceId = "E9" });
            ///items.Add(new KnownDataSource { Name = "signalR" });

            var container = database.GetContainer("SensorData");

            foreach (var item in items)
            {
                item.id = Guid.NewGuid().ToString();
                ItemResponse<CdbDataSource> response = container.CreateItemAsync(item, new PartitionKey(item.DeviceId)).Result;
            }
        }

        public IEnumerable<SensorDataModel> MostRecent()
        {
            var items = new List<CdbSensorData>();
            var queryresultIterator = container.GetItemQueryIterator<CdbSensorData>(
                queryText: "select top 50 * from c where not is_defined(c.EntityType) order by c.TimeStamp desc");
            while (queryresultIterator.HasMoreResults)
            {
                var resultset = queryresultIterator.ReadNextAsync().Result;
                Debug.WriteLine($"RequestCharge: {resultset.RequestCharge}");
                items.AddRange(resultset);
            }

            var results = new List<SensorDataModel>();
            foreach (var group in items.GroupBy(g => g.DeviceId))
            {
                var max = group.OrderBy(o => o.TimeStamp).First();
                results.Add(mapper.Map<SensorDataModel>(max));
            }

            return results;
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

        public void PostSensorData(Models.SensorDataModel sensorData)
        {
            var item = new CdbSensorData
            {
                Id = Guid.NewGuid().ToString(),
                DeviceId = sensorData.DeviceId,
                TimeStamp = DateTime.Now,
                Payload = sensorData.Payload.ToString()
            };
            var container = database.GetContainer("SensorData");
            ItemResponse<CdbSensorData> response = container.CreateItemAsync<CdbSensorData>(item, new PartitionKey(item.DeviceId)).Result;

        }

        private List<CdbSensorData> GetSensorData(string queryText, string deviceId)
        {
            var results = new List<CdbSensorData>();
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
            var queryresultIterator = container.GetItemQueryIterator<CdbSensorData>(
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


        private IList<CdbDataSource> GetDataSource(string queryText)
        {
            var results = new List<CdbDataSource>();
            var query = new QueryDefinition(queryText);
            var container = database.GetContainer("DataSource");
            var queryresultIterator = container.GetItemQueryIterator<CdbDataSource>(query);
            while (queryresultIterator.HasMoreResults)
            {
                var resultset = queryresultIterator.ReadNextAsync().Result;
                results.AddRange(resultset);
            }
            return results;
        }
    }
}
