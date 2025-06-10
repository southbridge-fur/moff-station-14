using Content.Shared._Moffstation.Actions.Components;
using Content.Shared.Actions.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Moffstation.Actions.EntitySystems;

public abstract class SharedChargesActionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargesActionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ChargesActionComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<ChargesActionComponent>(entity, out var comp))
            return;
        var curtime = _timing.CurTime;
        for (var i = 0; i < comp.MaxCharges; i++)
        {
            comp.RechargeCooldowns.Enqueue(NewCooldown(comp, curtime, curtime + new TimeSpan(1)));
        }
        Dirty(entity,comp);
    }

    public bool HasCharges(EntityUid uid, ChargesActionComponent? comp)
    {
        if (!Resolve(uid, ref comp))
            return false;

        var curTime = _timing.CurTime;
        var oldestCooldown = comp.RechargeCooldowns.Peek();
        return oldestCooldown.HasValue && oldestCooldown.Value.End < curTime;
    }

    public bool TryUseCharge(EntityUid uid, ChargesActionComponent? comp)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (!HasCharges(uid, comp))
            return false;

        var curTime = _timing.CurTime;
        comp.RechargeCooldowns.Dequeue();
        comp.RechargeCooldowns.Enqueue(NewCooldown(comp, curTime));
        Dirty(uid,comp);

        return true;
    }

    private ActionCooldown NewCooldown(ChargesActionComponent comp, TimeSpan start, TimeSpan? end = null)
    {
        if (end is not { })
            end = start + comp.RechargeDuration;

        return new ActionCooldown { Start = start, End = end.Value };
    }
}
