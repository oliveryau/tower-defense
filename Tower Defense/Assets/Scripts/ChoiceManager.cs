using System;
using System.Collections.Generic;
using System.Linq; // LINQ helpers - Where, Any, Sum
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    public static event Action<bool> OnChoicePhaseChanged; // open/close state (true=open, false=closed)
    public static event Action<List<ChoiceData>> OnChoicesGenerated; // rolled choices to UI listeners
    public static event Action OnTowerCapacityChanged;

    [Header("Roll Settings")]
    [SerializeField] private int choiceEveryNWaves;
    [SerializeField] private int optionsPerRoll;
    [SerializeField] private List<ChoiceData> allChoices = new();

    private readonly HashSet<TowerData> _unlockedTowers = new();
    private readonly Dictionary<string, int> _choiceStacks = new(); // tracks how many times each choice id has been picked
    private readonly Dictionary<TowerData, int> _towerCapacityCount = new();
    private readonly List<TowerData> _unlockOrder = new();

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

        if (waveNumber % choiceEveryNWaves != 0) return;
        OpenChoicePhase();
    }

    private void OpenChoicePhase()
    {
        if (_isChoiceOpen) return;

        _isChoiceOpen = true;
        Time.timeScale = 0f;
        OnChoicePhaseChanged?.Invoke(true);

        List<ChoiceData> options = GenerateOptions(optionsPerRoll);
        OnChoicesGenerated?.Invoke(options);
    }

    private List<ChoiceData> GenerateOptions(int count)
    {
        List<ChoiceData> candidates = allChoices.Where(IsOptionValid).ToList(); // check if choice is allowed
        List<ChoiceData> result = new();

        while (result.Count < count && candidates.Count > 0)
        {
            ChoiceData c = CalculateOptionWeight(candidates); // selects one option using weighted randomness
            result.Add(c);
            candidates.Remove(c);
        }

        return result; // return list for UI display
    }

    private bool IsOptionValid(ChoiceData choice)
    {
        if (string.IsNullOrWhiteSpace(choice.id)) return false;

        int currentStacks = _choiceStacks.TryGetValue(choice.id, out int s) ? s : 0; // read current pick count for this id
        if (currentStacks >= choice.maxStacks) return false; // reject if already reached pick limit for this run

        if (choice.ChoiceType == ChoiceType.TowerUpgrade) // extra validation for tower upgrade choices
        {
            if (choice.towerData == null) return false;

            Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None); // get currently placed towers
            bool hasTargetTower = towers.Any(t => t.GetTowerData() == choice.towerData);

            if (!hasTargetTower) return false; // reject buff choice if player has no eligible tower
        }

        return true; // choice passes all checks and can be included in roll pool
    }

    private ChoiceData CalculateOptionWeight(List<ChoiceData> pool) 
    {
        int total = pool.Sum(c => Mathf.Max(1, c.weight)); // Sum all weights (minimum 1 each)
        int roll = UnityEngine.Random.Range(0, total);
        int running = 0;

        foreach (ChoiceData c in pool)
        {
            running += Mathf.Max(1, c.weight);
            if (roll < running) return c;
        }

        return pool[0]; // fallback
    }

    public void PickChoice(ChoiceData picked) // called by UI when player clicks choice
    {
        if (!_isChoiceOpen || picked == null) return;

        ApplyChoice(picked);

        _isChoiceOpen = false;
        Time.timeScale = 1f;
        OnChoicePhaseChanged?.Invoke(false);
    }

    private void ApplyChoice(ChoiceData choice)
    {
        if (!_choiceStacks.ContainsKey(choice.id)) _choiceStacks[choice.id] = 0; // checking how many times picked
        _choiceStacks[choice.id]++;

        switch (choice.ChoiceType)
        {
            case ChoiceType.TowerCapacity: // unlock new tower + capacity
                ApplyTowerCapacity(choice);
                break;

            case ChoiceType.TowerUpgrade: // unlock new tower abilities
                ApplyTowerUpgrade(choice);
                break;

            case ChoiceType.HeroUpgrade: // placeholder
                Debug.Log($"Hero upgrade picked: {choice.displayName}");
                break;

            case ChoiceType.HeroAbility:
                Debug.Log($"Hero ability picked: {choice.displayName}");
                break;
        }
    }

    private void ApplyTowerCapacity(ChoiceData choice)
    {
        if (choice.towerData == null) return;

        _unlockedTowers.Add(choice.towerData);
        if (!_unlockOrder.Contains(choice.towerData)) _unlockOrder.Add(choice.towerData);

        int add = Mathf.Max(0, choice.towerCapacity);
        if (add == 0) add = 1;

        if (!_towerCapacityCount.ContainsKey(choice.towerData)) _towerCapacityCount[choice.towerData] = 0;

        _towerCapacityCount[choice.towerData] += add;
        OnTowerCapacityChanged?.Invoke();
    }

    public int GetTowerCapacityCount(TowerData data)
    {
        if (data == null) return 0;
        return _towerCapacityCount.TryGetValue(data, out int n) ? n : 0;
    }

    public int GetPlacedCount(TowerData data)
    {
        if (data == null) return 0;
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        return towers.Count(t => t.GetTowerData() == data);
    }

    public bool DisplayTowerButtons(TowerData data) => IsTowerUnlocked(data) && GetRemainingPlacements(data) > 0;


    public bool IsTowerUnlocked(TowerData data) => _unlockedTowers.Contains(data);

    public int GetRemainingPlacements(TowerData data)
    {
        return Mathf.Max(0, GetTowerCapacityCount(data) - GetPlacedCount(data));
    }

    public void NotifyTowerPlacedOrRemoved()
    {
        OnTowerCapacityChanged?.Invoke();
    }

    public List<TowerData> GetTowersForButtons()
    {
        var list = new List<TowerData>();
        foreach (TowerData td in _unlockOrder)
        {
            if (DisplayTowerButtons(td))
                list.Add(td);
        }
        Debug.Log(list.Count);
        return list;
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
}