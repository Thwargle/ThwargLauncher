using System;
using System.Collections.Generic;

using Mag.Shared;

using Decal.Adapter;

namespace MagFilter
{
    class AfterLoginCompleteMessageQueueManager
    {
        bool freshLogin;

        LoginCommands _loginCommands = new LoginCommands();
        bool sendingLastEnter;

        DateTime loginCompleteTime = DateTime.MaxValue;

        public void FilterCore_ClientDispatch(object sender, NetworkMessageEventArgs e)
        {
            if (e.Message.Type == 0xF7C8) // Enter Game
                freshLogin = true;

            if (freshLogin && e.Message.Type == 0xF7B1 && Convert.ToInt32(e.Message["action"]) == 0xA1) // Character Materialize (Any time is done portalling in, login or portal)
            {
                freshLogin = false;

                var persister = new LoginCommandPersister();
                _loginCommands = persister.ReadQueue();

                if (_loginCommands.MessageQueue.Count > 0)
                {
                    loginCompleteTime = DateTime.Now;

                    sendingLastEnter = false;
                    CoreManager.Current.RenderFrame += new EventHandler<EventArgs>(Current_RenderFrame);
                }
            }
        }

        void Current_RenderFrame(object sender, EventArgs e)
        {
            try
            {
                if (DateTime.Now.Subtract(TimeSpan.FromMilliseconds(_loginCommands.WaitMillisencds)) < loginCompleteTime)
                    return;

                if (_loginCommands.MessageQueue.Count == 0 && sendingLastEnter == false)
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
                    string cmd = _loginCommands.MessageQueue.Dequeue();
                    // The game is losing the first character of our commands
                    // So deliberately send a space at the start
                    if (!cmd.StartsWith(" "))
                    {
                        cmd = " " + cmd;
                    }
                    log.WriteLogMsg(String.Format("Dequeued a login message: '{0}'", cmd));
                    PostMessageTools.SendCharString(cmd);
                    sendingLastEnter = true;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        public void FilterCore_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            bool writeChanges = true;
            bool global = false;
            if (e.Text.Contains("/mfglobal")) { global = true; }
            log.WriteLogMsg("QQ TEST");
            if (e.Text.StartsWith("/mf alcmq add ") || e.Text.StartsWith("/mf olcmq add "))
            {
                _loginCommands.MessageQueue.Enqueue(e.Text.Substring(14, e.Text.Length - 14));
                Debug.WriteToChat("After Login Complete Message Queue added: " + e.Text);

                e.Eat = true;
            }
            else if (e.Text == "/mf alcmq clear" || e.Text == "/mf olcmq clear")
            {
                _loginCommands.MessageQueue.Clear();
                Debug.WriteToChat("After Login Complete Message Queue cleared");

                e.Eat = true;
            }
            else if (e.Text.StartsWith("/mf alcmq wait set "))
            {
                _loginCommands.WaitMillisencds = int.Parse(e.Text.Substring(19, e.Text.Length - 19));
                Debug.WriteToChat("After Login Complete Message Queue Wait time set: " + e.Text + "ms");

                e.Eat = true;
            }
            else if (e.Text.StartsWith("/mf olcwait set ")) // Backwards Compatability
            {
                _loginCommands.WaitMillisencds = int.Parse(e.Text.Substring(16, e.Text.Length - 16));
                Debug.WriteToChat("After Login Complete Message Queue Wait time set: " + e.Text + "ms");

                e.Eat = true;
            }
            else if (e.Text == "/mf alcmq wait clear" || e.Text == "/mf olcwait clear")
            {
                _loginCommands.ClearWait();
                Debug.WriteToChat(string.Format("After Login Complete Wait time reset to default {0} ms", LoginCommands.DefaultMillisecondsToWaitAfterLoginComplete));

                e.Eat = true;
            }
            else if (e.Text == "/mf alcmq show" || e.Text == "/mf olcmq show")
            {
                Debug.WriteToChat(string.Format("LoginCmds: {0}", _loginCommands.MessageQueue.Count));
                foreach (string cmd in _loginCommands.MessageQueue)
                {
                    Debug.WriteToChat(string.Format("cmd: {0}", cmd));
                }
                Debug.WriteToChat(string.Format("Wait: {0}", _loginCommands.WaitMillisencds));

                e.Eat = true;
                writeChanges = false;
            }
            if (e.Eat && writeChanges)
            {
                var persister = new LoginCommandPersister();
                persister.WriteQueue(_loginCommands, global);
            }
        }
    }
}