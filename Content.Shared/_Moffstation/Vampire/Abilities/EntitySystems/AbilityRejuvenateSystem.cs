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

namespace Content.Shared._Moffstation.Vampire.Abilities.EntitySystems;

/// <summary>
/// This system handles the <see cref="Content.Shared._Moffstation.Vampire.Abilities.Components.AbilityRejuvenateComponent"/>
/// which on initialization here gives the entity the Rejuvenate ability. This also handles that ability's events.
/// </summary>
/// <remarks>
/// todo: Add upgrades
/// </remarks>
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

    /// <summary>
    /// When the Rejuvenate ability is used this method triggers.
    /// It handles:
    ///     - Admin Logging
    ///     - Generating a Popup to the user
    ///     - Playing the sound specified in the component.
    ///     - Reducing stamina damage on the entity. This also handles stuns and knockdown.
    ///     - Reducing drunkenness on the entity.
    ///     - Reducing stutter time on the entity.
    /// </summary>
    /// <remarks>
    /// todo: Add upgrades
    /// todo: Add some kind of visual feedback, perhaps a brief animation or a dim red light effect?
    /// </remarks>
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
