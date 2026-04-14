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
        ChoiceManager.OnChoicePhaseChanged += HandleChoicePhaseChanged;
        ChoiceManager.OnTowerCapacityChanged += RefreshTowerButtons;

        if (choiceScreen != null) choiceScreen.SetActive(false);
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnHealthChanged -= UpdateHealthText;
        GameManager.OnManaChanged -= UpdateManaText;

        ChoiceManager.OnChoicesGenerated -= ShowChoices;
        ChoiceManager.OnChoicePhaseChanged -= HandleChoicePhaseChanged;
        ChoiceManager.OnTowerCapacityChanged -= RefreshTowerButtons;
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

    private void HandleChoicePhaseChanged(bool open)
    {
        if (choiceScreen != null) choiceScreen.SetActive(open);
        if (!open) RefreshTowerButtons();
    }

    private void RefreshTowerButtons()
    {
        if (choiceManager == null) return;

        List<TowerData> towers = choiceManager.GetTowersForButtons();
        for (int i = 0; i < towerButtons.Length; i++)
        {
            bool show = i < towers.Count;
            towerButtons[i].gameObject.SetActive(show);
            if (!show) continue;

            TowerData td = towers[i];
            towerNames[i].text = td.GetDisplayName();
            if (towerCapacityCount != null && i < towerCapacityCount.Length && towerCapacityCount[i] != null) towerCapacityCount[i].text = choiceManager.GetRemainingPlacements(td).ToString();
            towerButtons[i].onClick.RemoveAllListeners();
            TowerData captured = td;
            //towerButtons[i].onClick.AddListener(() =>
            //{
            //    if (towerPlacer != null)
            //        towerPlacer.SelectTower(captured);
            //});
        }
    }
}
