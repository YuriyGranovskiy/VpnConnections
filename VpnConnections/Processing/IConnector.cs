using System.Collections.Generic;
using VpnConnections.Connections;

namespace VpnConnections.Processing
{
    public interface IConnector
    {
        IEnumerable<Connection> GetConnections();
        bool Disconnect(Connection connection);
        bool Connect(Connection connection);
        bool CheckConnection(Connection connection);
    }
}