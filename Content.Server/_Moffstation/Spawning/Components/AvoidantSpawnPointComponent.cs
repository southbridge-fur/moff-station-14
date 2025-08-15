namespace Content.Server._Moffstation.Spawning.Components;

/// <summary>
/// Attached to a spawn point which avoids nearby players when spawning.
/// </summary>
[RegisterComponent]
public sealed partial class AvoidantSpawnPointComponent : Component
{
    /// <summary>
    /// The distance
    /// </summary>
    [DataField]
    public float Range = 10f;
}
