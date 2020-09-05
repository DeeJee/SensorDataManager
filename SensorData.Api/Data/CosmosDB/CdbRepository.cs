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
    public class CdbRepository 
    {
        protected readonly IMapper mapper;
        protected readonly CosmosClient cosmosClient;
        protected readonly Database database;
        protected readonly Container container;

        public CdbRepository(IConfiguration configuration, IMapper mapper)
        {
            this.mapper = mapper;
            var connectionstring = configuration.GetConnectionString("CosmosConnectionstring");
            cosmosClient = new CosmosClient(connectionstring);
            database = cosmosClient.GetDatabase("denniscosmosdb");
            container = database.GetContainer("SensorData");
        }
    }
}
