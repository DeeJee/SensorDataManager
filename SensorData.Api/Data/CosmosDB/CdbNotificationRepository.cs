using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using NLog;
using SensorData.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbNotificationRepository : CdbRepository, INotificationRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public CdbNotificationRepository(IConfiguration configuration, IMapper mapper) : base(configuration, mapper)
        {
        }

        public void AddNotification(NotificationModel notification)
        {
            var item = new CdbNotification
            {
                Id = Guid.NewGuid().ToString(),
                DeviceId = notification.DeviceId,
                Created = DateTime.Now,
                LogLevel = notification.LogLevel,
                Text = notification.Text
            };
            var container = database.GetContainer("SensorData");
            var response = container.CreateItemAsync<CdbNotification>(item, new PartitionKey(item.DeviceId)).Result;
        }

        public void Delete(string id, string deviceId)
        {
            var container = database.GetContainer("SensorData");
            var queryresultIterator = container.DeleteItemAsync<CdbNotification>(id, new PartitionKey(deviceId));
            var resultset = queryresultIterator.Result;            
        }

        public NotificationModel GetById(string id)
        {
            var container = database.GetContainer("SensorData");
            var queryresultIterator = container.GetItemQueryIterator<CdbNotification>(
                queryText: $"select * from c where c.EntityType='Notification' and c.id='{id}'"
                );
            var resultset = queryresultIterator.ReadNextAsync().Result;
            var item = resultset.SingleOrDefault();
            if (item == null) return null;
            return mapper.Map<NotificationModel>(item);
        }

        public List<NotificationModel> GetNotifications(int maxResults)
        {
            var results = new List<NotificationModel>();
            var container = database.GetContainer("SensorData");
            var queryresultIterator = container.GetItemQueryIterator<CdbNotification>(
                queryText: "select * from c where c.EntityType='Notification'"
                );
            while (queryresultIterator.HasMoreResults)
            {
                var resultset = queryresultIterator.ReadNextAsync().Result;
                results.AddRange(resultset.Select(s => mapper.Map<NotificationModel>(s)));
            }
            return results;
        }
    }
}
