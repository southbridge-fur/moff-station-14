namespace Content.Shared._Moffstation.Vampire.Components;

public sealed partial class VampireHungerComponent : Component
{
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(5);
}
