using System.Collections.Generic;
using RedBjorn.ProtoTiles;
using UnityEngine;

public class ResourceStructure : TileOccupant
{
    static protected ResourceManager _resourceManager = ResourceManager.instance;

    public ResourceType type;
    public override float Health 
    { 
        get { return base.Health; } 
        set 
        {
            base.Health = value; 
            if (!Alive)
            {
                Occupied = null;
                _taskHandler.HandleDespawn(SpawnedObject);
            }
        }
    }
    protected override GameObject SpawnedObject 
    { 
        get { return base.SpawnedObject; }
        set
        {
            base.SpawnedObject = value; 
            SpawnedObject.GetComponent<Transform>().rotation = Quaternion.Euler(0, Random.Range(0, 359), 0);
            _taskHandler = SpawnedObject.GetComponent<ResourceTaskHandler>();
        }
    }
    public bool Empty { get { return Health == 0; } }
    protected ResourceTaskHandler _taskHandler;

    public ResourceStructure(float amount, ResourceType type, TileEntity tile) : base(amount, amount, 0, new List<TileEntity>{ tile }, Party.Neutral)
    {
        this.type = type; 
        SpawnedObject = GameObject.Instantiate(_resourceManager.GetPrefab(type));
    }

    public void HarvestBy(Party party, float amount)
    {
        float prev = Health;
        Health -= amount;
        float gain = prev - Health;

        switch (type)
        {
            case ResourceType.Metal:
                _resourceManager.UpdateAmount(party, ResourceType.Metal, _resourceManager.GetAmount(party, ResourceType.Metal) + gain);
                break;
            case ResourceType.Wood:
                _resourceManager.UpdateAmount(party, ResourceType.Wood, _resourceManager.GetAmount(party, ResourceType.Wood) + gain);
                break;
        }
    }
}