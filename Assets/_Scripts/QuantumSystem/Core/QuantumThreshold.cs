/// <summary>
/// Enum representing the three discrete Planck activity levels.
/// Each SuperpositionController declares a minimumThreshold - the
/// lowest Planck level at which it remains quantum-active.
/// Ordered numerically so comparisons use standard < / >= operators.
/// </summary>

public enum QuantumThreshold
{
    Small = 0, //Active even at low Planck values - persists longest
    Medium = 1,
    Large = 2 //Only active at high Planck values - deactivates first
}