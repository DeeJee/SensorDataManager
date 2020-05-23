using Microsoft.AspNetCore.SignalR;
using NLog;
using MySensorData.Common.Data;
using SensorDataApi.Signalling;
using System.Threading.Tasks;
using System;

namespace SensorDataApi
{
    public class SensorDataHub : Hub
    {
        //private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        //private static Logger Log = LogManager.GetCurrentClassLogger();

        //public void NotificationReceived(Notification notification)
        //{
        //    Clients.All.SendAsync("Notification", notification);
        //    Log.Info($"Notified clients of notification from {notification.DeviceId} with text '{notification.Text}'");
        //}

        //public void SensorDataReceived(MySensorData.Common.Data.SensorData sensorData)
        //{
        //    Clients.All.SendAsync("SensorData", sensorData);
        //    Log.Info($"Notified clients of sensorData from {sensorData.DeviceId} with payload '{sensorData.Payload}'");
        //}

        //public override Task OnConnectedAsync()
        //{
        //    Log.Info($"Connection established: {Context.ConnectionId}");
        //    _connections.Add(Context.ConnectionId);

        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception exception)
        //{
        //    Log.Info($"Disconnected: {Context.ConnectionId}");
        //    _connections.Remove(Context.ConnectionId);

        //    return base.OnDisconnectedAsync(exception);
        //}
    }
}