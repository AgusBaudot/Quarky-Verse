using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton. Owns the Planck constant value for the entire session.
/// Drives all quantum behavior globally - every SuperpositionController
/// subscribes to OnThresholdChanged to know whether it should remain active.
///
/// Internally the value is a float [0,1]. It maps to QuantumThreshold.
///
/// OnThresholdChanged firest ONLY when the enum level changes, not on every slider tick - avois per-frame comparisons across all objects.
/// </summary>

// PROTOTYPE NOTE: The Slider in Interactable so designers can test thresholds in Play mode.
// In production set interactable = false and drive value exclusively through SetValue(), called
// by game  events (level completion, puzzle solve, etc)

// PROTOTYPE NOTE: Singleton pattern used for speed.
// Replace with a ScriptableObject event channel before production to decouple from
// scene load order and enable unit testing.

public class PlanckBar : MonoBehaviour
{
    public static PlanckBar Instance { get; private set; }

    [SerializeField] private Slider _slider;

    private QuantumThreshold _currentLevel;
    public QuantumThreshold CurrentLevel => _currentLevel;

    public event Action<QuantumThreshold> OnThresholdChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _slider.minValue = 0;
        _slider.maxValue = 1;
        _slider.value = 1; //World starts fully quantum
        _currentLevel = ToThreshold(_slider.value);
    }

    private void Start()
    {
        _slider.onValueChanged.AddListener(HandleSliderChanged);
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveAllListeners();
    }
    
    //Called by game events (level complete, puzzle solve) in production.
    //The only intended external write path once Slider is non-interactable.
    public void SetValue(float value) => _slider.value = Mathf.Clamp01(value);
    
    //Fires only on boundary crossings - not every tick.
    // E.g. sliding from 0.9 to 0.7 fires nothing; crossing 0.66 fires once.
    private void HandleSliderChanged(float value)
    {
        var newLevel = ToThreshold(value);
        if (newLevel == _currentLevel) return;
        _currentLevel = newLevel;
        OnThresholdChanged?.Invoke(_currentLevel);
    }

    private QuantumThreshold ToThreshold(float value)
    {
        if (value > 0.66f) return QuantumThreshold.Large;
        if (value > 0.33f) return QuantumThreshold.Medium;
        return QuantumThreshold.Small;
    }
}
