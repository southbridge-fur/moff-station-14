using System.ComponentModel.DataAnnotations;
using Content.Shared._Moffstation.EntitySystems.Swappable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Moffstation.Swappable;

/// <summary>
/// Adds a verb menu to the attached entity which allows it to be swapped to one of several other entities.
/// It's usually best to place this component on a parent entity, with all the <see cref="Prototypes"/> set to every
/// child entity so they can be swapped between each other.
/// </summary>
[Access(typeof(SharedSwappableSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SwappableComponent : Component
{
    /// <summary>
    /// Minimum delay between swaps.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(5);

    /// <summary>
    /// When the current Cooldown ends.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan CooldownEnd = TimeSpan.Zero;

    /// <summary>
    /// Doafter length for the swap.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Whether to include the current entity this component has been placed on into the options.
    /// </summary>
    /// <remarks>
    /// This is so you can create a start or "seed" entity, which can then be changed to one of several options.
    /// </remarks>
    [DataField]
    public bool IncludeSelf = true;

    /// <summary>
    /// The list of prototypes to swap between.
    /// </summary>
    [DataField, Required]
    public List<EntProtoId> Prototypes = new();

    /// <summary>
    /// Sound effect to play on swap.
    /// </summary>
    [DataField]
    public SoundSpecifier SwapSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    /// <summary>
    /// The whitelist used to determine who can perform the swaps.
    /// </summary>
    /// <remarks>
    /// This is to allow for cases where you want to restrict who is performing the swap to specific people
    /// Such as only allowing the chaplain to swap their bible, or only for people with hands to swap another device.
    /// </remarks>
    [DataField]
    public EntityWhitelist Whitelist = new();
}

[Serializable, NetSerializable]
public sealed class SwappableOpenUIMessage(List<EntProtoId> prototypes, NetEntity entity) : BoundUserInterfaceState
{
    public List<EntProtoId> Prototypes = prototypes;
    public NetEntity TargetEntity = entity;
}

[Serializable, NetSerializable]
public sealed class SwappableEntitySelectedMessage(EntProtoId proto, NetEntity entity) : BoundUserInterfaceMessage
{
    public EntProtoId Prototype = proto;
    public NetEntity TargetEntity = entity;
}
