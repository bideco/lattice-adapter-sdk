package cx.bide.lattice.sdk;

public enum LatticeCapability {
    KEYBOARD(0),
    MOUSE(1),
    VIRTUAL_CURSOR(2),
    SCROLL(3),
    MIDDLE_CLICK(4),
    SETTINGS_SYNC(5),
    LIVE_SETTINGS_APPLY(6),
    RESOURCEPACKS(7),
    MOD_CONFIGS(8),
    HUD(9),
    PROFILES(10),
    WINDOW_LAYOUT(11),
    MULTI_INSTANCE_SWITCHING(12);

    private final int bit;

    LatticeCapability(int bit) {
        this.bit = bit;
    }

    public int bit() {
        return bit;
    }
}
