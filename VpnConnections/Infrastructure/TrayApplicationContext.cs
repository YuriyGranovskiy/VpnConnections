using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NLog;
using VpnConnections.Connections;
using VpnConnections.Processing;
using VpnConnections.Resources;

namespace VpnConnections.Infrastructure
{
    public class TrayApplicationContext : ApplicationContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private NotifyIcon _notifyIcon;
        private IEnumerable<Connection> _connections;
        private const string DefaultCultureKey = "DefaultCulture";

        public TrayApplicationContext()
        {
            InitializeContext();
        }

        public void InitializeContext()
        {
            SetResourcesCulture();

            _connections = ConnectionManager.GetConnections();
            var icon = new Icon(Properties.Resources.pnidui_3048, 33, 33);
            _notifyIcon = new NotifyIcon
                              {
                                  Icon = icon,
                                  Visible = true
                              };
            List<MenuItem> menuItems = _connections.Select(GetMenuItemForConnection).ToList();
            var exitMenuItem = new MenuItem(MenuItems.Exit) {Name = "Exit"};
            exitMenuItem.Click += (o, args) => ExitThread();
            menuItems.Add(exitMenuItem);
            _notifyIcon.ContextMenu = new ContextMenu(menuItems.ToArray());
        }

        private static void SetResourcesCulture()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(DefaultCultureKey))
            {
                var culture = new CultureInfo(ConfigurationManager.AppSettings[DefaultCultureKey]);
                MenuItems.Culture = culture;
            }
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
            var connectMenuItem = new MenuItem(MenuItems.Connect) {Name = "Connect"};
            connectMenuItem.Click += connectMenuItem_Click;

            var disconnectMenuItem = new MenuItem(MenuItems.Disconnect) {Name = "Disconnect"};
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
                Logger.ErrorException(string.Format("Error was handled while chacking connection '{0}'", menuItem.Name), ex);
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
