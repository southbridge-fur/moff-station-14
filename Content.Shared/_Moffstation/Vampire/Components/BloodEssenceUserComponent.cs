using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Moffstation.Vampire.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodEssenceUserComponent : Component
{
    /// <summary>
    /// The vampire's current amount of blood essence collected.
    /// </summary>
    [DataField]
    public FixedPoint2 BloodEssenceBalance = 0.0;

    // todo: Keep a record of people who we have taken blood essence from

}
