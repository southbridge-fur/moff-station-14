using Content.Shared.Popups;
using Content.Shared.DoAfter;
using Content.Server.Actions;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Store.Systems;
using Content.Shared._Moffstation.Vampire.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.Random;

namespace Content.Server._Moffstation.Vampire.EntitySystems;

public sealed partial class VampireSystem : SharedVampireSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly BloodEssenceUserSystem _bloodEssence = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VampireComponent, MapInitEvent>(OnMapInit);

    }

    private void OnStartup(EntityUid uid, VampireComponent component, ComponentStartup args)
    {
        //todo
    }

    private void OnMapInit(EntityUid uid, VampireComponent component, MapInitEvent args)
    {
        //todo
    }
}
