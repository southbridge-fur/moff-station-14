using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

public sealed class VampireHungerSystem : EntitySystem
{
    [Dependency] private readonly GameTiming _timing = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly HungerSystem _hungerSystem = default!;
    [Dependency] private readonly ThirstSystem _thirstSystem = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;
        var enumerator = EntityQueryEnumerator<VampireHungerComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            Update(uid, comp, time);
        }
    }

    protected void Update(EntityUid uid, VampireHungerComponent comp, TimeSpan time)
    {
        if (time < comp.NextUpdate)
            return;

        comp.NextUpdate += comp.UpdateInterval;

        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return; // we need at least the blood stream before we can do something.

        if (TryComp<HungerComponent>(uid, out var hunger))
        {
            var hungerValue = hunger.Thresholds[HungerThreshold.Overfed] * _bloodstreamSystem.GetBloodLevelPercentage(uid, bloodstream);
            _hungerSystem.SetHunger(uid, hungerValue, hunger);
        }

        if (TryComp<ThirstComponent>(uid, out var thirst))
        {
            var thirstValue = thirst.ThirstThresholds[ThirstThreshold.OverHydrated] * _bloodstreamSystem.GetBloodLevelPercentage(uid, bloodstream);
            _thirstSystem.SetThirst(uid, thirst, thirstValue);
        }
    }
}
