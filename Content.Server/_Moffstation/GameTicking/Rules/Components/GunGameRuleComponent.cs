using Content.Server.GameTicking.Rules;
using Content.Shared.FixedPoint;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Moffstation.GameTicking.Rules.Components;

/// <summary>
/// A game rule which rewards players with a new weapon sequentially and
/// ends once someone gets a kill with their final weapon.
/// </summary>

[RegisterComponent]
public sealed partial class GunGameRuleComponent : Component
{
    /// <summary>
    /// How long until the round restarts
    /// </summary>
    [DataField]
    public TimeSpan RestartDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The person who won.
    /// We store this here in case of some assist shenanigans.
    /// </summary>
    [DataField]
    public NetUserId? Victor;

    /// <summary>
    /// The queue to use when spawning weapons.
    /// This queue is replicated for each player and every respective player receives the next
    /// item in the queue when they get a kill.
    /// </summary>
    /// <remarks>
    /// todo: Implement the ability to use EntityTables so multiple items can be spawned at once.
    /// </remarks>
    [DataField]
    public Queue<EntProtoId> RewardSpawnsQueue = new();

    /// <summary>
    /// Individual player reward queues, copied from the reward spawns queue.
    /// </summary>
    [DataField]
    public Dictionary<NetUserId, Queue<EntProtoId>> PlayerRewards = new();

    /// <summary>
    /// The gear all players spawn with.
    /// This loadout should not include a starting weapon as the starting weapon is selected from the
    /// <see cref="RewardSpawnsQueue"/>.
    /// </summary>
    [DataField]
    public ProtoId<StartingGearPrototype> Gear = "GunGameGear";
}
