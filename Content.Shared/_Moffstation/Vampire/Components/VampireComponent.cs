using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Moffstation.Vampire.Components;

[NetworkedComponent]
[RegisterComponent, Access(typeof(SharedVampireSystem))]
public sealed partial class VampireComponent : Component
{
    /// <summary>
    /// The BloodEssence prototype for the shop
    /// </summary>
    [DataField]
    public ProtoId<CurrencyPrototype> StolenBloodEssenceCurrencyPrototype = "BloodEssence";

#region Feed Ability
    /// <summary>
    /// The amount of blood to drink from someone per feed action
    /// </summary>
    [DataField]
    public FixedPoint2 BloodPerFeed = 10.0;

    /// <summary>
    /// The duration of the feed action (in seconds)
    /// </summary>
    [DataField]
    public float FeedDuration = 2.5f;
#endregion // Feed

}
