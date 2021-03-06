﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NLog;
using VpnConnections.Connections;
using VpnConnections.Resources;

namespace VpnConnections.Processing
{
    public class MenuBuilder : IUiBuilder
    {
        private const string ConnectItemName = "Connect";
        private const string DisconnectItemName = "Disconnect";
        private const string ExitItemName = "Exit";
        private const string CodepageKeyName = "Codepage";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private NotifyIcon _notifyIcon;
        
        private IEnumerable<Connection> _connections;

        private readonly IConnector _connector;
        
        public event EventHandler Exit;

        public MenuBuilder()
        {
            Encoding encoding = null;
            if (ConfigurationManager.AppSettings.AllKeys.Contains(CodepageKeyName))
            {
                var codepageValue = ConfigurationManager.AppSettings[CodepageKeyName];
                int codepage;
                int.TryParse(codepageValue, out codepage);
                if (codepage > 0)
                {
                    encoding = Encoding.GetEncoding(codepage);
                }
            }

            _connector = new Connector {OutputEncoding = encoding};
        }

        public void Build()
        {
            _connections = _connector.GetConnections();
            var icon = new Icon(Properties.Resources.pnidui_3048, new Size(16, 16));
            _notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Visible = true
            };

            List<MenuItem> menuItems = _connections.Select(GetMenuItemForConnection).ToList();
            var exitMenuItem = new MenuItem(MenuItems.Exit) { Name = ExitItemName };
            exitMenuItem.Click += (o, args) => ExitThread();
            menuItems.Add(exitMenuItem);
            _notifyIcon.ContextMenu = new ContextMenu(menuItems.ToArray());
        }

        private void ExitThread()
        {
            _notifyIcon.Visible = false;

            if (Exit != null)
                Exit(this, null);
        }

        private MenuItem GetMenuItemForConnection(Connection connection)
        {
            var menuItem = new MenuItem
            {
                Name = connection.Name,
                Text = connection.Name
            };

            menuItem.Popup += MenuItemPopup;
            var connectMenuItem = new MenuItem(MenuItems.Connect) { Name = ConnectItemName };
            connectMenuItem.Click += ConnectMenuItemClick;

            var disconnectMenuItem = new MenuItem(MenuItems.Disconnect) { Name = DisconnectItemName };
            disconnectMenuItem.Click += DisconnectMenuItemClick;
            menuItem.MenuItems.Add(connectMenuItem);
            menuItem.MenuItems.Add(disconnectMenuItem);
            return menuItem;
        }

        private Connection GetConnection(MenuItem menuItem)
        {
            var connectionName = menuItem.Parent.Name;
            var connection = _connections.FirstOrDefault(c => c.Name == connectionName);
            return connection;
        }


        void MenuItemPopup(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connection = _connections.FirstOrDefault(c => c.Name == menuItem.Name);
            bool isConnected = false;
            try
            {
                isConnected = _connector.CheckConnection(connection);

            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Error was handled while chacking connection '{0}'", menuItem.Name), ex);
            }

            var connectItem = menuItem.MenuItems.Find(ConnectItemName, false).SingleOrDefault();
            if (connectItem != null)
            {
                connectItem.Enabled = !isConnected;
            }

            var disconnectItem = menuItem.MenuItems.Find(DisconnectItemName, false).SingleOrDefault();
            if (disconnectItem != null)
            {
                disconnectItem.Enabled = isConnected;
            }
        }

        private void ConnectMenuItemClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connection = GetConnection(menuItem);
            if (connection == null)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.ConnectionChecking, Messages.ConnectionNotFound, ToolTipIcon.Info);
                return;
            }

            var connectResult = _connector.Connect(connection);
            if (connectResult)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.Connection, string.Format(Messages.SuccesfulConnection, connection.Name), ToolTipIcon.Info);
            }
            else
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.Connection, string.Format(Messages.NonSuccesfulConnection, connection.Name), ToolTipIcon.Error);
            }
        }

        private void DisconnectMenuItemClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connection = GetConnection(menuItem);
            if (connection == null)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.ConnectionChecking, Messages.ConnectionNotFound, ToolTipIcon.Info);
                return;
            }

            var disconnestResult = _connector.Disconnect(connection);
            if (disconnestResult)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.Disconnection, string.Format(Messages.SuccesfulDisconnection, connection.Name), ToolTipIcon.Info);
            }
            else
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.Disconnection, string.Format(Messages.NonSuccesfulDisconnection, connection.Name), ToolTipIcon.Error);
            }
        }
    }
}
