using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance { get; private set; }

    private List<BuildingStructure> _buildings = new List<BuildingStructure>();

    [SerializeField] private GameObject _constructionSite;
    [SerializeField] private GameObject _cpuConstructionSite;
    [SerializeField] private GameObject _basePrefab;
    [SerializeField] private GameObject _cpuBasePrefab;
    [SerializeField] private GameObject _CannonPrefab;
    [SerializeField] private GameObject _cpuCannonPrefab;
    [SerializeField] private GameObject _campfirePrefab;
    [SerializeField] private GameObject _cpuCampfirePrefab;

    public GameObject GetPrefab(BuildingType type, Party party = Party.Player) 
    {
        switch (type)
        {
            case BuildingType.Base:
                return party == Party.Player ? _basePrefab : _cpuBasePrefab;
            case BuildingType.ConstructionSite:
                return party == Party.Player ? _constructionSite : _cpuConstructionSite;
            case BuildingType.Cannon:
                return party == Party.Player ? _CannonPrefab : _cpuCannonPrefab;
            case BuildingType.Campfire:
                return party == Party.Player ? _campfirePrefab : _cpuCampfirePrefab;
        }
        return null;
    }

    public BuildingStructure FindMostRecentBuilding() 
    {
        if (_buildings.Count > 0) {
            return _buildings[_buildings.Count - 1];
        }
        else {
            Debug.LogWarning("No building found in the list.");
            return null;
        }
    }

    // Temp not implement, expect to implement string-to-building dictionary for players to ref towers
    // public BuildingStructure FindBuildingByTag(string tag)
    // {

    // }

    public void Register(BuildingStructure building) 
    {
        _buildings.Add(building);
    }

    public void Deregister(BuildingStructure building) 
    {
        _buildings.Remove(building);
    }

    public void Awake() 
    {
        instance = this;
    }
}