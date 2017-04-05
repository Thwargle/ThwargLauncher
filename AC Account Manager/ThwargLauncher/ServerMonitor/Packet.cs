using System;
using System.Runtime.InteropServices;
using System.Text;
using ACE.Common.Cryptography;

namespace ThwargLauncher
{
    internal class Packet
    {
        [Flags]
        public enum PacketHeaderFlags : uint
        {
            None = 0x00000000,
            Retransmission = 0x00000001,
            EncryptedChecksum = 0x00000002, // can't be paired with 0x00000001, see FlowQueue::DequeueAck
            BlobFragments = 0x00000004,
            ServerSwitch = 0x00000100,
            Referral = 0x00000800,
            RequestRetransmit = 0x00001000,
            RejectRetransmit = 0x00002000,
            AckSequence = 0x00004000,
            Disconnect = 0x00008000,
            LoginRequest = 0x00010000,
            WorldLoginRequest = 0x00020000,
            ConnectRequest = 0x00040000,
            ConnectResponse = 0x00080000,
            CICMDCommand = 0x00400000,
            TimeSynch = 0x01000000,
            EchoRequest = 0x02000000,
            EchoResponse = 0x04000000,
            Flow = 0x08000000
        }
        public static byte[] MakeLoginPacket()
        {
            byte[] loginPacket = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x93, 0x00, 0xd0, 0x05, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x04, 0x00, 0x31, 0x38, 0x30, 0x32, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0xb8, 0xa8, 0x58, 0x1c, 0x00, 0x61, 0x63, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x74, 0x72, 0x61, 0x63, 0x6b, 0x65, 0x72, 0x3a, 0x6a, 0x6a, 0x39, 0x68, 0x32, 0x36, 0x68, 0x63, 0x73, 0x67, 0x67, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return loginPacket;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class PacketHeader
        {
            public static uint HeaderSize { get { return 0x20u; } }

            public uint Sequence { get; set; }
            public PacketHeaderFlags Flags { get; set; }
            public uint Checksum { get; set; }
            public ushort Id { get; set; }
            public ushort Time { get; set; }
            public ushort Size { get; set; }
            public ushort Table { get; set; }

            public PacketHeader(PacketHeaderFlags flags)
            {
                this.Size = (ushort)HeaderSize;
                this.Flags = flags;
            }

            public byte[] GetRaw()
            {
                var headerHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
                try
                {
                    byte[] bytes = new byte[Marshal.SizeOf(typeof(PacketHeader))];
                    Marshal.Copy(headerHandle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                    return bytes;
                }
                finally
                {
                    headerHandle.Free();
                }
            }

            public void CalculateHash32(out uint checksum)
            {
                uint original = Checksum;

                Checksum = 0x0BADD70DD;
                byte[] rawHeader = GetRaw();
                checksum = Hash32.Calculate(rawHeader, rawHeader.Length);
                Checksum = original;
            }
        }
    }
}
