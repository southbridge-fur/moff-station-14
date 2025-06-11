using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared._Moffstation.Vampire.Events;

namespace Content.Server._Moffstation.Vampire.EntitySystems.Abilities;

// todo: consider moving this to a generic "bloodsucker" or "bloodfeeder" component/system
public sealed class ActionFeed : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly BloodEssenceUserSystem _bloodEssence = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, VampireEventFeedAbility>(OnFeedStart);
        SubscribeLocalEvent<VampireComponent, VampireEventFeedAbilityDoAfter>(OnFeedEnd);
    }

    private void OnFeedStart(EntityUid uid, VampireComponent? component, VampireEventFeedAbility args)
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

        var feedDoAfter = new DoAfterArgs(EntityManager, uid, args.FeedDuration, new VampireEventFeedAbilityDoAfter(), uid, target: target)
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

    private void OnFeedEnd(EntityUid uid, VampireComponent component, VampireEventFeedAbilityDoAfter args)
    {
        // if canceled return (maybe spray blood?)
        if (args.Handled || args.Cancelled)
            return;

        if (!TryComp<VampireComponent>(uid, out var vampire))
            return;

        var target = args.Args.Target;
        if (target == null || !TryComp<BloodstreamComponent>(target, out var targetBloodstream))
            return;

        if (_bloodEssence.TryExtractBlood(uid, args.BloodPerFeed, target.Value, targetBloodstream))
        {
            _popup.PopupEntity(Loc.GetString("vampire-feeding-successful-vampire", ("target", target)), uid, uid, PopupType.Medium);
            _popup.PopupEntity(Loc.GetString("vampire-feeding-successful-target", ("vampire", uid)), uid, target.Value, PopupType.MediumCaution);
        }

        args.Handled = true;
    }
}
