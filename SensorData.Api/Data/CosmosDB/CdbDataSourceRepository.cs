using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using SensorData.Api.Data.SqlServer;
using SensorData.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbDataSourceRepository : IDataSourceRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly IMapper mapper;
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;
        private readonly CosmosClient cosmosClient;
        private readonly Database database;
        private readonly Container container;


        public CdbDataSourceRepository(IConfiguration configuration, IMapper mapper)
        {
            this.mapper = mapper;
            var connectionstring = configuration.GetConnectionString("CosmosConnectionstring");
            cosmosClient = new CosmosClient(connectionstring);
            database = cosmosClient.GetDatabase("denniscosmosdb");
            container = database.GetContainer("SensorData");
        }


        public void blaat()
        {
            var datasourceIterator = container.GetItemQueryIterator<CdbDataSource>(
                queryText: $"select * from c where c.EntityType='DataSource' and c.DataType.Name='PLantHumidity'"
                );

            while (datasourceIterator.HasMoreResults)
            {
                var datasources = datasourceIterator.ReadNextAsync().Result;
                foreach (var item in datasources)
                {
                    container.DeleteItemAsync<CdbDataSource>(item.id, new PartitionKey(item.DeviceId)).Wait();
                    item.DataType.Id = "61";
                    container.CreateItemAsync<CdbDataSource>(item).Wait();
                }

            }


        }
        public void Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<ChannelModel> GetChannels()
        {
            throw new NotImplementedException();
        }

        public DataSourceModel GetDataSource(string id)
        {
            var datasourceQuery = new QueryDefinition($"select * from c where c.EntityType='DataSource' and c.DeviceId='{id}'");

            var datasourceIterator = container.GetItemQueryIterator<CdbDataSource>(datasourceQuery);

            while (datasourceIterator.HasMoreResults)
            {
                var result = datasourceIterator.ReadNextAsync().Result.SingleOrDefault();
                if (result == null) return null;
                return mapper.Map<DataSourceModel>(result);
            }
            return null;
        }

        public List<DataSourceModel> GetDataSources()
        {
            var datasources = new List<DataSourceModel>();
            var datasourceIterator = container.GetItemQueryIterator<CdbDataSource>(queryText: "select * from c where c.EntityType='DataSource'");

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                datasources.AddRange(resultset.Select(item => mapper.Map<DataSourceModel>(item)));
            }
            return datasources;
        }

        public List<DataSourceModel> GetDataSources(string channel)
        {
            var datasources = new List<DataSourceModel>();

            var queryText = "select * from c where c.EntityType='DataSource'";
            if (channel != null)
            {
                queryText += $" and c.DataType.Id='{channel}'";
            }

            var datasourceIterator = container.GetItemQueryIterator<CdbDataSource>(queryText: queryText);

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                datasources.AddRange(resultset.Select(item => mapper.Map<DataSourceModel>(item)));
            }
            return datasources;
        }

        public NewDatasourceModel[] GetNewDatasources()
        {
            var datasources = new List<NewDatasourceModel>();
            //var datasourceQuery = new QueryDefinition("select * from c where c.EntityType='DataSource' and not is_defined(c.Registration)");

            var datasourceIterator = container.GetItemQueryIterator<CdbDataSource>(
                queryText: "select * from c where c.EntityType='DataSource' and not is_defined(c.Registration)");

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                datasources.AddRange(resultset.Select(s => mapper.Map<NewDatasourceModel>(s)));
            }

            foreach (var ds in datasources)
            {
                var iter = container.GetItemQueryIterator<dynamic>(
                    queryText: $"select count(c.TimeStamp) as count, max(c.TimeStamp) as created from c where not is_defined (c.EntityType) and c.DeviceId ='{ds.DeviceId}'");
                while (iter.HasMoreResults)
                {
                    var resultset = iter.ReadNextAsync().Result;
                    var item = resultset.Single();
                    ds.Count = item.count;
                    ds.TimeStamp = item.created;
                }
            }

            return datasources.ToArray();
        }

        private NewDatasourceModel[] Methode1()
        {
            double charge = 0;
            var result = new List<NewDatasourceModel>();

            var datasources = new List<string>();
            var datasourceQuery = new QueryDefinition("select value c.DeviceId from c where c.EntityType='DataSource'");

            var datasourceIterator = container.GetItemQueryIterator<string>(datasourceQuery);

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                charge += resultset.RequestCharge;
                datasources.AddRange(resultset.ToArray());
            }

            var knownDatasources = String.Join(',', datasources);
            var query = new QueryDefinition($"select c.DeviceId, count(c.DeviceId) as Count, max(c.TimeStamp) as TimeStamp from c where c.DeviceId not in ('{knownDatasources}') group by c.DeviceId");
            var sensorDataIterator = container.GetItemQueryIterator<NewDatasourceModel>(
                queryDefinition: query
                );

            while (sensorDataIterator.HasMoreResults)
            {
                var resultset = sensorDataIterator.ReadNextAsync().Result;
                charge += resultset.RequestCharge;
                result.AddRange(resultset);
            }

            Debug.WriteLine($"Method1 request charge: {charge}");
            return result.ToArray();
        }

        private NewDatasourceModel[] Methode2()
        {
            double charge = 0;
            var result = new List<NewDatasourceModel>();
            var datasources = new List<string>();
            var iterator = container.GetItemQueryIterator<string>(
                queryText: "select distinct value c.DeviceId from c"
                );

            while (iterator.HasMoreResults)
            {
                var resultset = iterator.ReadNextAsync().Result;
                charge += resultset.RequestCharge;
                datasources.AddRange(resultset.Select(s => s).ToArray());
            }

            Debug.WriteLine($"{datasources.Count} items found");

            var newiterator = container.GetItemQueryIterator<string>(queryText: "select value c.DeviceId from c where IS_DEFINED(c.EntityType)");

            var registeredDatasources = new List<string>();
            while (newiterator.HasMoreResults)
            {
                var resultset = newiterator.ReadNextAsync().Result;
                charge += resultset.RequestCharge;
                registeredDatasources.AddRange(resultset.ToArray());
            }

            var unregistered = datasources.Except(registeredDatasources);

            foreach (string datasource in unregistered)
            {
                var query = new QueryDefinition($"select c.DeviceId, count(c.DeviceId) as Count, max(c.TimeStamp) as TimeStamp from c where c.DeviceId ='{datasource}' group by c.DeviceId");
                var sensorDataIterator = container.GetItemQueryIterator<NewDatasourceModel>(queryDefinition: query);
                while (sensorDataIterator.HasMoreResults)
                {
                    var resultSet = sensorDataIterator.ReadNextAsync().Result;
                    charge += resultSet.RequestCharge;
                    result.AddRange(resultSet);
                }
            }

            Debug.WriteLine($"Method2 request charge: {charge}");
            return result.ToArray();
        }

        public void UploadImage(string id, IFormFile postedFile)
        {
            throw new NotImplementedException();
        }

        public void Add(DataSourceModel dataSource)
        {
            var item = mapper.Map<CdbDataSource>(dataSource);
            ItemResponse<CdbDataSource> response = container.CreateItemAsync(item, new PartitionKey(dataSource.DeviceId)).Result;
        }

        public void Add(string deviceId)
        {
            ItemResponse<CdbDataSource> response = container.CreateItemAsync(new CdbDataSource
            {
                DeviceId = deviceId,
                id = Guid.NewGuid().ToString()
            }, new PartitionKey(deviceId)).Result;
        }
    }
}
