using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Moffstation.Vampire.Events;

public sealed partial class VampireEventFeedAbility : EntityTargetActionEvent
{
    /// <summary>
    /// The duration of the feed action (in seconds)
    /// </summary>
    [DataField]
    public TimeSpan FeedDuration = TimeSpan.FromSeconds(2.5);
}

[Serializable, NetSerializable]
public sealed partial class VampireEventFeedAbilityDoAfter : SimpleDoAfterEvent
{
    /// <summary>
    /// The amount of blood to drink from someone per feed action
    /// </summary>
    [DataField]
    public FixedPoint2 BloodPerFeed = 10.0;
}
