using Content.Shared._Moffstation.Vampire.Abilities.Components;
using Content.Server.Stunnable;
using Content.Shared._Moffstation.Vampire.Events;
using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server._Moffstation.Vampire.Abilities.EntitySystems;

/// <summary>
/// The system for the Glare ability component.
/// </summary>
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

    public void OnMapInit(Entity<AbilityGlareComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<AbilityGlareComponent>(entity, out var comp))
            return;

        _action.AddAction(entity, ref comp.Action, comp.ActionProto, entity);
    }

    /// <summary>
    /// Executed when a vampire performs their Glare attack.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="args"></param>
    public void OnGlare(Entity<AbilityGlareComponent> entity, ref VampireEventGlareAbility args)
    {
        if (!TryComp<AbilityGlareComponent>(entity, out var comp))
            return;

        var (coords,facing) = _transform.GetMoverCoordinateRotation(entity, Transform(entity));
        facing = new Angle(facing.ToWorldVec());

        _popup.PopupEntity(Loc.GetString("vampire-glare-alert", ("vampire", entity)), entity, PopupType.Medium);

        _audio.PlayPvs(comp.Sound, entity);
        SpawnAttachedTo(comp.FlashEffectProto, coords);

        // todo: Make it to where when the vampire is on the ground or restrained, all sides count as a side attack.
        GlareStun(entity, coords, facing, comp.DamageFront, true, true);
        GlareStun(entity, coords, facing + Angle.FromDegrees(-90), comp.DamageSides, true);
        GlareStun(entity, coords, facing + Angle.FromDegrees(90),  comp.DamageSides, true);
        GlareStun(entity, coords, facing + Angle.FromDegrees(180), comp.DamageRear);

        args.Handled = true;
    }

    /// <summary>
    /// Performs a Glare stun on entities in an arc around the user.
    /// </summary>
    /// <param name="user">The user of this ability</param>
    /// <param name="coords">The entity coordinates of the user</param>
    /// <param name="angle">The center angle of the arc, this angle +- 45 degrees,
    /// creates the arc which will be scanned for entities to stun.</param>
    /// <param name="damage">The amount of stamina damage to perform on each valid entity in the arc</param>
    /// <param name="knockdown">Whether to knockdown each valid entity in the arc</param>
    /// <param name="stun">Whether to stun each valid entity in the arc</param>
    /// <remarks>
    /// todo: Add some interaction with flash protection. Not a full nullification of the ability's effects, but
    /// some sort of reduction so it's not useless.
    /// </remarks>
    private void GlareStun(Entity<AbilityGlareComponent> user, EntityCoordinates coords, Angle angle, float damage, bool knockdown = false, bool stun = false)
    {
        if (!TryComp<AbilityGlareComponent>(user, out var comp))
            return;
        var nearbyEntities = _lookup.GetEntitiesInArc(coords, comp.Range, angle, 90, LookupFlags.Uncontained);
        foreach (var target in nearbyEntities)
        {
            if (target == user.Owner)
                continue;
            if (knockdown)
                _stuns.TryKnockdown(target, comp.KnockdownTime, false);
            if (stun)
                _stuns.TryStun(target, comp.StunTime, false);
            _stamina.TakeStaminaDamage(target, damage);
        }
    }
}
