using Content.Shared._Moffstation.Vampire;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

public sealed partial class VampireSystem : SharedVampireSystem
{
    private void InitializeBasicAbilities()
    {
        SubscribeLocalEvent<VampireComponent, VampireEventGlare>(OnGlare);
        SubscribeLocalEvent<VampireComponent, VampireEventRejuvenate>(OnRejuvenate);
        SubscribeLocalEvent<VampireComponent, VampireEventFeed>(OnFeedStart);
        SubscribeLocalEvent<VampireComponent, VampireEventFeedDoAfter>(OnFeedEnd);
    }

    private void OnGlare(EntityUid uid, VampireComponent? component, VampireEventGlare args)
    {
        if (!Resolve(uid, ref component))
            return;

        // todo: make generic arc attacks and just do 4 of them at once.
        (var coords, var facing) = _transform.GetMoverCoordinateRotation(uid, Transform(uid));

        GlareStun(uid, component, coords, facing - Angle.FromDegrees(-45), component.GlareDamageFront);
        GlareStun(uid, component, coords, facing - Angle.FromDegrees(-135), component.GlareDamageSides);
        GlareStun(uid, component, coords, facing - Angle.FromDegrees(45), component.GlareDamageSides);
        GlareStun(uid, component, coords, facing - Angle.FromDegrees(135), component.GlareDamageRear);

        args.Handled = true;
    }

    private void GlareStun(EntityUid uid, VampireComponent component, EntityCoordinates coords, Angle angle, float damage)
    {
        var nearbyEntities = _lookup.GetEntitiesInArc(coords, component.GlareRange, angle, 90, LookupFlags.Uncontained);
        foreach (var entity in nearbyEntities)
        {
            if (entity == uid)
                continue;
            _stamina.TakeStaminaDamage(entity, damage);
        }
    }

    private void OnRejuvenate(EntityUid uid, VampireComponent? component, VampireEventRejuvenate args)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        _stamina.TakeStaminaDamage(uid, -Math.Abs(component.RejuvenateStamHealing));

        args.Handled = true;
    }

    private void OnFeedStart(EntityUid uid, VampireComponent? component, VampireEventFeed args)
    {
        if (!Resolve(uid, ref component))
            return;

        if (args.Handled)
            return;

        args.Handled = true;

        if (args.Target == args.Performer)
            return;

        var target = args.Target;

        if (!HasComp<BloodstreamComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("vampire-target-has-no-bloodstream", ("target", target)),
                uid,
                uid,
                PopupType.Medium);
            return;
        }

        var feedDoAfter = new DoAfterArgs(EntityManager, uid, component.FeedDuration, new VampireEventFeedDoAfter(), uid, target: target)
        {
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            BreakOnDamage = true,
        };

        if (!_doAfter.TryStartDoAfter(feedDoAfter))
            return;

        _popup.PopupEntity(Loc.GetString("vampire-feeding-on-vampire", ("target", target)), uid, uid, PopupType.Medium);
        _popup.PopupEntity(Loc.GetString("vampire-feeding-on-target", ("vampire", uid)), uid, target, PopupType.LargeCaution);
    }

    private void OnFeedEnd(EntityUid uid, VampireComponent component, VampireEventFeedDoAfter args)
    {
        // if canceled return (maybe spray blood?)
        if (args.Handled || args.Cancelled)
            return;

        if (!TryComp<VampireComponent>(uid, out var vampire))
            return;

        var target = args.Args.Target;
        if (target == null || !TryComp<BloodstreamComponent>(target, out var targetBloodstream))
            return;

        if (_bloodEssence.TryExtractBlood(uid, vampire.BloodPerFeed, target.Value, targetBloodstream))
        {
            _popup.PopupEntity(Loc.GetString("vampire-feeding-successful-vampire", ("target", target)), uid, uid, PopupType.Medium);
            _popup.PopupEntity(Loc.GetString("vampire-feeding-successful-target", ("vampire", uid)), uid, target.Value, PopupType.MediumCaution);
        }

        args.Handled = true;
    }
}
