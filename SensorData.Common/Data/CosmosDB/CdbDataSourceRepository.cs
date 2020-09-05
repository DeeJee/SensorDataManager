using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MySensorData.Common.Data;
using Newtonsoft.Json.Linq;
using NLog;
using SensorDataCommon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MySensorData.Common.Data.CosmosDB
{
    public class CdbDataSourceRepository : ICdbDataSourceRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly IMapper mapper;
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;
        Database database;
        Container container;

        CosmosClient cosmosClient;

        public CdbDataSourceRepository(IConfiguration configuration, IMapper mapper)
        {
            this.mapper = mapper;
            var connectionstring = configuration.GetConnectionString("CosmosConnectionstring");
            cosmosClient = new CosmosClient(connectionstring);
            database = cosmosClient.GetDatabase("denniscosmosdb");
            container = database.GetContainer("SensorData");
        }
        public void Add(DataSource dataSource)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<ChannelViewModel> GetChannels()
        {
            throw new NotImplementedException();
        }

        public DataSource GetDataSource(string id)
        {
            var datasourceQuery = new QueryDefinition($"select * from c where c.EntityType='DataSource' and c.DeviceId='{id}'");

            var datasourceIterator = container.GetItemQueryIterator<DataSource>(datasourceQuery);

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                return resultset.SingleOrDefault();
            }
            return null;
        }

        public List<DataSource> GetDataSources()
        {
            var datasources = new List<DataSource>();
            var datasourceIterator = container.GetItemQueryIterator<DataSource>(queryText: "select * from c where c.EntityType='DataSource'");

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                datasources.AddRange(resultset.ToArray());
            }
            return datasources;
        }

        public List<DataSource> GetDataSources(string channel)
        {
            var datasources = new List<DataSource>();
            var datasourceIterator = container.GetItemQueryIterator<DataSource>(queryText: "select * from c where c.EntityType='DataSource'");

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                datasources.AddRange(resultset.ToArray());
            }
            return datasources;
        }

        public List<NewDatasource> NewDatasources()
        {



            var datasources = new List<NewDatasource>();
            //var datasourceQuery = new QueryDefinition("select * from c where c.EntityType='DataSource' and not is_defined(c.Registration)");

            var datasourceIterator = container.GetItemQueryIterator<DataSource>(queryText: "select * from c where c.EntityType='DataSource' and not is_defined(c.Registration)");

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                foreach (var item in resultset)
                {
                    var aap = mapper.Map<NewDatasource>(item);
                }

                datasources.AddRange(resultset.Select(s => mapper.Map<NewDatasource>(s)));
            }

            return datasources;
        }

        private NewDatasource[] Methode1()
        {
            double charge = 0;
            var result = new List<NewDatasource>();

            var datasources = new List<string>();
            var datasourceQuery = new QueryDefinition("select value c.DeviceId from c where c.EntityType='DataSource'");

            var datasourceIterator = container.GetItemQueryIterator<string>(datasourceQuery);

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                charge += resultset.RequestCharge;
                datasources.AddRange(resultset.ToArray());
            }

            var knownDatasources = string.Join(',', datasources);
            var query = new QueryDefinition($"select c.DeviceId, count(c.DeviceId) as Count, max(c.TimeStamp) as TimeStamp from c where c.DeviceId not in ('{knownDatasources}') group by c.DeviceId");
            var sensorDataIterator = container.GetItemQueryIterator<NewDatasource>(
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

        private NewDatasource[] Methode2()
        {
            double charge = 0;
            var result = new List<NewDatasource>();
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
                var sensorDataIterator = container.GetItemQueryIterator<NewDatasource>(queryDefinition: query);
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

        public void Add(DataSource dataSource)
        {
            ItemResponse<DataSource> response = container.CreateItemAsync(dataSource, new PartitionKey(dataSource.DeviceId)).Result;
        }
    }
}
