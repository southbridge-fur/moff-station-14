using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Store.Systems;
using Content.Shared._Moffstation.Vampire;
using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Server.Store.Systems;
using Content.Shared.Store.Components;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

public sealed partial class VampireSystem : SharedVampireSystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VampireComponent, VampireShopOpenEvent>(OnShopOpenAction);
    }

    private void OnMapInit(EntityUid uid, VampireComponent comp, MapInitEvent args)
    {
        _entityManager.RemoveComponent<RespiratorComponent>(uid); // Don't need them to breath
    }

    private void OnShopOpenAction(EntityUid uid, VampireComponent comp, VampireShopOpenEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;
        
        _storeSystem.ToggleUi(uid, uid, store);
    }
}
