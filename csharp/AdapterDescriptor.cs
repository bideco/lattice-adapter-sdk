using System;

namespace Lattice.AdapterSdk
{
    public sealed class AdapterDescriptor
    {
        public AdapterDescriptor(
            string gameId,
            string adapterId,
            string adapterVersion,
            int slot,
            int encodedGameVersion,
            params LatticeCapability[] capabilities)
        {
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            AdapterId = adapterId ?? throw new ArgumentNullException(nameof(adapterId));
            AdapterVersion = adapterVersion ?? throw new ArgumentNullException(nameof(adapterVersion));
            Slot = slot;
            EncodedGameVersion = encodedGameVersion;
            Capabilities = capabilities ?? Array.Empty<LatticeCapability>();
        }

        public string GameId { get; }
        public string AdapterId { get; }
        public string AdapterVersion { get; }
        public int Slot { get; }
        public int EncodedGameVersion { get; }
        public LatticeCapability[] Capabilities { get; }

        public int CapabilityMask()
        {
            var mask = 0;
            foreach (var capability in Capabilities)
            {
                mask |= 1 << (int)capability;
            }
            return mask;
        }
    }
}
