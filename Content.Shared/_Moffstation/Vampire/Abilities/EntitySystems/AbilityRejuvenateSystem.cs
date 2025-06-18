using Content.Shared._Moffstation.Vampire.Abilities.Components;
using Content.Shared._Moffstation.Vampire.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Drunk;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
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

    public void OnMapInit(Entity<AbilityRejuvenateComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<AbilityRejuvenateComponent>(entity, out var comp))
            return;

        _action.AddAction(entity, ref comp.Action, comp.ActionProto, entity);
    }

    private void OnRejuvenate(Entity<AbilityRejuvenateComponent> entity, ref VampireEventRejuvenateAbility args)
    {
        if (args.Handled)
            return;

        if (!TryComp<AbilityRejuvenateComponent>(entity, out var rejuvenateComp))
            return;

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(entity):user} used Rejuvenate.");
        _popup.PopupEntity(Loc.GetString("vampire-rejuvenate-popup"), entity, entity, PopupType.Medium);

        _audio.PlayPvs(rejuvenateComp.Sound, entity);
        _stamina.TakeStaminaDamage(entity, rejuvenateComp.StamHealing);
        _drunkSystem.TryRemoveDrunkenessTime(entity, rejuvenateComp.StatusEffectReductionTime.TotalSeconds);
        _stuttering.DoRemoveStutterTime(entity, rejuvenateComp.StatusEffectReductionTime.TotalSeconds);

        args.Handled = true;
    }
}
