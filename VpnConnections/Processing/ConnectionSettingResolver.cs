using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using VpnConnections.Connections;

namespace VpnConnections.Processing
{

    public class ConnectionSettingResolver
    {
        public IEnumerable<Connection>  GetConnections(XDocument settingsDocument)
        {
            var xmlConnections = settingsDocument.Descendants("Connection").Where(c => c.Element("Name") != null);
            var connections = new List<Connection>();
            foreach (var connectionElement in xmlConnections)
            {
                var connection = new Connection();
                connection.Name = connectionElement.Element("Name").Value;
                connection.UserName = connectionElement.Element("UserName").Value;
                connection.Password = connectionElement.Element("Password").Value;
                connection.Domain = connectionElement.Element("Domain").Value;
                var xmlRoutes = connectionElement.Element("Routes");
                if (xmlRoutes != null)
                {
                    connection.Routes =
                        xmlRoutes.Elements("Route").Select(
                            e => new Route
                            {
                                NetAddress = e.Element("NetAddress") != null ? e.Element("NetAddress").Value : string.Empty,
                                Mask = e.Element("Mask") != null ? e.Element("Mask").Value : string.Empty
                            });
                }

                connections.Add(connection);
            }

            return connections;
        }
    }
}
