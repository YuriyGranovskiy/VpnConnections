using System;

namespace VpnConnections.Connections
{
    /// <summary>
    /// Route
    /// </summary>
    [Serializable]
    public class Route
    {
        public string NetAddress { get; set; }
        public string Mask { get; set; }
    }
}
