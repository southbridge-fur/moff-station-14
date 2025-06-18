using Content.Shared._Moffstation.Vampire.Abilities.Components;
using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Store.Systems;
using Content.Shared._Moffstation.Vampire;
using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

public sealed partial class VampireSystem : SharedVampireSystem
{
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StoreComponent, VampireShopEvent>(OnShopOpenAction);
    }

    private void OnMapInit(Entity<VampireComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp<VampireComponent>(entity, out var comp))
            return;

        RemComp<RespiratorComponent>(entity); // Don't need them to breath

        // give the actions
        _action.AddAction(entity, ref comp.ShopAction, comp.ActionVampireShopProto, entity);

        EnsureComp<AbilityGlareComponent>(entity);
        EnsureComp<AbilityFeedComponent>(entity);
        EnsureComp<AbilityRejuvenateComponent>(entity);
    }

    private void OnShopOpenAction(Entity<StoreComponent> entity, ref VampireShopEvent args)
    {
        if (!TryComp<StoreComponent>(entity, out var store))
            return;

        _storeSystem.ToggleUi(entity, entity, store);
    }

    public void DepositEssence(Entity<VampireComponent> entity, float amount)
    {
        if (amount <= 0.0f)
            return;

        if (!TryComp<VampireComponent>(entity, out var comp))
            return;

        _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                { { comp.BloodEssenceCurrencyPrototype, amount } },
            entity);
    }
}
