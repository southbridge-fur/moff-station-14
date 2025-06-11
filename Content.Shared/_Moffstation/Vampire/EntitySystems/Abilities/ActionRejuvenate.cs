using Content.Shared._Moffstation.Vampire.Components;
using Content.Shared._Moffstation.Vampire.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Shared._Moffstation.Vampire.EntitySystems.Abilities;

public sealed class ActionRejuvenate : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem  _stamina = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireEventRejuvenate>(OnRejuvenate);
    }

    private void OnRejuvenate(VampireEventRejuvenate args)
    {
        var uid = args.Performer;
        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        _stamina.TakeStaminaDamage(uid, -Math.Abs(args.StamHealing));

        args.Handled = true;
    }
}
