package cx.bide.lattice.sdk;

import java.util.Arrays;
import java.util.List;

public final class AdapterDescriptor {
    public final String gameId;
    public final String adapterId;
    public final String adapterVersion;
    public final int slot;
    public final int encodedGameVersion;
    public final List<LatticeCapability> capabilities;

    public AdapterDescriptor(
            String gameId,
            String adapterId,
            String adapterVersion,
            int slot,
            int encodedGameVersion,
            LatticeCapability... capabilities) {
        this.gameId = gameId;
        this.adapterId = adapterId;
        this.adapterVersion = adapterVersion;
        this.slot = slot;
        this.encodedGameVersion = encodedGameVersion;
        this.capabilities = Arrays.asList(capabilities);
    }

    public int capabilityMask() {
        int mask = 0;
        for (LatticeCapability capability : capabilities) {
            mask |= 1 << capability.bit();
        }
        return mask;
    }
}
