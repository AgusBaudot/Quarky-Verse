/// <summary>
/// Defines the three categories of quantum object behaviour.
/// Set this on SuperpositionController — all other components
/// read from it rather than maintaining their own flags.
/// </summary>
public enum QuantumObjectRole {
    // All positions displayed as ghosts permanently.
    // Cannot be collapsed by the Collapse Glasses.
    // Use for environmental/atmospheric quantum objects.
    Decorative,

    // Standard puzzle object. Collapsable by the Collapse Glasses.
    // Has one real (fully opaque) position at any given time.
    PuzzleObject,

    // Starts behaving as Decorative — all ghosts, not collapsable.
    // A QuantumSwitch in the scene unlocks it mid-game, collapsing
    // it to one specific designer-defined position.
    SwitchActivated
}