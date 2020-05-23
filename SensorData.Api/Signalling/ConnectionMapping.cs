using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SensorDataApi.Signalling
{
    public class ConnectionMapping<T>
    {
        private readonly static Logger Log = LogManager.GetCurrentClassLogger();
        private readonly List<string> _connections = new List<string>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(string connectionId)
        {
            lock (_connections)
            {
                _connections.Add(connectionId);
                LogConnections();
            }
        }

        public void Remove(string connectionId)
        {
            lock (_connections)
            {
                _connections.Remove(connectionId);
                LogConnections();
            }
        }

        private void LogConnections()
        {
            Log.Info($"Active connections ({_connections.Count}):");
            foreach (var conn in _connections)
            {
                Log.Info($"connection: {conn}");
            }
        }
    }
}