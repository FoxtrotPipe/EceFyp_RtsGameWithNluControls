using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance { get; private set; }

    private List<Unit> _units = new List<Unit>();

    [SerializeField] private GameObject _soldierPrefab;
    [SerializeField] private GameObject _cpuSoldierPrefab;
    [SerializeField] private GameObject _engineerPrefab;
    [SerializeField] private GameObject _cpuEngineerPrefab;
    [SerializeField] private Sprite _soldierHead;
    [SerializeField] private Sprite _engineerHead;

    public GameObject GetPrefab(UnitType type, Party party = Party.Player) 
    {
        switch (type)
        {
            case UnitType.Soldier:
                return party == Party.Player ? _soldierPrefab : _cpuSoldierPrefab;
            case UnitType.Engineer:
                return party == Party.Player ? _engineerPrefab : _cpuEngineerPrefab;
        }
        return null;
    }

    public Sprite GetHeadSprite(UnitType type)
    {
        switch (type)
        {
            case UnitType.Soldier:
                return _soldierHead;
            case UnitType.Engineer:
                return _engineerHead;
        }
        return null;
    }

    public Unit FindMostRecentUnit() 
    {
        if (_units.Count > 0) {
            return _units[_units.Count - 1];
        }
        else {
            Debug.LogWarning("No _units found in the list.");
            return null;
        }
    }

    public void Register(Unit unit) 
    {
        _units.Add(unit);
    }

    public void Deregister(Unit unit) 
    {
        _units.Remove(unit);
    }

    public void Awake() 
    {
        instance = this;
    }
}
