using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceInfo
{
    public float Metal;
    public float Wood;

    public ResourceInfo(float metal, float wood)
    {
        Metal = metal;
        Wood = wood;
    }
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }

    [SerializeField] private TMP_Text woodLabel;
    [SerializeField] private TMP_Text metalLabel;
    [SerializeField] private List<GameObject> _woodPrefabs;
    [SerializeField] private List<GameObject> _metalPrefabs;

    Dictionary<Party, ResourceInfo> ResourceDict = new()
    {
        { Party.Player, new ResourceInfo(50, 50) },
        { Party.CPU, new ResourceInfo(0, 0) }
    };

    public float GetAmount(Party party, ResourceType type)
    {
        return type == ResourceType.Wood ? ResourceDict[party].Wood : ResourceDict[party].Metal;
    }

    public void UpdateAmount(Party party, ResourceType type, float amount)
    {
        if (type == ResourceType.Wood)
        {
            ResourceDict[party].Wood = amount;

            if (party == Party.Player)
            {   
                woodLabel.text = amount.ToString();
            }
        }
        else
        {
            ResourceDict[party].Metal = amount;

            if (party == Party.Player)
            {
                metalLabel.text = amount.ToString();
            }
        }
    }

    public GameObject GetPrefab(ResourceType type) 
    {
        switch (type)
        {
            case ResourceType.Wood:
                return _woodPrefabs[Random.Range(0, _woodPrefabs.Count - 1)];
            case ResourceType.Metal:
                return _metalPrefabs[Random.Range(0, _metalPrefabs.Count - 1)];
        }
        return null;
    }

    public bool CanAffordUnit(Party party)
    {
        if (party == Party.CPU)
        {
            // Cpu can always afford unit
            return true;
        }
        else
        {
            return GetAmount(party, ResourceType.Metal) >= 2;
        }
    }

    public bool CanAffordBuilding(Party party)
    {
        if (party == Party.CPU)
        {
            // Cpu can always afford building
            return true;
        }
        else
        {
            return GetAmount(party, ResourceType.Wood) >= 8;
        }
    }

    public bool ChargeForUnit(Party party)
    {
        if (CanAffordUnit(party))
        {
            UpdateAmount(party, ResourceType.Metal, GetAmount(party, ResourceType.Metal) - 2);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ChargeForBuilding(Party party)
    {
        if (CanAffordBuilding(party))
        {
            UpdateAmount(party, ResourceType.Wood, GetAmount(party, ResourceType.Wood) - 8);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Start()
    {
        foreach (var (party, info) in ResourceDict)
        {
            UpdateAmount(party, ResourceType.Wood, info.Wood);
            UpdateAmount(party, ResourceType.Metal, info.Metal);
        }
    }

    public void Awake() 
    {
        instance = this;
    }
}
