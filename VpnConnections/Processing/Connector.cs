﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml.Linq;
using NLog;
using VpnConnections.Connections;

namespace VpnConnections.Processing
{
    /// <summary>
    /// ConnectionManager
    /// </summary>
    public class Connector : IConnector
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly ConnectionSettingResolver SettingResolver = new ConnectionSettingResolver();

        public Encoding OutputEncoding { get; set; }
        
        public IEnumerable<Connection> GetConnections()
        {
            var connDocument = XDocument.Load("Connections.xml");
            var connections = SettingResolver.GetConnections(connDocument);
            return connections;
        }
    
        public bool Disconnect(Connection connection)
        {
            try
            {
                var arguments = string.Format("{0} /DISCONNECT", connection.Name);

                ExecuteProcessSync("rasdial.exe", arguments);
                Logger.Info(string.Format("Connection '{0}' disconnected successfully", connection.Name));
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Connection '{0}' failed to disconnect successfully", connection.Name), ex);
                return false;
            }
        }

        public bool Connect(Connection connection)
        {
            try
            {
                if (!CheckConnection(connection))
                {
                    var arguments = new StringBuilder();
                    arguments.AppendFormat("{0} {1} {2}", connection.Name, connection.UserName,
                                                  connection.Password);

                    if (!string.IsNullOrWhiteSpace(connection.Domain))
                    {
                        arguments.AppendFormat(" /DOMAIN:{0}", connection.Domain);
                    }
                     
                    ExecuteProcessSync("rasdial.exe", arguments.ToString());
                }

                var netif = NetworkInterface.GetAllNetworkInterfaces().SingleOrDefault(n => n.Name == connection.Name);
                if (netif != null)
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
                else
                {
                    Logger.Error(string.Format("Connection '{0}' failed to connect", connection.Name));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Connection '{0}' failed to connect", connection.Name), ex);
                return false;
            }

            Logger.Info(string.Format("Connection '{0}' connected successfully", connection.Name));

            return true;
        }

        public bool CheckConnection(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException();
            }

            var connectedNetIf = NetworkInterface.GetAllNetworkInterfaces().SingleOrDefault(n => n.Name == connection.Name);
            return connectedNetIf != null;
        }

        private void ExecuteProcessSync(string command, string arguments)
        {
            var processStartInfo = new ProcessStartInfo(command, arguments)
                                       {
                                           CreateNoWindow = true, 
                                           UseShellExecute = false, 
                                           RedirectStandardError = true,
                                           RedirectStandardOutput = true
                                       };

            if (OutputEncoding != null)
            {
                processStartInfo.StandardOutputEncoding = OutputEncoding;
            }
            

            var process = Process.Start(processStartInfo);

            process.ErrorDataReceived += (sender, errorLine) =>
                                             {
                                                 if (errorLine.Data != null)
                                                 {
                                                     Logger.Error(errorLine.Data);
                                                 }
                                             };

            process.OutputDataReceived += (sender, outputLine) =>
            {
                if (outputLine.Data != null)
                {
                    Logger.Debug(outputLine.Data);
                }
            };

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
        }
    }
}
