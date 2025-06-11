using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Content.Server.Actions;
using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Store.Systems;
using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

public sealed partial class VampireSystem : SharedVampireSystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, VampireComponent component, MapInitEvent args)
    {
        _entityManager.RemoveComponent<RespiratorComponent>(uid); // Don't need them to breath
    }
}
