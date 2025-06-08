using Content.Shared._Moffstation.Vampire;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Popups;

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

    private void OnGlare(EntityUid uid, VampireComponent component, VampireEventGlare args)
    {
        args.Handled = true;
    }

    private void OnRejuvenate(EntityUid uid, VampireComponent component, VampireEventRejuvenate args)
    {
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
