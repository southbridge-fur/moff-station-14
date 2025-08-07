using Content.Server._Moffstation.Objectives.Components;
using Content.Server.Preferences.Managers;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Robust.Shared.Random;

namespace Content.Server._Moffstation.Objectives.Systems;

/// <summary>
/// This handles selecting a high-value target for any systems that select random targets.
/// </summary>
public sealed class HighValueTargetSelectionSystem : EntitySystem
{
    [Dependency] private readonly IServerPreferencesManager _pref = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// Selects a target from the list of possible targets with a priority on those with
    /// the High Value Target role selected.
    /// </summary>
    public bool SelectTarget(
        EntityUid rule,
        HighValueTargetSelectionComponent? component,
        List<Entity<MindComponent>> allPossibleTargets,
        out Entity<MindComponent> target)
    {
        if (!Resolve(rule, ref component))
        {
            target = _random.Pick(allPossibleTargets);
            return true;
        }

        _random.Shuffle(allPossibleTargets);
        var targetQueue = new Queue<Entity<MindComponent>>(allPossibleTargets);

        while (targetQueue.Count > 1)
        {
            var mind = targetQueue.Dequeue();
            if (mind.Comp.UserId is not { } userId)
                continue;

            var pref = (HumanoidCharacterProfile) _pref.GetPreferences(userId).SelectedCharacter;
            if (!pref.AntagPreferences.Contains(component.HighValueTargetPrototype)
                && _random.Prob(component.RerollProbability))
                continue;

            target = mind;
            return true;
        }

        target = targetQueue.Peek();
        return true;
    }
}
