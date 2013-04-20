using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Xml.Linq;

namespace VpnConnections
{
    
    /// <summary>
    /// ConnectionManager
    /// </summary>
    public class ConnectionManager
    {
        public static IEnumerable<Connection> GetConnections()
        {
            var connDocument = XDocument.Load("Connections.xml");
            var xmlConnections = connDocument.Descendants("Connection").Where(c => c.Element("Name") != null);
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
    
        public static bool DisConnect(Connection connection)
        {
            try
            {
                var arguments = string.Format("{0} /DISCONNECT", connection.Name);

                ExecuteProcessSync("rasdial.exe", arguments);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Connect(Connection connection)
        {
            try
            {
                if (!CheckConnection(connection))
                {
                    var arguments = string.Format("{0} {1} {2} /DOMAIN:{3}", connection.Name, connection.UserName,
                                                  connection.Password, connection.Domain);

                    ExecuteProcessSync("rasdial.exe", arguments);
                }

                var netif = NetworkInterface.GetAllNetworkInterfaces().SingleOrDefault(n => n.Name == connection.Name);
                if(netif != null)
                {
                    IPInterfaceProperties properties = netif.GetIPProperties();

                    var ipAddress = properties.UnicastAddresses.First().Address;

                    foreach (var route in connection.Routes)
                    {
                        string routeArgs = string.Format("add {0} mask {1} {2}", route.NetAddress, route.Mask, ipAddress);
                        ExecuteProcessSync("route.exe", routeArgs);
                    }
                    ExecuteProcessSync("net.exe", "stop dnscache");
                    ExecuteProcessSync("net.exe", "start dnscache");
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void ExecuteProcessSync(string command, string arguments)
        {
            var processStartInfo = new ProcessStartInfo(command, arguments)
                                       {CreateNoWindow = true, UseShellExecute = false};
            var process = Process.Start(processStartInfo);
            if (process != null) process.WaitForExit();
        }

        public static bool CheckConnection(Connection connection)
        {
            var connectedNetIf = NetworkInterface.GetAllNetworkInterfaces().SingleOrDefault(n => n.Name == connection.Name);
            return connectedNetIf != null;
        }
    }
}
