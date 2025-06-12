using Robust.Shared.Prototypes;

namespace Content.Server._Moffstation.Vampire.Abilities.Components;

public sealed partial class AbilityRejuvenateComponent : Component
{
    [DataField]
    public float StamHealing = 100.0f;

    [DataField]
    public EntProtoId ActionProto = "ActionVampireRejuvenate";

    [DataField]
    public EntityUid? Action;
}
