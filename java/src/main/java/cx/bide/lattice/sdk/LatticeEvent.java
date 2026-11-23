package cx.bide.lattice.sdk;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public final class LatticeEvent {
    public static final int SIZE = 12;

    public final LatticeEventType type;
    public final int param1;
    public final int param2;

    public LatticeEvent(LatticeEventType type, int param1, int param2) {
        this.type = type;
        this.param1 = param1;
        this.param2 = param2;
    }

    public byte[] encode() {
        byte[] data = new byte[SIZE];
        data[0] = type.id();
        ByteBuffer.wrap(data).order(ByteOrder.LITTLE_ENDIAN).putInt(4, param1);
        ByteBuffer.wrap(data).order(ByteOrder.LITTLE_ENDIAN).putInt(8, param2);
        return data;
    }

    public static LatticeEvent decode(byte[] data) {
        if (data == null || data.length < SIZE) return null;
        LatticeEventType type = LatticeEventType.from(data[0]);
        if (type == null) return null;
        ByteBuffer buffer = ByteBuffer.wrap(data).order(ByteOrder.LITTLE_ENDIAN);
        return new LatticeEvent(type, buffer.getInt(4), buffer.getInt(8));
    }

    public static LatticeEvent handshake(int protocolVersion, int slot, int encodedGameVersion) {
        return new LatticeEvent(LatticeEventType.HANDSHAKE, (protocolVersion << 16) | (slot & 0xFFFF), encodedGameVersion);
    }

    public static LatticeEvent heartbeat() {
        return new LatticeEvent(LatticeEventType.HEARTBEAT, 0, 0);
    }

    public static LatticeEvent adapterInfo(int capabilityMask) {
        return new LatticeEvent(LatticeEventType.ADAPTER_INFO, capabilityMask, 0);
    }
}
