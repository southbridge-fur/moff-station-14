using Content.Shared.Maps;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Tiles
{
    /// <summary>
    /// This gives items floor tile behavior, but it doesn't have to be a literal floor tile.
    /// A lot of materials use this too. Note that the AfterInteract will fail without a stack component on the item.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class FloorTileComponent : Component
    {
        [DataField]
        public List<ProtoId<ContentTileDefinition>>? Outputs;

        [DataField("placeTileSound")] public SoundSpecifier PlaceTileSound =
            new SoundPathSpecifier("/Audio/Items/genhit.ogg")
            {
                Params = AudioParams.Default.WithVariation(0.125f),
            };
    }
}
