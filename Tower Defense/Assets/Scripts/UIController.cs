using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Base UI")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text manaText;

    [Header("Choice Screen UI")]
    [SerializeField] private ChoiceManager choiceManager;
    [SerializeField] private GameObject choiceScreen;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceTitles;
    [SerializeField] private TMP_Text[] choiceDescriptions;

    [Header("Tower Options UI")]
    [SerializeField] private Button[] towerButtons;
    [SerializeField] private TMP_Text[] towerNames;
    [SerializeField] private TMP_Text[] towerCapacityCount;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnHealthChanged += UpdateHealthText;
        GameManager.OnManaChanged += UpdateManaText;

        ChoiceManager.OnChoicesGenerated += ShowChoices;
        ChoiceManager.OnChoiceStateChanged += HandleChoiceStateChanged;
        if (choiceScreen != null) choiceScreen.SetActive(false);
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnHealthChanged -= UpdateHealthText;
        GameManager.OnManaChanged -= UpdateManaText;

        ChoiceManager.OnChoicesGenerated -= ShowChoices;
        ChoiceManager.OnChoiceStateChanged -= HandleChoiceStateChanged;
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Round: {currentWave + 1}";
    }

    private void UpdateHealthText(int currentHealth)
    {
        healthText.text = $"{currentHealth}";
    }

    private void UpdateManaText(int currentMana)
    {
        manaText.text = $"{currentMana}";
    }

    private void ShowChoices(List<ChoiceData> choices)
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool active = i < choices.Count;
            choiceButtons[i].gameObject.SetActive(active);
            if (!active) continue;
            ChoiceData c = choices[i];
            choiceTitles[i].text = c.displayName;
            choiceDescriptions[i].text = c.description;
            choiceButtons[i].onClick.RemoveAllListeners();
            ChoiceData picked = c;
            choiceButtons[i].onClick.AddListener(() => choiceManager.PickChoice(picked));
        }
    }

    private void HandleChoiceStateChanged(bool open)
    {
        if (choiceScreen != null) choiceScreen.SetActive(open);
    }
}
