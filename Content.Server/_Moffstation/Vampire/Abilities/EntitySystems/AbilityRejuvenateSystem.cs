using Content.Server._Moffstation.Vampire.Abilities.Components;
using Content.Shared._Moffstation.Vampire.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Drunk;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Moffstation.Vampire.Abilities.EntitySystems;

public sealed class AbilityRejuvenateSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedDrunkSystem _drunkSystem = default!;
    [Dependency] private readonly SharedStutteringSystem _stuttering = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbilityRejuvenateComponent, VampireEventRejuvenateAbility>(OnRejuvenate);
        SubscribeLocalEvent<AbilityRejuvenateComponent, MapInitEvent>(OnMapInit);
    }

    public void OnMapInit(EntityUid uid, AbilityRejuvenateComponent? comp, MapInitEvent args)
    {
        if (!Resolve(uid, ref comp))
            return;
        _action.AddAction(uid, ref comp.Action, comp.ActionProto, uid);
    }

    private void OnRejuvenate(EntityUid uid, AbilityRejuvenateComponent? comp, VampireEventRejuvenateAbility args)
    {
        if (args.Handled)
            return;

        if (!Resolve(uid, ref comp))
            return;

        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(uid):user} used Rejuvenate.");
        _popup.PopupEntity(Loc.GetString("vampire-rejuvenate-popup"), uid, PopupType.Medium);

        _audio.PlayPvs(comp.Sound, uid);
        _stamina.TakeStaminaDamage(uid, comp.StamHealing);
        _drunkSystem.TryRemoveDrunkenessTime(uid, comp.StatusEffectReductionTime.TotalSeconds);
        _stuttering.DoRemoveStutterTime(uid, comp.StatusEffectReductionTime.TotalSeconds);

        args.Handled = true;
    }
}
