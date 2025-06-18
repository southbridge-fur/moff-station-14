using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

/// <summary>
/// This system handles entities with the
/// <see cref="Content.Shared._Moffstation.Vampire.Components.BloodConsumptionComponent"/>
/// and effectively manages their bloodstream.
/// The intention behind this is for vampire-like creatures to be able to use their bloodstream as
/// their reservoir for blood.
/// Blood in this case being their resource for spellcasting, healing, and is intended to be replenishable
/// by drinking the blood of other creatures (or eating blood packs).
/// </summary>
/// <remarks>
/// todo: set up proper adapter methods to interface with the bloodstream, for other systems to use.
/// </remarks>
public sealed class BloodConsumptionSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly HungerSystem _hungerSystem = default!;
    [Dependency] private readonly ThirstSystem _thirstSystem = default!;
    [Dependency] private readonly DamageableSystem _damageSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodConsumptionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BloodConsumptionComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<BloodConsumptionComponent>(entity, out var component))
            return;

        component.NextUpdate = _timing.CurTime + component.UpdateInterval;

        if (!TryComp<BloodstreamComponent>(entity, out var bloodstream))
            return;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;
        var enumerator = EntityQueryEnumerator<BloodConsumptionComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            UpdateBloodConsumption(uid, comp, time);
        }
    }

    /// <summary>
    /// This method does a couple things on update:
    ///     - Calls <see cref="UpdateRegeneration"/> to drain the bloodstream slightly and heal if needed.
    ///     - Calls <see cref="UpdateHungerThirst"/> To set the percentage values of hunger and thirst to a percentage of the bloodstream.
    ///     - Calls <see cref="FlushTempSolution"/> Which flushes the temporary solution, preventing them from spilling blood on the ground.
    /// </summary>
    private void UpdateBloodConsumption(EntityUid uid, BloodConsumptionComponent comp, TimeSpan time)
    {
        if (time < comp.NextUpdate)
            return;

        comp.NextUpdate += comp.UpdateInterval;

        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return; // we need at least the blood stream before we can do something.

        UpdateRegeneration(uid, comp, bloodstream);
        UpdateHungerThirst(uid, comp, bloodstream);
        FlushTempSolution(uid, bloodstream);
    }

    /// <summary>
    /// Updates the vampire's hunger and thirst values periodically based on the current blood level percentage.
    /// The hunger and thirst values are limited in how fast they can change via the
    /// <see cref="BloodConsumptionComponent.MaxChange"/> value.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="comp"></param>
    /// <param name="bloodstream"></param>
    private void UpdateHungerThirst(EntityUid uid, BloodConsumptionComponent comp, BloodstreamComponent bloodstream)
    {
        var bloodstreamPercentage = _bloodstreamSystem.GetBloodLevelPercentage(uid, bloodstream);
        var modificationPercentage = Math.Clamp(
            bloodstreamPercentage - comp.PrevBloodPercentage,
            -comp.MaxChange,
            comp.MaxChange);
        if (TryComp<HungerComponent>(uid, out var hunger))
            _hungerSystem.ModifyHunger(uid, modificationPercentage * hunger.Thresholds[HungerThreshold.Overfed], hunger);
        if (TryComp<ThirstComponent>(uid, out var thirst))
            _thirstSystem.ModifyThirst(uid, thirst, modificationPercentage * thirst.ThirstThresholds[ThirstThreshold.OverHydrated]);
        comp.PrevBloodPercentage += modificationPercentage;
    }

    /// <summary>
    /// Updates the vampire's bloodstream according to whether they are healing or not. Also performs their healing.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="comp"></param>
    /// <param name="bloodstream"></param>
    private void UpdateRegeneration(EntityUid uid, BloodConsumptionComponent comp, BloodstreamComponent bloodstream)
    {
        // check damage
        if (TryComp<DamageableComponent>(uid, out var damage))
        {
            if (damage.Damage.AnyPositive()) // Vampires should be able to heal all damage types
            {
                // heal according to comp amount
                _damageSystem.TryChangeDamage(uid, comp.HealPerUpdate, true, false, damage);
                // subtract blood for healing
                _bloodstreamSystem.TryModifyBloodLevel(uid, comp.HealingBloodlossPerUpdate, bloodstream);
                return;
            }
        }
        // else subtract the usual amount of blood
        _bloodstreamSystem.TryModifyBloodLevel(uid, comp.BaseBloodlossPerUpdate, bloodstream);
    }

    /// <summary>
    /// Clear the temporary solution of the Bloodstream.
    /// This is an in-elegant method to avoid the vampire spilling their blood all over the floor.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="comp"></param>
    /// <param name="bloodstream"></param>
    /// <remarks>
    /// todo: Make this a bit more elegant, and maybe introduce a method where vampires will spill reagents that
    /// their body rejects (food, drinks, basically anything that isn't blood). The problem is that the bloodstream
    /// will fail to properly initialize if we nullify the temporary solution, so this may require some changes
    /// to bloodstreams themselves to accept the ability to not have a temporary solution.
    /// </remarks>
    private void FlushTempSolution(EntityUid uid, BloodstreamComponent bloodstream)
    {
        if (!_solutionContainerSystem.ResolveSolution(uid,
                bloodstream.BloodTemporarySolutionName,
                ref bloodstream.TemporarySolution,
                out var tempSolution))
            return;
        tempSolution.RemoveAllSolution();
    }
}
