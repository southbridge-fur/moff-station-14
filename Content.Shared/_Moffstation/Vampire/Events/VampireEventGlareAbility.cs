using Content.Shared.Actions;

namespace Content.Shared._Moffstation.Vampire.Events;

public sealed partial class VampireEventGlareAbility : InstantActionEvent
{
    [DataField]
    public float Range = 1.0f;

    [DataField]
    public float DamageFront = 70;

    [DataField]
    public float DamageSides = 35;

    [DataField]
    public float DamageRear = 10;
}
