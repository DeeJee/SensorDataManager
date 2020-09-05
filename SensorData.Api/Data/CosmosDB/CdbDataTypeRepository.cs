using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using SensorData.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbDataTypeRepository : CdbRepository, IDataTypeRepository
    {
        //private readonly IMapper mapper;
        //private readonly CosmosClient cosmosClient;
        //private readonly Database database;
        //private readonly Container container;

        public CdbDataTypeRepository(IConfiguration configuration, IMapper mapper):base(configuration, mapper)
        {
            //this.mapper = mapper;
            //var connectionstring = configuration.GetConnectionString("CosmosConnectionstring");
            //cosmosClient = new CosmosClient(connectionstring);
            //database = cosmosClient.GetDatabase("denniscosmosdb");
            //container = database.GetContainer("SensorData");
        }
        public void AddDataType(DataTypeModel dataType)
        {
            throw new NotImplementedException();
        }

        public void DeleteDataType(int id)
        {
            throw new NotImplementedException();
        }

        public DataTypeModel GetDataType(int id)
        {
            return GetDataTypes().SingleOrDefault(item => item.Id == id);
        }

        public IEnumerable<DataTypeModel> GetDataTypes()
        {
            var datasources = new List<DataTypeModel>();
            var datasourceIterator = container.GetItemQueryIterator<CdbDataType>(
                queryText: $"select distinct value c.DataType from c where c.EntityType='DataSource' and is_defined(c.DataType)");

            while (datasourceIterator.HasMoreResults)
            {
                var resultset = datasourceIterator.ReadNextAsync().Result;
                datasources.AddRange(resultset.Select(item => mapper.Map<DataTypeModel>(item)));
            }
            return datasources;



            var items = new List<DataTypeModel>();
            items.Add(new DataTypeModel { Id = 59, Name = "LightIntensity", Properties = "Voltage,RSSI,Intensity" });
            items.Add(new DataTypeModel { Id = 60, Name = "TempHum", Properties = "Temperature,Humidity,RSSI" });
            items.Add(new DataTypeModel { Id = 61, Name = "PLantHumidity", Properties = "SoilMoisture,RSSI" });
            items.Add(new DataTypeModel { Id = 63, Name = "gfgf", Properties = "Intensity,Voltage,RSSI" });
            items.Add(new DataTypeModel { Id = 71, Name = "calibrate", Properties = "Voltage" });

            return items;
        }

        public void UpdateDataType(DataTypeModel dataType)
        {
            throw new NotImplementedException();
        }
    }
}
