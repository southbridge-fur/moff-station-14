using Content.Server._Moffstation.Vampire.Abilities.Components;
using Content.Server.Stunnable;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared._Moffstation.Vampire.Events;
using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server._Moffstation.Vampire.Abilities.EntitySystems;

public sealed class AbilityGlareSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stuns = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbilityGlareComponent, VampireEventGlareAbility>(OnGlare);
        SubscribeLocalEvent<AbilityGlareComponent, MapInitEvent>(OnMapInit);
    }

    public void OnMapInit(EntityUid uid, AbilityGlareComponent? comp, MapInitEvent args)
    {
        if (!Resolve(uid, ref comp))
            return;

        _action.AddAction(uid, ref comp.Action, comp.ActionProto, uid);
    }

    public void OnGlare(EntityUid uid, AbilityGlareComponent? comp, VampireEventGlareAbility args)
    {
        if (!Resolve(uid, ref comp))
            return;

        (var coords, var facing) = _transform.GetMoverCoordinateRotation(uid, Transform(uid));

        _popup.PopupEntity(Loc.GetString("vampire-glare-alert", ("vampire", uid)), uid, PopupType.Medium);

        _audio.PlayPvs(comp.Sound, uid);

        GlareStun(uid, comp, coords, facing, comp.DamageFront, true, true);
        GlareStun(uid, comp, coords, facing + Angle.FromDegrees(-90), comp.DamageSides, true);
        GlareStun(uid, comp, coords, facing + Angle.FromDegrees(90),  comp.DamageSides, true);
        GlareStun(uid, comp, coords, facing + Angle.FromDegrees(180), comp.DamageRear);

        args.Handled = true;
    }

    private void GlareStun(EntityUid uid, AbilityGlareComponent comp, EntityCoordinates coords, Angle angle, float damage, bool knockdown = false, bool stun = false)
    {
        var nearbyEntities = _lookup.GetEntitiesInArc(coords, comp.Range, angle, 90, LookupFlags.Uncontained);
        foreach (var entity in nearbyEntities)
        {
            if (entity == uid)
                continue;
            if (knockdown)
                _stuns.TryKnockdown(entity, comp.KnockdownTime, false);
            if (stun)
                _stuns.TryStun(entity, comp.StunTime, false);
            _stamina.TakeStaminaDamage(entity, damage);
        }
    }
}
