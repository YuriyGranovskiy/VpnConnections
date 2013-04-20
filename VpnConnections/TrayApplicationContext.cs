using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VpnConnections
{
    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;
        private IEnumerable<Connection> _connections;

        public TrayApplicationContext()
        {
            InitializeContext();
        }

        public void InitializeContext()
        {
            _connections = ConnectionManager.GetConnections();
            var icon = new Icon(Properties.Resources.pnidui_3048, 33, 33);
            _notifyIcon = new NotifyIcon
                              {
                                  Icon = icon,
                                  Visible = true
                              };
            List<MenuItem> menuItems = _connections.Select(GetMenuItemForConnection).ToList();
            var exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Name = "Exit";
            exitMenuItem.Click += exitMenuItem_Click;
            menuItems.Add(exitMenuItem);
            _notifyIcon.ContextMenu = new ContextMenu(menuItems.ToArray());
        }

        void exitMenuItem_Click(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            _notifyIcon.Visible = false; // should remove lingering tray icon!

            base.ExitThreadCore(); 
        }

        private MenuItem GetMenuItemForConnection(Connection connection)
        {
            var menuItem = new MenuItem
                               {
                                   Name = connection.Name, 
                                   Text = connection.Name
                               };

            menuItem.Popup += menuItem_Popup;
            var connectMenuItem = new MenuItem("Подключить");
            connectMenuItem.Name = "Connect";
            connectMenuItem.Click += connectMenuItem_Click;
            
            var disconnectMenuItem = new MenuItem("Отключить");
            disconnectMenuItem.Name = "Disconnect";
            disconnectMenuItem.Click += disconnectMenuItem_Click;
            menuItem.MenuItems.Add(connectMenuItem);
            menuItem.MenuItems.Add(disconnectMenuItem);
            return menuItem;
        }

        void menuItem_Popup(object sender, EventArgs e)
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
            }
            var connectItem = menuItem.MenuItems.Find("Connect", false).SingleOrDefault();
            if(connectItem != null)
            {
                connectItem.Enabled = !isConnected;
            }

            var disconnectItem = menuItem.MenuItems.Find("Disconnect", false).SingleOrDefault();
            if (disconnectItem != null)
            {
                disconnectItem.Enabled = isConnected;
            }
        }

        void disconnectMenuItem_Click(object sender, EventArgs e)
        {
            var connection = GetConnection((MenuItem)sender);
            ConnectionManager.DisConnect(connection);
        }

        void connectMenuItem_Click(object sender, EventArgs e)
        {
            var connection = GetConnection((MenuItem) sender);
            ConnectionManager.Connect(connection);
        }

        private Connection GetConnection(MenuItem menuItem)
        {
            var connectionName = menuItem.Parent.Name;
            var connection = _connections.FirstOrDefault(c => c.Name == connectionName);
            return connection;
        }
    }
}
