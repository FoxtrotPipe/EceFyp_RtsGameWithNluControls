using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitGroupManager : MonoBehaviour
{
    public static UnitGroupManager instance { get; private set; }
    /// <summary>
    /// Dictionary that stores either UnitGroup<Soldier> or UnitGroup<Engineer>
    /// </summary>
    private Dictionary<string, UnitGroup> _groupDictionary = new();
    public HashSet<UnitGroup> SelectedGroups { get; private set; } = new();

    public List<Image> HeadSprites = new();

    public bool IsSelected(UnitGroup group)
    {
        return SelectedGroups.Contains(group);
    }

    public UnitGroup GetGroup(string alias)
    {
        UnitGroup group;
        _groupDictionary.TryGetValue(alias, out group);
        return group;
    }

    public UnitGroup GetFirstNonfullGroup(Party party)
    {
        foreach (var (_, gp) in _groupDictionary)
        {
            if (gp.Party == party && !gp.Full)
            {
                return gp;
            }
        }

        return null;
    }

    public List<UnitGroup> GetAllGroups()
    {
        List<UnitGroup> gpSet = new();

        foreach(var gp in _groupDictionary.Values)
        {
            gpSet.Add(gp);
        }

        return gpSet;
    }

    public void SelectGroup(UnitGroup group)
    {
        if (group.Size > 0)
        {
            SelectedGroups.Add(group);
            group.UpdateSelectEffect(true);
            RenderSelectedUnits();
            UIManager.instance.MoveCameraTo(group);
        }
        else
        {
            Debug.LogWarning("Attempt to select a unit group of size 0");
        }
    }

    public void SelectGroup(List<UnitGroup> groups)
    {
        foreach(var gp in groups) SelectGroup(gp);
    }

    public void DeselectGroup(UnitGroup group)
    {
        SelectedGroups.Remove(group);
        group.UpdateSelectEffect(false);
        RenderSelectedUnits();
    }

    public void DeselectGroup(List<UnitGroup> groups)
    {
        foreach(var gp in groups) DeselectGroup(gp);
    }

    public void DeselectAllGroups()
    {
        foreach (UnitGroup gp in SelectedGroups) 
        {
            gp.UpdateSelectEffect(false);
        }
        SelectedGroups.Clear();
        RenderSelectedUnits();
    }

    // Implement spot making for multiple unit groups
    // public List<TileEntity> MakeSpotsForGroups(List<UnitGroup> groups)
    // {
    //     return new NotImplementedException();
    // }

    public void Register(string alias, UnitGroup group)
    {
        _groupDictionary.Add(alias, group);
    }

    public void Deregister(string alias) 
    {
        _groupDictionary.Remove(alias);
    }

    public void RenderSelectedUnits()
    {
        int spriteIndex = 0;

        foreach (var gp in SelectedGroups)
        {
            for (int index = 0; index < gp.Size; index++)
            {
                HeadSprites[spriteIndex].sprite = UnitManager.instance.GetHeadSprite(gp.UnitType);
                HeadSprites[spriteIndex].enabled = true;
                spriteIndex++;

                if (spriteIndex == HeadSprites.Count)
                {
                    return;
                }
            }
        }

        while(spriteIndex < HeadSprites.Count)
        {
            HeadSprites[spriteIndex].enabled = false;
            spriteIndex++;
        }
    }

    public void Awake() {
        instance = this;
    }
}
