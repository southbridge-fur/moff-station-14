using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server._Moffstation.Vampire.Abilities.Components;

[RegisterComponent]
public sealed partial class AbilityRejuvenateComponent : Component
{
    [DataField]
    public float StamHealing = -100.0f;

    [DataField]
    public TimeSpan StatusEffectReductionTime = TimeSpan.FromSeconds(120);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg");

    [DataField]
    public EntProtoId ActionProto = "ActionVampireRejuvenate";

    [DataField]
    public EntityUid? Action;
}
