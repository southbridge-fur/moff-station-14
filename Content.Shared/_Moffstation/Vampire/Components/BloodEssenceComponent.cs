using Robust.Shared.GameStates;

namespace Content.Shared._Moffstation.Vampire.Components;

/// <summary>
/// This component tracks the amount of BloodEssence any particular entity has.
/// The intended use for this is to give it to people who have been fed on by an entity
/// with <see cref="Vampire.Components.BloodEssenceUserComponent"/> using
/// the <see cref="Vampire.Abilities.Components.AbilityFeedComponent"/> in order to track the total Blood Essence
/// that entity has left.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodEssenceComponent : Component
{
    /// <summary>
    /// The total BloodEssence this entity has left.
    /// </summary>
    [DataField]
    public float BloodEssence = 200.0f;

    /// <summary>
    /// Handles withdrawal of blood essence from this component.
    /// </summary>
    /// <param name="withdraw">The amount of BloodEssence to attempt to withdraw from the owner.</param>
    /// <returns>
    /// Returns a value between 0.0 and <see cref="withdraw"/> which corresponds to the amount of BloodEssence withdrawn.
    /// 0.0 being the minimum value if the owner is out of BloodEssence.
    /// </returns>
    public float Withdraw(float withdraw)
    {
        if (BloodEssence < withdraw)
        {
            var withdrawn = BloodEssence;
            BloodEssence = 0.0f;
            return withdrawn;
        }
        BloodEssence -= withdraw;
        return withdraw;
    }
}
