using System;

using Filter.Shared;

namespace ThwargFilter
{
    class ThwargFilterCommandExecutor
    {
        public void ExecuteCommand(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                DecalProxy.DispatchChatToBoxWithPluginIntercept(command);
            }
        }
    }
}
