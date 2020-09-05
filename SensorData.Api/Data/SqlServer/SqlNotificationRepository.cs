using AutoMapper;
using NLog;
using SensorData.Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SensorData.Api.Data.SqlServer
{
    public class SqlNotificationRepository : INotificationRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        private SensorDataSqlContext db;
        private readonly IMapper mapper;

        public SqlNotificationRepository(SensorDataSqlContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public void AddNotification(NotificationModel notification)
        {
            try
            {
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(notification);
                Validator.ValidateObject(notification, validationContext);
                db.Notification.Add(mapper.Map<SqlNotification>(notification));
                db.SaveChanges();

                notification.Created = DateTime.Now;
            }
            catch (ValidationException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public void Delete(string id, string deviceId)
        {
            SqlNotification notification = db.Notification.Find(id);
            if (notification != null)
            {
                db.Notification.Remove(notification);
                db.SaveChanges();
            }
        }

        public NotificationModel GetById(string id)
        {
            var result =db.Notification.Find(id);
            return mapper.Map<NotificationModel>(result);
        }

        public List<NotificationModel> GetNotifications(int maxResults)
        {
            var results = db.Notification.OrderByDescending(o => o.Id).Take(maxResults);
            return results.Select(item=>mapper.Map<NotificationModel>(item)).ToList();
        }
    }
}
