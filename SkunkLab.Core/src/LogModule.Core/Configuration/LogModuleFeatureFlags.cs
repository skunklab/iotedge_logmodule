using System;

namespace LogModule.Configuration
{
    [Flags]
    public enum LogModuleFeatureFlags
    {
        WebHost = 1,
        EdgeHubHost = 2,
        DirectMethodsHost = 4
    }
}
