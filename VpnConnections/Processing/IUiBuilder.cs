using System;

namespace VpnConnections.Processing
{
    public interface IUiBuilder
    {
        event EventHandler Exit;
        void Build();
    }
}