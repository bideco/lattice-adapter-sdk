package cx.bide.lattice.sdk;

public enum LatticeEventType {
    MOUSE_MOVE(0x00),
    MOUSE_BUTTON(0x01),
    MOUSE_SCROLL(0x02),
    KEY_PRESS(0x03),
    KEY_RELEASE(0x04),
    CAPTURE_STATE(0x05),
    HANDSHAKE(0x06),
    HEARTBEAT(0x07),
    CHAR(0x0A),
    SLOT_BUSY(0x0B),
    SLOT_ACTIVE(0x0C),
    SLOT_INACTIVE(0x0D),
    ADAPTER_INFO(0x1F);

    private final byte id;

    LatticeEventType(int id) {
        this.id = (byte) id;
    }

    public byte id() {
        return id;
    }

    public static LatticeEventType from(byte id) {
        for (LatticeEventType type : values()) {
            if (type.id == id) return type;
        }
        return null;
    }
}
