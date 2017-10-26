using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher.UtilityCode
{
    class WinSetErrorMode
    {
        [DllImport("kernel32.dll")]
        public static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }
    }
}
