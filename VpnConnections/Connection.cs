using System.Collections.Generic;

namespace VpnConnections
{
    /// <summary>
    /// Connection
    /// </summary>
    public class Connection
    {
        public string Name { get; set; }

        public IEnumerable<Route> Routes { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }
    }
}
