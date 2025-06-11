using Content.Shared.Damage;
using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Shared._Moffstation.Vampire.Components;

public sealed partial class BurnedBySunComponent : Component
{
    [DataField]
    public List<ProtoId<ContentTileDefinition>> TileBlacklist = new();

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

    [DataField]
    public TimeSpan LastBurn = TimeSpan.Zero;

    [DataField]
    public float Accumulation = 0.0f;

    [DataField]
    public DamageSpecifier Damage = new();
}
