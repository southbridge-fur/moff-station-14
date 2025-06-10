using Content.Shared._Moffstation.Actions.EntitySystems;

using Content.Shared.Actions.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Moffstation.Actions.Components;

/// <summary>
/// An Action that has charges
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedChargesActionSystem))]
[EntityCategory("Actions")]
public sealed partial class ChargesActionComponent : Component
{
    /// <summary>
    /// The maximum amount of charges this action can have.
    /// </summary>
    [DataField]
    public int MaxCharges;

    /// <summary>
    /// The amount of time until a charge refills.
    /// </summary>
    [DataField]
    public TimeSpan RechargeDuration;

    [DataField]
    public Queue<ActionCooldown?> RechargeCooldowns = new();
}
