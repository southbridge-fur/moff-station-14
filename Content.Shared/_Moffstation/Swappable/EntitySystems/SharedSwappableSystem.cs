using Content.Shared._Moffstation.Swappable;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;

namespace Content.Shared._Moffstation.EntitySystems.Swappable;

public sealed class SharedSwappableSystem : EntitySystem
{
    [Dependency]
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwappableComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SwappableComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
    }

    private void OnMapInit(Entity<SwappableComponent> entity, ref MapInitEvent args)
    {

    }

    private void OnGetAlternativeVerb(Entity<SwappableComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        args.User

        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("swappable-verb"),
            Act = () =>
            {

            },
            Disabled = component.IsConnected,
        });
    }

    public void UpdateUserInterface(Entity<SwappableComponent?> entity, EntityUid? user)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;
    }


}
