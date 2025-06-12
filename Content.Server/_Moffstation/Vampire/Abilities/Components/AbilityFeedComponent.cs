using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._Moffstation.Vampire.Abilities.Components;

public sealed partial class AbilityFeedComponent : Component
{
    /// <summary>
    /// The duration of the feed action (in seconds)
    /// </summary>
    [DataField]
    public TimeSpan FeedDuration = TimeSpan.FromSeconds(2.5);

    /// <summary>
    /// The amount of blood to drink from someone per feed action
    /// </summary>
    [DataField]
    public FixedPoint2 BloodPerFeed = 10.0;

    [DataField]
    public EntProtoId ActionProto = "ActionVampireFeed";

    [DataField]
    public EntityUid? Action;
}
