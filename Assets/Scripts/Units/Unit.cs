using RedBjorn.ProtoTiles;
using System;
using RedBjorn.Utils;
using TMPro;
using System.Collections.Generic;

abstract public class Unit : TileOccupant
{
    static protected UnitManager _unitManager = UnitManager.instance;

    public override float Health 
    { 
        get => base.Health; 
        set 
        {
            base.Health = value; 
            if (!Alive)
            {
                Animate("TriggerDead");
                Tile = null;
                Group.Unassign(this);
                UnitTaskHandler.HandleDespawn(SpawnedObject);
            }
        }
    }
    
    protected UnitTaskHandler UnitTaskHandler;
    private UnitGroup _group = null;
    public UnitGroup Group
    {
        get => _group;
        set
        {
            _group = value;

            if (value != null)
            {
                UpdateSelectEffect(value.Selected);
                UpdateLabelEffect(value.Alias);
            }
            else
            {
                UpdateSelectEffect(false);
                UpdateLabelEffect("");
            }
        }
    }

    public Unit(float maxHealth, int range, TileEntity tile, Party party) : base(maxHealth, maxHealth, range, new List<TileEntity>{ tile }, party) 
    {
        _unitManager.Register(this);
    }
    
    /// <summary>
    /// Move to a tile
    /// </summary>
    /// <param name="tile">Target tile</param>
    /// <param name="onMoveToEnd">Callback fired when move operation ends</param>
    public virtual void MoveTo(TileEntity tile, Action onMoveToEnd = null, bool ignoreGroupMembers = true)
    {
        UnitTaskHandler.HandleMoveToTile 
        (
            tile, 
            ignoreGroupMembers ? Group.Occupants : null,
            (TileEntity nextTile) => 
            {
                Tile = nextTile;
                return false;
            }, 
            () => 
            {
                onMoveToEnd.SafeInvoke();
            }   
        );
    }

    /// <summary>
    /// Update the select effect
    /// </summary>
    /// <param name="enabled"></param>
    public void UpdateSelectEffect(bool enabled) 
    { 
        if (UnitTaskHandler.HighlightOutline != null)
        {
            UnitTaskHandler.HighlightOutline.enabled = enabled;
        }
    }

    /// <summary>
    /// Update the label effect
    /// </summary>
    /// <param name="label"></param>
    public void UpdateLabelEffect(string label) 
    { 
        foreach(TMP_Text textObject in UnitTaskHandler.TextLabels) textObject.text = label;
    }

    /// <summary>
    /// Trigger heal effect
    /// </summary>
    public void TriggerHealEffect()
    {
        UnitTaskHandler.HealParticle.Play();
    }
}