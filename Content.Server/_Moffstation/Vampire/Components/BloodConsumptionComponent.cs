using Content.Server.Body.Components;

namespace Content.Server._Moffstation.Vampire.Components;

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
    /// Previous Blood Percentage
    /// </summary>
    [DataField]
    public float PrevBloodPercentage = 0.5f;

    /// <summary>
    /// The maximum percentage of change per update.
    /// </summary>
    [DataField]
    public float MaxChange = 0.1f;
}
