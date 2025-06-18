using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Moffstation.Vampire.Components;

/// <summary>
/// This is the component given to
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodConsumptionComponent : Component
{
    /// <summary>
    /// The next time an update will be performed
    /// </summary>
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// The interval between updates.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The amount of blood to lose per update. In standard units (u).
    /// </summary>
    [DataField]
    public float BaseBloodlossPerUpdate = -0.01f;

    /// <summary>
    /// The amount of blood to lose per update while restoring health naturally. In standard units (u).
    /// </summary>
    [DataField]
    public float HealingBloodlossPerUpdate = -1.0f;

    /// <summary>
    /// Damage to deal per update (use negative values to specify healing)
    /// </summary>
    [DataField]
    public DamageSpecifier HealPerUpdate = new();

    /// <summary>
    /// Previous Blood Percentage
    /// </summary>
    [DataField]
    public float PrevBloodPercentage = 0.5f;

    /// <summary>
    /// The maximum percentage (0.0-1.0) of change per update.
    /// </summary>
    [DataField]
    public float MaxChange = 0.1f;
}
