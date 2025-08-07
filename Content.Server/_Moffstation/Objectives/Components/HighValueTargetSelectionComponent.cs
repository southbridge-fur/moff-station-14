using System.ComponentModel.DataAnnotations;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Moffstation.Objectives.Components;

/// <summary>
/// This is used to mark objectives which select random people and prioritize high value targets
/// </summary>
[RegisterComponent]
public sealed partial class HighValueTargetSelectionComponent : Component
{
    /// <summary>
    /// The antag prototype selected from each possible target's preferences
    /// </summary>
    [DataField, Required]
    public ProtoId<AntagPrototype> HighValueTargetPrototype = "HighValueTarget";

    /// <summary>
    /// If the selected individual is not a high value target, how likely are they to be re-rolled?
    /// </summary>
    [DataField]
    public float RerollProbability = 0.8f;
}
