using System;

namespace ThwargLauncher
{
    class GameStatusNotice
    {
        public string StatusText;
        public bool IsWaiting;
        public bool IsSuccess;
        public bool IsFailure;
        public bool IsWrongServer;
        public static GameStatusNotice CreateWaiting(string status) { return new GameStatusNotice() { StatusText = status, IsWaiting = true }; }
        public static GameStatusNotice CreateSuccess(string status) { return new GameStatusNotice() { StatusText = status, IsSuccess = true }; }
        public static GameStatusNotice CreateFailure(string status) { return new GameStatusNotice() { StatusText = status, IsFailure = true }; }
        public static GameStatusNotice CreateWrongServer(string status) { return new GameStatusNotice() { StatusText = status, IsWrongServer = true }; }
    }
}
