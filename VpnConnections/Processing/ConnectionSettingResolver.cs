using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using VpnConnections.Connections;

namespace VpnConnections.Processing
{

    public class ConnectionSettingResolver
    {
        public IEnumerable<Connection> GetConnections(XDocument settingsDocument)
        {
            var xmlConnections = settingsDocument.Descendants("Connection").Where(c => c.Element("Name") != null);
            var connections = new List<Connection>();
            foreach (var connectionElement in xmlConnections)
            {
                var connection = new Connection
                                     {
                                         Name = GetSafeElementValue(connectionElement, "Name"),
                                         UserName = GetSafeElementValue(connectionElement, "UserName"),
                                         Password = GetSafeElementValue(connectionElement, "Password"),
                                         Domain = GetSafeElementValue(connectionElement, "Domain")
                                     };

                var xmlRoutes = connectionElement.Element("Routes");
                if (xmlRoutes != null)
                {
                    connection.Routes =
                        xmlRoutes.Elements("Route").Select(
                            e => new Route
                            {
                                NetAddress = GetSafeElementValue(e, "NetAddress"),
                                Mask = GetSafeElementValue(e, "Mask")
                            });
                }

                connections.Add(connection);
            }

            return connections;
        }

        private static string GetSafeElementValue(XContainer element, string elementName)
        {
            var namedElement = element.Element(elementName);
            return namedElement != null ? namedElement.Value : string.Empty;
        }
    }
}
