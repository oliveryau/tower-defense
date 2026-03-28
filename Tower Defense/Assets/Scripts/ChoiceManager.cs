using System;
using System.Collections.Generic;
using System.Linq; // LINQ helpers - Where, Any, Sum
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    public static event Action<List<ChoiceData>> OnChoicesGenerated; // rolled choices to UI listeners
    public static event Action<bool> OnChoiceStateChanged; // open/close state (true=open, false=closed)

    [Header("Roll Settings")]
    [SerializeField] private int choiceEveryNWaves = 3;
    [SerializeField] private int optionsPerRoll = 3;
    [SerializeField] private List<ChoiceData> allChoices = new();

    private readonly Dictionary<string, int> _choiceStacks = new(); // tracks how many times each choice id has been picked
    private readonly HashSet<TowerData> _unlockedTowers = new(); // tracks towers unlocked by tower unlocked choices

    private bool _isChoiceOpen = false;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += HandleWaveChanged;
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= HandleWaveChanged;
    }

    private void HandleWaveChanged(int waveIndexZeroBased)
    {
        int waveNumber = waveIndexZeroBased + 1;

        if (waveNumber % choiceEveryNWaves != 0) return; // do nothing if not a milestone wave
        OpenChoicePhase();
    }

    private void OpenChoicePhase()
    {
        if (_isChoiceOpen) return;

        _isChoiceOpen = true;
        Time.timeScale = 0f;
        OnChoiceStateChanged?.Invoke(true); // choice phase is now open

        List<ChoiceData> options = GenerateOptions(optionsPerRoll); // roll options from valid weighted pool
        OnChoicesGenerated?.Invoke(options); // send rolled options to UI for display
    }

    private List<ChoiceData> GenerateOptions(int count)
    {
        List<ChoiceData> candidates = allChoices.Where(IsOptionValid).ToList(); // checking if choice is allowed
        List<ChoiceData> result = new(); // to store final rolled choices

        while (result.Count < count && candidates.Count > 0)
        {
            ChoiceData c = CalculateOptionWeight(candidates); // selects one of the options using weighted randomness
            result.Add(c);
            candidates.Remove(c);
        }

        return result; // return list for UI display
    }

    private bool IsOptionValid(ChoiceData choice)
    {
        if (string.IsNullOrWhiteSpace(choice.id)) return false; // reject choices with empty id

        int currentStacks = _choiceStacks.TryGetValue(choice.id, out int s) ? s : 0; // read current pick count for this id
        if (currentStacks >= choice.maxStacks) return false; // reject if already reached pick limit for this run

        if (choice.ChoiceType == ChoiceType.TowerUpgrade) // extra validation for tower upgrade choices
        {
            if (choice.towerData == null) return false; // reject if upgrade has no target tower type configured

            Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None); // get currently placed/active towers
            bool hasTargetTower = towers.Any(t => t.GetTowerData() == choice.towerData);

            if (!hasTargetTower) return false; // reject buff choice if player has no eligible tower
        }

        return true; // choice passes all checks and can be included in roll pool
    }

    private ChoiceData CalculateOptionWeight(List<ChoiceData> pool) 
    {
        int total = pool.Sum(c => Mathf.Max(1, c.weight)); // Sum all weights (minimum 1 each to avoid zero weight edge case)
        int roll = UnityEngine.Random.Range(0, total);
        int running = 0;

        foreach (ChoiceData c in pool)
        {
            running += Mathf.Max(1, c.weight);
            if (roll < running) return c; // if random roll falls into this choice's range, pick it
        }

        return pool[0]; // fallback
    }

    public void PickChoice(ChoiceData picked) // called by UI when player clicks choice
    {
        if (!_isChoiceOpen || picked == null) return;

        ApplyChoice(picked);

        _isChoiceOpen = false;
        Time.timeScale = 1f;
        OnChoiceStateChanged?.Invoke(false);
    }

    private void ApplyChoice(ChoiceData choice)
    {
        if (!_choiceStacks.ContainsKey(choice.id)) _choiceStacks[choice.id] = 0; // checking how many times picked
        _choiceStacks[choice.id]++;

        switch (choice.ChoiceType)
        {
            case ChoiceType.TowerCapacity: // unlock new tower
                if (choice.towerData != null)
                {
                    _unlockedTowers.Add(choice.towerData);
                    Debug.Log($"Unlocked tower: {choice.towerData.name}"); // Optional debug feedback in Console.
                }
                break;

            case ChoiceType.TowerUpgrade: // unlock new tower abilities
                ApplyTowerUpgrade(choice);
                break;

            case ChoiceType.HeroUpgrade: // placeholder
                Debug.Log($"Hero upgrade picked: {choice.displayName}"); // Temporary log until hero system exists.
                break;
        }
    }

    private void ApplyTowerUpgrade(ChoiceData choice)
    {
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (Tower tower in towers)
        {
            if (tower.GetTowerData() == choice.towerData)
            {
                //tower.ApplyBuff( 
                //    choice.bonusDamage, 
                //    choice.bonusRange, 
                //    choice.bonusAttackSpeedPercent 
                //);
            }
        }

        Debug.Log($"Applied buff to towers of type: {choice.towerData.name}"); // Optional debug confirmation.
    }

    public bool IsTowerUnlocked(TowerData data) => _unlockedTowers.Contains(data); // Utility method for build/shop UI checks.
}