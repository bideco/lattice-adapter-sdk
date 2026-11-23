using System;

namespace Lattice.AdapterSdk
{
    public enum LatticeCapability
    {
        Keyboard = 0,
        Mouse = 1,
        VirtualCursor = 2,
        Scroll = 3,
        MiddleClick = 4,
        SettingsSync = 5,
        LiveSettingsApply = 6,
        Resourcepacks = 7,
        ModConfigs = 8,
        Hud = 9,
        Profiles = 10,
        WindowLayout = 11,
        MultiInstanceSwitching = 12,
    }

    public enum LatticeEventType : byte
    {
        MouseMove = 0x00,
        MouseButton = 0x01,
        MouseScroll = 0x02,
        KeyPress = 0x03,
        KeyRelease = 0x04,
        CaptureState = 0x05,
        Handshake = 0x06,
        Heartbeat = 0x07,
        Char = 0x0A,
        SlotBusy = 0x0B,
        SlotActive = 0x0C,
        SlotInactive = 0x0D,
        AdapterInfo = 0x1F,
    }

    public struct LatticeEvent
    {
        public const int Size = 12;

        public LatticeEvent(LatticeEventType type, int param1, int param2)
        {
            Type = type;
            Param1 = param1;
            Param2 = param2;
        }

        public LatticeEventType Type { get; }
        public int Param1 { get; }
        public int Param2 { get; }

        public byte[] Encode()
        {
            var buffer = new byte[Size];
            buffer[0] = (byte)Type;
            BitConverter.GetBytes(Param1).CopyTo(buffer, 4);
            BitConverter.GetBytes(Param2).CopyTo(buffer, 8);
            return buffer;
        }

        public static bool TryDecode(byte[] buffer, out LatticeEvent latticeEvent)
        {
            latticeEvent = default(LatticeEvent);
            if (buffer == null || buffer.Length < Size)
            {
                return false;
            }
            latticeEvent = new LatticeEvent(
                (LatticeEventType)buffer[0],
                BitConverter.ToInt32(buffer, 4),
                BitConverter.ToInt32(buffer, 8));
            return true;
        }

        public static LatticeEvent Handshake(int protocolVersion, int slot, int encodedGameVersion)
        {
            return new LatticeEvent(
                LatticeEventType.Handshake,
                (protocolVersion << 16) | (slot & 0xFFFF),
                encodedGameVersion);
        }

        public static LatticeEvent Heartbeat()
        {
            return new LatticeEvent(LatticeEventType.Heartbeat, 0, 0);
        }

        public static LatticeEvent AdapterInfo(int capabilityMask)
        {
            return new LatticeEvent(LatticeEventType.AdapterInfo, capabilityMask, 0);
        }
    }
}
