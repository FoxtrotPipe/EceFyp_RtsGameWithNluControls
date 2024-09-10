using UnityEngine;
using RedBjorn.ProtoTiles;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

abstract public class TileOccupant
{
    static protected TileManager _tileManager = TileManager.instance;

    private float _health;
    public virtual float Health 
    { 
        get => _health;
        set
        {
            Interlocked.Exchange(ref _health, (value < 0) ? 0 : (value > MaxHealth) ? MaxHealth : value);
            _healthBar?.UpdateBar(_health, MaxHealth);
        } 
    }
    public float MaxHealth { get; protected set; }
    public TileEntity Tile 
    { 
        get => Occupied.Any() ? Occupied[0] : null;
        set
        {
            foreach(var t in _occupied) 
            {
                t.DeregisterOccupant();
            }
            _occupied.Clear();

            if (value != null)
            {
                // Update tile map data
                value.RegisterOccupant(this);

                // Update data member
                _occupied.Add(value);
                _detectTileSet = _tileManager.Area(_occupied, _range);
            }
            else
            {
                _detectTileSet.Clear();
            }
        } 
    }
    private List<TileEntity> _occupied = new();
    public List<TileEntity> Occupied
    {
        get => _occupied;
        protected set 
        {
            foreach(var t in _occupied) 
            {
                t.DeregisterOccupant();
            }
            _occupied.Clear();

            if (value != null)
            {
                foreach(var t in value) 
                {
                    // Update tile map data
                    t.RegisterOccupant(this);

                    // Update data member
                    _occupied.Add(t);
                }
                
                _detectTileSet = _tileManager.Area(_occupied, _range);
            }
            else
            {
                _detectTileSet.Clear();
            }
        }
    }
    public Vector3 Center
    {
        get 
        {
            Vector3 result = Vector3.zero; 
            foreach(var t in Occupied) result += _tileManager.ToWorldPosition(t.Position);
            return result / Occupied.Count;
        }
    }
    public Party Party { get; protected set; }
    public bool Alive { get => Health > 0; }
    private int _range;
    private List<TileEntity> _detectTileSet = new();
    public List<TileOccupant> DetectableOccupants
    {
        get
        {
            List<TileOccupant> set = new();
        
            foreach(var t in _detectTileSet)
            {
                if (t != Tile && t.Occupant != null)
                {
                    set.Add(t.Occupant);
                }
            }

            return set;
        }
    }
    public List<TileOccupant> DetectableHostiles
    {
        get => DetectableOccupants.Where(o => IsHostile(o)).ToList();
    }
    public List<TileOccupant> DetectableFriendlies
    {
        get => DetectableOccupants.Where(o => !IsHostile(o)).ToList();
    }
    public TileOccupant NearestHostile
    {
        get
        {
            TileOccupant o = null;
            List<TileOccupant> hostiles = DetectableHostiles;
            float minDist = -1;

            if (hostiles != null)
            {
                foreach (var hostile in hostiles)
                {
                    float dist = _tileManager.Distance(Tile, hostile.Occupied);

                    if (minDist == -1 || dist < minDist)
                    {
                        minDist = dist;
                        o = hostile;
                    }
                }
            }

            return o;
        }
    }
    public TileEntity NearestVacantTile
    {
        get => _tileManager.WalkableTile(Tile);
    }
    
    private GameObject _spawnedObject;
    protected virtual GameObject SpawnedObject
    {
        get => _spawnedObject;
        set 
        {
            GameObject.Destroy(_spawnedObject);
            _spawnedObject = value;
            _spawnedObject.GetComponent<Transform>().position = _tileManager.ToWorldPosition(Tile.Position);
            _animator = _spawnedObject.GetComponentInChildren<Animator>();
            _healthBar = _spawnedObject.GetComponentInChildren<Bar>();
            _healthBar.UpdateBar(_health, MaxHealth);
        }
    }
    private Animator _animator;
    private Bar _healthBar;

    public TileOccupant(float health, float maxHealth, int range, List<TileEntity> tileSet, Party party)
    {
        this.MaxHealth = maxHealth;
        this.Health = health;
        _range = range;
        Occupied = tileSet;
        this.Party = party;
    }

    public bool WithinRange(TileOccupant occupant)
    {
        return _tileManager.Distance(Tile, occupant.Occupied) <= _range;
    }

    public bool IsHostile(TileOccupant occupant)
    {
        return occupant.Party != Party.Neutral && occupant.Party != Party;
    }

    public void Animate(string key) 
    {
        if (_animator != null)
        {
            _animator.SetTrigger(key);
        }
        else
        {
            Debug.LogWarning("Attempt to trigger " + key + " animation when GameObject has no animator");
        }
    }
}