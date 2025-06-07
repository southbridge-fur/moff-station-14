using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.FixedPoint;
using Content.Shared.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Shared._Moffstation.Vampire.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Utility;

namespace Content.Server._Moffstation.Vampire.EntitySystems;
/// <summary>
/// An adapter for handing blood interactions
/// </summary>
public sealed partial class BloodEssenceUserSystem : EntitySystem
{
    [Dependency] private readonly InjectorSystem _injectorSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    /// <summary>
    /// Extracts blood from the target creature and places it in the user's stomach.
    /// This also handles giving the target the BloodEssenceComponent as well as interacts with that component to
    /// pull essence from the target and put it in the user's BloodEssencePool
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="transferAmount"></param>
    /// <param name="target"></param>
    /// <param name="targetBloodstream"></param>
    /// <returns></returns>
    public bool TryExtractBlood(Entity<BloodEssenceUserComponent?,BodyComponent?> uid, FixedPoint2 quantity, EntityUid target, BloodstreamComponent? targetBloodstream)
    {
        if (!(quantity > FixedPoint2.Zero))
            return false;

        var transferAmount = quantity;
        if (!TryComp<BloodEssenceUserComponent>(uid, out var bloodEssenceUser) || !TryComp<BodyComponent>(uid, out var body))
            return false;

        if (!Resolve(target, ref targetBloodstream))
            return false;

        if (!_body.TryGetBodyOrganEntityComps<StomachComponent>((uid, body), out var stomachs))
            return false;

        var firstStomach = stomachs.FirstOrNull(stomach => _stomach.MaxTransferableSolution(stomach, transferAmount) > FixedPoint2.Zero);

        // All stomachs are full or null somehow
        if (firstStomach == null)
            return false;

        transferAmount = _stomach.MaxTransferableSolution(firstStomach.Value, transferAmount);

        if (_solutionContainerSystem.ResolveSolution(target, targetBloodstream.ChemicalSolutionName, ref targetBloodstream.ChemicalSolution, out var targetChemSolution))
        {
            var chemSolution = targetChemSolution.SplitSolution(transferAmount * 0.15f); // make a fraction of what we pull come from the chem solution
            if (!_stomach.TryTransferSolution(firstStomach.Value, chemSolution))
                _solutionContainerSystem.TryAddSolution(targetBloodstream.ChemicalSolution.Value, chemSolution); // put that thing back where it came from or so help me
            else
                transferAmount -= chemSolution.Volume;
            _solutionContainerSystem.UpdateChemicals(targetBloodstream.ChemicalSolution.Value);

        }

        if (_solutionContainerSystem.ResolveSolution(target, targetBloodstream.BloodSolutionName, ref targetBloodstream.BloodSolution, out var targetBloodSolution))
        {
            var bloodSolution = targetBloodSolution.SplitSolution(transferAmount);
            if (!_stomach.TryTransferSolution(firstStomach.Value, bloodSolution))
                _solutionContainerSystem.TryAddSolution(targetBloodstream.BloodSolution.Value, bloodSolution);
            _solutionContainerSystem.UpdateChemicals(targetBloodstream.BloodSolution.Value);
        }

        return true;
    }
}
