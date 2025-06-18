using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

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

        _bloodstreamSystem.TryModifyBloodLevel(entity, (bloodstream.BloodMaxVolume*component.PrevBloodPercentage) - bloodstream.BloodMaxVolume, bloodstream);
        component.PrevBloodPercentage = _bloodstreamSystem.GetBloodLevelPercentage(entity, bloodstream);
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
    /// Sets the percentage values of hunger and thirst to a percentage of the bloodstream.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="comp"></param>
    /// <param name="time"></param>
    /// <remarks>
    /// </remarks>
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
                _damageSystem.TryChangeDamage(uid, comp.HealPerUpdate, false, false, damage);
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
    /// their body rejects (food, drinks, basically anything that isn't blood)
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
