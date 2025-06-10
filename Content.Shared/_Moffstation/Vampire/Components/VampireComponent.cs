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

#region Glare Ability

    // Glare should really be changed to a more generic ability that is then customized for this but I'm lazy right now
    [DataField]
    public float GlareRange = 1.0f;

    [DataField]
    public float GlareDamageFront = 70;

    [DataField]
    public float GlareDamageSides = 35;

    [DataField]
    public float GlareDamageRear = 10;

#endregion // Glare

#region Rejuvenate Ability
    [DataField]
    public float RejuvenateStamHealing = 100;
#endregion // Rejuvenate
}
