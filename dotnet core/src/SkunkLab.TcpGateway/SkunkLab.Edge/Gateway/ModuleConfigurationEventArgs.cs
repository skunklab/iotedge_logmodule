using SkunkLab.Edge.Gateway.Mqtt;
using System;
using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway
{
    public class ModuleConfigurationEventArgs : EventArgs
    {
        public ModuleConfigurationEventArgs(EdgeConfig config)
        {
            Config = config;
        }

        public EdgeConfig Config { get; set; }
    }
}
