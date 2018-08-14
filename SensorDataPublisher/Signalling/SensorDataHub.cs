using Microsoft.AspNet.SignalR;
using NLog;
using SensorDataCommon;
using System.Threading.Tasks;

namespace SensorDataPublisher.Signalling
{
    public class SensorDataHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void NotificationReceived(Notification notification)
        {
            Clients.All.notificationReceived(notification);
            Log.Info($"Notified clients of notification from {notification.DeviceId} with text '{notification.Text}'");
        }

        public void SensorDataReceived(SensorData sensorData)
        {
            Clients.All.SensorDataReceived(sensorData);
            Log.Info($"Notified clients of sensorData from {sensorData.DeviceId} with payload '{sensorData.Payload}'");
        }

        public override Task OnConnected()
        {
            Log.Info($"Connection established: {Context.ConnectionId}");
            _connections.Add(Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Log.Info($"Disconnected: {Context.ConnectionId}");
            _connections.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Log.Info($"Reconnected: {Context.ConnectionId}");
            _connections.Add(Context.ConnectionId);

            return base.OnReconnected();
        }
    }
}