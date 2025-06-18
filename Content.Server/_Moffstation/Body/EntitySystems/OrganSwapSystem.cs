using Content.Server.Antag;
using Content.Shared._Moffstation.Body.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._Moffstation.Body.EntitySystems;

public sealed partial class OrganSwapSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OrganSwapComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedBodySystem)]);
    }

    private void OnMapInit(EntityUid uid, OrganSwapComponent? comp, MapInitEvent args)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        // we want to manipulate the containers ourselves to not cause any side-effects with the organ system.
        foreach ((var bodyPartUid, var bodyPartComp) in _bodySystem.GetBodyChildren(uid, body))
        {
            foreach (var slotName in bodyPartComp.Organs.Keys)
            {
                foreach ((var replaceSlotName, var organProto) in comp.OrganSwaps)
                {
                    if (slotName == replaceSlotName)
                    {
                        if (!TryComp<ContainerManagerComponent>(bodyPartUid, out var containerManager))
                            continue;

                        TrySwapOrgan(_containerSystem.GetContainer(bodyPartUid,
                                SharedBodySystem.GetOrganContainerId(slotName),
                                containerManager),
                            organProto,
                            out var _);

                        Dirty(bodyPartUid,bodyPartComp);
                    }
                }
            }
        }
    }

    private bool TrySwapOrgan(BaseContainer organContainer, EntProtoId organProto, out EntityUid? organ)
    {
        // remove current organs and delete them
        // todo: handle issues with Diona causing the nymphs to spawn when the organs get deleted.
        _containerSystem.CleanContainer(organContainer);
        // spawn new organ and insert it
        return _entityManager.TrySpawnInContainer(organProto, organContainer.Owner, organContainer.ID, out organ);
    }
}
