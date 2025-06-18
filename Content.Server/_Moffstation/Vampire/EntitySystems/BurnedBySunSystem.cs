using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared.Damage;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

/// <summary>
/// This system interfaces with the <see cref="Content.Shared._Moffstation.Vampire.Components.BurnedBySunComponent"/>
/// to deal damage to entities with the component when they are exposed to the sun.
/// </summary>
public sealed class BurnedBySunSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefs = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;
        var enumerator = EntityQueryEnumerator<BurnedBySunComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            if (IsInTheSun(uid, comp, time))
                Damage(uid, comp);
        }
    }

    /// <summary>
    /// Checks if the entity is in the sun.
    /// This is done by first checking if we're even on grid and then checking if any of the tiles we're on
    /// are in the blacklist.
    /// </summary>
    /// <returns>True if we are in the sun, false otherwise.</returns>
    /// <remarks>
    /// This could also include a raycast to the "sun" but that could be annoying and without feedback
    /// it would be hard for the user to know they are in the sun. Especially since we don't have sunlight streaming
    /// through windows or anything (although that would be cool to implement :3)
    /// </remarks>
    private bool IsInTheSun(EntityUid uid, BurnedBySunComponent comp, TimeSpan time)
    {
        if (time < comp.NextUpdate)
            return false;

        comp.NextUpdate += comp.UpdateInterval;

        var xform = Transform(uid);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return true; // we aren't on a grid, time to burn

        if (comp.TileBlacklist.Count == 0)
            return false;

        // This could be cached but then if the tile definitions ever change during a round, it will be obsolete.
        var tileBlacklist = new List<ITileDefinition>();
        foreach (var tileProto in comp.TileBlacklist)
        {
            if (_tileDefs.TryGetDefinition(tileProto, out var tileDef))
                tileBlacklist.Add(tileDef);
        }

        var tileRef = _map.GetTileRef(xform.GridUid.Value,
            grid,
            new EntityCoordinates(uid, 0.0f, 0.0f));

        _tileDefs.TryGetDefinition(tileRef.Tile.TypeId, out var currTileDef);
        return currTileDef is null || tileBlacklist.Contains(currTileDef);
    }

    /// <summary>
    /// Causes damage to the entity according to the component's specified damage.
    /// The damage does ramp up to the full amount based upon the Accumulation and the AccumulationPerUpdate
    /// </summary>
    private void Damage(EntityUid uid, BurnedBySunComponent comp)
    {
        // Make it ramp up in severity over time.
        comp.Accumulation = (comp.LastBurn >= comp.NextUpdate - comp.UpdateInterval)
            ? Math.Clamp(comp.Accumulation + comp.AccumulationPerUpdate, 0.0f, 1.0f)
            : 0.0f;

        // todo: give the entity some kind of feedback, like a shader effect
        // todo: Consider bursting into flames
        _damage.TryChangeDamage(uid, comp.Damage * comp.Accumulation);
        comp.LastBurn = _timing.CurTime;
    }
}
