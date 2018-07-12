using System;
using System.Collections.Generic;

using Filter.Shared;

using Decal.Adapter;

namespace ThwargFilter
{
    class LoginMessageQueueManager
    {
        readonly Queue<string> loginMessageQueue = new Queue<string>();
        bool sendingLastEnter;

        void Current_RenderFrame(object sender, EventArgs e)
        {
            try
            {
                if (loginMessageQueue.Count == 0 && sendingLastEnter == false)
                {
                    CoreManager.Current.RenderFrame -= new EventHandler<EventArgs>(Current_RenderFrame);
                    return;
                }

                if (sendingLastEnter)
                {
                    PostMessageTools.SendEnter();
                    sendingLastEnter = false;
                }
                else
                {
                    PostMessageTools.SendEnter();
                    PostMessageTools.SendMsg(loginMessageQueue.Dequeue());
                    sendingLastEnter = true;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }
}
