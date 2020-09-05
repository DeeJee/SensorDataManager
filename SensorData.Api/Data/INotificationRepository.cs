using SensorData.Api.Data.SqlServer;
using SensorData.Api.Models;
using System.Collections.Generic;

namespace SensorData.Api.Data
{
    public interface INotificationRepository
    {
        List<NotificationModel> GetNotifications(int maxResults);
        NotificationModel GetById(string id);
        void AddNotification(NotificationModel notification);
        void Delete(string id, string deviceId);
    }
}
