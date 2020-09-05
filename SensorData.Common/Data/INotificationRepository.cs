using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySensorData.Common.Data
{
    public interface INotificationRepository
    {
        List<Notification> GetNotifications(int maxResults);
        Notification GetById(int id);
        void AddNotification(Notification notification);
        void Delete(int id);
    }
}
