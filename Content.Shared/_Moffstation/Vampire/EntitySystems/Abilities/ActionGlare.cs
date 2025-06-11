using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared._Moffstation.Vampire.Events;
using Content.Shared.Damage.Systems;
using Robust.Shared.Map;

namespace Content.Shared._Moffstation.Vampire.EntitySystems.Abilities;

public sealed class ActionGlare : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireEventGlareAbility>(OnGlare);
    }

    private void OnGlare(VampireEventGlareAbility args)
    {
        var uid = args.Performer;

        (var coords, var facing) = _transform.GetMoverCoordinateRotation(uid, Transform(uid));

        GlareStun(uid, coords, facing - Angle.FromDegrees(-45), args.DamageFront, args.Range);
        GlareStun(uid, coords, facing - Angle.FromDegrees(-135), args.DamageSides, args.Range);
        GlareStun(uid, coords, facing - Angle.FromDegrees(45), args.DamageSides, args.Range);
        GlareStun(uid, coords, facing - Angle.FromDegrees(135), args.DamageRear, args.Range);

        args.Handled = true;
    }

    private void GlareStun(EntityUid uid, EntityCoordinates coords, Angle angle, float damage, float range)
    {
        var nearbyEntities = _lookup.GetEntitiesInArc(coords, range, angle, 90, LookupFlags.Uncontained);
        foreach (var entity in nearbyEntities)
        {
            if (entity == uid)
                continue;
            _stamina.TakeStaminaDamage(entity, damage);
        }
    }

}
