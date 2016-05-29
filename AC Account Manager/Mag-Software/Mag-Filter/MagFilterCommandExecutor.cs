using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Mag.Shared;

namespace MagFilter
{
    class MagFilterCommandExecutor
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
