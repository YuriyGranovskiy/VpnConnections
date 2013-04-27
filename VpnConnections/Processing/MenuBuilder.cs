using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private NotifyIcon _notifyIcon;
        
        private IEnumerable<Connection> _connections;

        public event EventHandler Exit;
        
        public void Build()
        {
            _connections = ConnectionManager.GetConnections();
            var icon = new Icon(Properties.Resources.pnidui_3048, 33, 33);
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

            menuItem.Popup += menuItemPopup;
            var connectMenuItem = new MenuItem(MenuItems.Connect) { Name = ConnectItemName };
            connectMenuItem.Click += connectMenuItemClick;

            var disconnectMenuItem = new MenuItem(MenuItems.Disconnect) { Name = DisconnectItemName };
            disconnectMenuItem.Click += disconnectMenuItemClick;
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


        void menuItemPopup(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connection = _connections.FirstOrDefault(c => c.Name == menuItem.Name);
            bool isConnected = false;
            try
            {
                isConnected = ConnectionManager.CheckConnection(connection);

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

        private void connectMenuItemClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connection = GetConnection(menuItem);
            if (connection == null)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.ConnectionChecking, Messages.ConnectionNotFound, ToolTipIcon.Info);
                return;
            }

            var connectResult = ConnectionManager.Connect(connection);
            if (connectResult)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.Connection, string.Format(Messages.SuccesfulConnection, connection.Name), ToolTipIcon.Info);
            }
            else
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.Connection, string.Format(Messages.NonSuccesfulConnection, connection.Name), ToolTipIcon.Error);
            }
        }

        private void disconnectMenuItemClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connection = GetConnection(menuItem);
            if (connection == null)
            {
                _notifyIcon.ShowBalloonTip(5000, Messages.ConnectionChecking, Messages.ConnectionNotFound, ToolTipIcon.Info);
                return;
            }

            var disconnestResult = ConnectionManager.Disconnect(connection);
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
