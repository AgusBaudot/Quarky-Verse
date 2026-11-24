using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Health Medallion")]
    [SerializeField] private Image[] _heartIcons;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Planck Bar")]
    [SerializeField] private RectTransform _sliderNode;
    [SerializeField] private RectTransform[] _segmentSnapPoints;

    [Header("Quantum Devices")]
    [SerializeField] private Image _gogglesTimerRing;
    [SerializeField] private TextMeshProUGUI _braceletStateText;
    [SerializeField] private Image _particleHemisphere;
    [SerializeField] private Image _waveHemisphere;

    [Header("Goggles Overlay")]
    [SerializeField] private GameObject _visorOverlay;
    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Color _neutralColor = Color.white;
    [SerializeField] private Color _minorColor = Color.cyan;
    [SerializeField] private Color _majorColor = Color.red;

    private void OnEnable()
    {
        // If PlanckBar is migrated to SO Event Channel, replace Instance subscription here
        if (PlanckBar.Instance != null)
            PlanckBar.Instance.OnThresholdChanged += UpdatePlanckVisuals;

        CharacterSwapManager.OnCharacterSwapped += UpdateBraceletState;
        CollapseGlassesController.OnGlassesToggled += ToggleGogglesOverlay;
        CollapseGlassesController.OnTimerUpdated += UpdateGogglesTimer;
        CollapseGlassesController.OnTargetHovered += UpdateCrosshair;
    }

    private void OnDisable()
    {
        if (PlanckBar.Instance != null)
            PlanckBar.Instance.OnThresholdChanged -= UpdatePlanckVisuals;

        CharacterSwapManager.OnCharacterSwapped -= UpdateBraceletState;
        CollapseGlassesController.OnGlassesToggled -= ToggleGogglesOverlay;
        CollapseGlassesController.OnTimerUpdated -= UpdateGogglesTimer;
        CollapseGlassesController.OnTargetHovered -= UpdateCrosshair;
    }

    private void Update()
    {
        //TODO: PlayerHealth integration and event subscription.
    }

    private void UpdateHealthVisuals(int current, int max)
    {
        _healthText.text = $"{current} / {max} HP";
        for (int i = 0; i < _heartIcons.Length; i++)
        {
            _heartIcons[i].enabled = i < current; 
        }
    }

    private void UpdatePlanckVisuals(QuantumThreshold currentLevel)
    {
        int snapIndex = (int)currentLevel;
        if (snapIndex >= 0 && snapIndex < _segmentSnapPoints.Length)
        {
            _sliderNode.position = _segmentSnapPoints[snapIndex].position;
        }
    }

    private void UpdateBraceletState(bool isShodri)
    {
        _braceletStateText.text = isShodri ? "WAVE" : "PARTICLE";
        _particleHemisphere.color = isShodri ? Color.gray : new Color(1f, 0.5f, 0f);
        _waveHemisphere.color = isShodri ? Color.cyan : Color.gray;
    }

    private void ToggleGogglesOverlay(bool isActive)
    {
        _visorOverlay.SetActive(isActive);
        if (!isActive) _gogglesTimerRing.fillAmount = 1f;
    }

    private void UpdateGogglesTimer(float percentage)
    {
        _gogglesTimerRing.fillAmount = percentage;
    }

    private void UpdateCrosshair(QuantumObjectRole? role)
    {
        if (role == null || role == QuantumObjectRole.Decorative)
        {
            _crosshairImage.color = _neutralColor;
            return;
        }
        
        _crosshairImage.color = role == QuantumObjectRole.PuzzleObject ? _minorColor : _majorColor;
    }
}