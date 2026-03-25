using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text healthText;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnHealthChanged += UpdateHealthText;
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnHealthChanged -= UpdateHealthText;
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Round: {currentWave + 1}";
    }

    private void UpdateHealthText(int currentHealth)
    {
        healthText.text = $"{currentHealth}";
    }
}
