using Content.Shared._Moffstation.Vampire.Abilities.Components;
using Content.Server.Stunnable;
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
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbilityGlareComponent, VampireEventGlareAbility>(OnGlare);
        SubscribeLocalEvent<AbilityGlareComponent, MapInitEvent>(OnMapInit);
    }

    public void OnMapInit(Entity<AbilityGlareComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<AbilityGlareComponent>(entity, out var comp))
            return;

        _action.AddAction(entity, ref comp.Action, comp.ActionProto, entity);
    }

    public void OnGlare(Entity<AbilityGlareComponent> entity, ref VampireEventGlareAbility args)
    {
        if (!TryComp<AbilityGlareComponent>(entity, out var comp))
            return;

        (var coords, var facing) = _transform.GetMoverCoordinateRotation(entity, Transform(entity));

        _popup.PopupEntity(Loc.GetString("vampire-glare-alert", ("vampire", entity)), entity, PopupType.Medium);

        _audio.PlayPvs(comp.Sound, entity);
        _entityManager.SpawnAttachedTo(comp.FlashEffectProto, new EntityCoordinates(entity, 0, 0));

        GlareStun(entity, comp, coords, facing, comp.DamageFront, true, true);
        GlareStun(entity, comp, coords, facing + Angle.FromDegrees(-90), comp.DamageSides, true);
        GlareStun(entity, comp, coords, facing + Angle.FromDegrees(90),  comp.DamageSides, true);
        GlareStun(entity, comp, coords, facing + Angle.FromDegrees(180), comp.DamageRear);

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
