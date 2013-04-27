using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NLog;
using VpnConnections.Processing;
using VpnConnections.Resources;

namespace VpnConnections.Infrastructure
{
    public class TrayApplicationContext : ApplicationContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string DefaultCultureKey = "DefaultCulture";

        private readonly IUiBuilder _uiBuilder = new MenuBuilder();

        public TrayApplicationContext()
        {
            InitializeContext();
        }

        public void InitializeContext()
        {
            SetResourcesCulture();

            _uiBuilder.Build();
            _uiBuilder.Exit += (o, args) => ExitThread();
        }

        private static void SetResourcesCulture()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(DefaultCultureKey))
            {
                var culture = new CultureInfo(ConfigurationManager.AppSettings[DefaultCultureKey]);
                MenuItems.Culture = culture;
            }
        }
    }
}
