using Content.Shared.Actions;

namespace Content.Shared._Moffstation.Vampire.Events;

public sealed partial class VampireEventRejuvenate : InstantActionEvent
{
    [DataField]
    public float StamHealing = 100;
}
