using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RTS_Cam;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    private enum PromptState { Normal, PromptIntent, PromptBuilding, PromptTile, PromptGroup }
    private PromptState _promptState = PromptState.Normal;
    private (string intent, List<UnitGroup> groups, TileEntity tile, BuildingType? building) _prevParams;

    private bool _paused = false;
    [SerializeField] private GameObject PauseMenuUI;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button ControlButton;
    [SerializeField] private Button HideControlButton;
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Button QuitButton;
    
    [SerializeField] private GameObject EndMenuUI;
    [SerializeField] private TMP_Text EndGameLabel;
    [SerializeField] private GameObject ControlMenuUI;

    [SerializeField] private RTS_Camera MainCamera;

    [SerializeField] private Bar PlayerBaseHealth;
    [SerializeField] private Bar CpuBaseHealth;

    [SerializeField] private TMP_InputField InputField;
    [SerializeField] private KeyCode FocusOnInputFieldKey = KeyCode.Return;
    [SerializeField] private RTS_Camera Camera;
    [SerializeField] private KeyCode FocusOnMovementKey = KeyCode.RightShift;

    [SerializeField] private Image DisabledOverlay;

    // private Vector3 _targetCamPos = MainCamera.transform.position;
    // private float _camSpeed = 1f;

    public TileEntity GetTileInWorld(string screenTile)
    {
        var col = screenTile[0] - 'a';
        var row = screenTile[1] - '1';
        
        float x = ((Screen.width - 40) / 8) * (col + 0.5f) + 40;
        float y = ((Screen.height - 40 - 250) / 6) * (row + 0.5f) + 250;
        var clickPos = MyInput.GroundPositionOnScreen(x, y, TileManager.instance.MapSettings.Plane());
        var tile = TileManager.instance.Map.Tile(clickPos);

        return tile;
    }

    public void MoveCameraTo(TileOccupant occupant)
    {
        MainCamera.SetTarget(occupant);
    }

    public void MoveCameraTo(UnitGroup group)
    {
        if (group.Size > 0)
        {
            MainCamera.SetTarget(group.Occupants[0]);
        }
    }

    public void UpdateBaseHealth(float health, Party party)
    {
        if (party == Party.Player)
        {
            PlayerBaseHealth.UpdateBar(health, 6f * 20);
        }
        else if (party == Party.CPU)
        {
            CpuBaseHealth.UpdateBar(health, 6f * 20);
        }
    }

    public void UpdateInputField(bool interactable)
    {
        InputField.interactable = interactable;
        DisabledOverlay.enabled = !interactable;
    }

    public void FocusInputField()
    {
        Camera.useKeyboardInput = false;
        InputField.Select();
        InputField.ActivateInputField();
    }

    public void DefocusInputField()
    {
        Camera.useKeyboardInput = true;
        InputField.DeactivateInputField();
    }

    public void RefreshInputField()
    {
        InputField.Select();
        InputField.ActivateInputField();
        InputField.text = "";
    }

    public void ShowEndMenu(bool playerWin)
    {
        EndGameLabel.SetText(playerWin ? "You win!" : "You lose...");
        EndMenuUI.SetActive(true);
    }

    private string ToVerb(string intent)
    {
        switch (intent)
        {
            case "select_unit": return "select";
            case "cancel_select_unit": return "unselect";
            case "move_to": return "move";
            case "attack": return "attack";
            case "build_structure": return "build this structure";
            case "gather_resources": return "gather this resource";
        }

        return null;
    }

    private PromptState SetPromptTo(PromptState state, (string intent, List<UnitGroup> groups, TileEntity tile, BuildingType? building) param)
    {   
        if (state == PromptState.Normal)
        {
            ToastNotification.Hide();
            return PromptState.Normal;
        }
        else
        {
            // Prompt for the param two times and still fail to recognize
            if (state == _promptState)
            {
                ToastNotification.Show("Please try to rephrase. We cannot understand your intent.", 0f, "error");
                return PromptState.Normal;
            }
            // Prompt for the param first time
            else
            {
                switch (state)
                {
                    case PromptState.PromptIntent:
                        ToastNotification.Show("What order do you intend to execute?", 0f, "alert"); break;
                    case PromptState.PromptBuilding:
                        ToastNotification.Show("What building do you want to build?", 0f, "alert"); break;
                    case PromptState.PromptTile:
                        ToastNotification.Show("Where do you wish to " + ToVerb(param.intent) + "?", 0f, "alert"); break;
                    case PromptState.PromptGroup:
                        if (param.intent == "select_unit" || param.intent == "cancel_select_unit")
                        {
                            ToastNotification.Show(char.ToUpper(ToVerb(param.intent).First()) + ToVerb(param.intent)[1..].ToLower() + " which division?", 0f, "alert");
                        }
                        else
                        {
                            ToastNotification.Show("Order which division to " + ToVerb(param.intent) + "?", 0f, "alert");
                        }
                        break;
                }
                return state;
            }
        }
        
    }

    private PromptState RequireTileAndGroupAction(string intent, TileEntity tile, List<UnitGroup> groups, Action<UnitGroup> groupAction)
    {
        if (tile != null) 
        {
            if (groups != null) 
            {
                UnitGroupManager.instance.DeselectAllGroups();
                UnitGroupManager.instance.SelectGroup(groups);
                MoveCameraTo(groups[0]);
                foreach(var gp in groups) groupAction(gp);

                return SetPromptTo(PromptState.Normal, (intent, groups, tile, null));
            }
            else 
            {
                if (UnitGroupManager.instance.SelectedGroups.Any())
                {
                    foreach(var gp in UnitGroupManager.instance.SelectedGroups) groupAction(gp);
                    MoveCameraTo(UnitGroupManager.instance.SelectedGroups.ElementAt(0));

                    return SetPromptTo(PromptState.Normal, (intent, groups, tile, null));
                }
                else
                {
                    Debug.LogWarning("[Agent]: Missing unit / no selected unit(s) in " + intent + " intent");
                    return SetPromptTo(PromptState.PromptGroup, (intent, groups, tile, null));
                }
            }
        }
        else 
        {
            Debug.LogWarning("[Agent]: Missing / out of map (invalid) location in " + intent + " intent");
            return SetPromptTo(PromptState.PromptTile, (intent, groups, tile, null));
        }
    }

    // Process response and return next prompt state
    private PromptState ProcessResponse(string intent, List<UnitGroup> groups, TileEntity tile, BuildingType? building)
    {
        _prevParams = (intent, groups, tile, building);
        RefreshInputField();

        switch (intent)
        {
            case "select_unit":
                if (groups != null)
                {
                    UnitGroupManager.instance.DeselectAllGroups();
                    UnitGroupManager.instance.SelectGroup(groups);
                    return SetPromptTo(PromptState.Normal, (intent, groups, tile, building));
                }
                else
                {
                    Debug.LogWarning("[Agent]: Missing unit in select_unit intent");
                    return SetPromptTo(PromptState.PromptGroup, (intent, groups, tile, building));
                }
            case "cancel_select_unit":
                if (groups != null)
                {
                    UnitGroupManager.instance.DeselectGroup(groups);
                    return SetPromptTo(PromptState.Normal, (intent, groups, tile, building));
                }
                else
                {
                    UnitGroupManager.instance.DeselectAllGroups();
                    return SetPromptTo(PromptState.Normal, (intent, groups, tile, building));
                }
            case "build_structure":
                if (building != null)
                {
                    if (ResourceManager.instance.CanAffordBuilding(Party.Player))
                    {
                        return RequireTileAndGroupAction("build_structure", tile, new List<UnitGroup>() { UnitGroupManager.instance.GetGroup("4") }, (UnitGroup gp) => gp.OrganiseToBuild(tile, (BuildingType)building));
                    }
                    else
                    {
                        Debug.LogWarning("[Agent]: Insufficient resource to start building");
                        ToastNotification.Show("Insufficient resource to start building", 5f, "info");
                        return PromptState.Normal;
                    }
                }
                else
                {
                    Debug.LogWarning("[Agent]: Missing building type in build_structure intent");
                    return SetPromptTo(PromptState.PromptBuilding, (intent, groups, tile, building));
                }
            case "move_to": 
                return RequireTileAndGroupAction("move_to", tile, groups, (UnitGroup gp) => gp.Rally(tile));
            case "attack": 
                return RequireTileAndGroupAction("attack", tile, groups, (UnitGroup gp) => gp.Rally(tile));
            case "gather_resources": 
                return RequireTileAndGroupAction("gather_resources", tile, new List<UnitGroup>() { UnitGroupManager.instance.GetGroup("4") }, (UnitGroup gp) => gp.OrganiseToHarvest(tile));
        }

        return SetPromptTo(PromptState.Normal, (intent, groups, tile, building));
    }

    private (string intent, List<UnitGroup> groups, TileEntity tile, BuildingType? building) ParseResponse(ReceiveMessageJson response)
    {
        string intent_str = response.intent.name;
        string entity_str = "";
        List<UnitGroup> groups = null;
        TileEntity tile = null;
        BuildingType? building = null;
        
        foreach (EntityInfo entityInfo in response.entities) {
            string key = entityInfo.entity;
            string value = entityInfo.value.ToLower();

            entity_str += key + ": " + value + ", ";

            if (!string.IsNullOrEmpty(value))
            {
                switch (key)
                {
                    case "unit":
                        var matchNum = Regex.Match(value, @"\d+");

                        if (matchNum.Success)
                        {
                            var gp = UnitGroupManager.instance.GetGroup(matchNum.Value);

                            if (gp != null)
                            {
                                groups = new List<UnitGroup>() { gp };
                            }
                        }
                        else
                        {
                            var matchAll = Regex.Match(value, @"\ball\b", RegexOptions.IgnoreCase);

                            if (matchAll.Success)
                            {
                                groups = UnitGroupManager.instance.GetAllGroups();
                            }
                        }

                        break;
                    case "location":
                        tile = GetTileInWorld(value);
                        break;
                    case "building":
                        building = value == "cannon" ? BuildingType.Cannon : BuildingType.Campfire;
                        break;
                }
            }
        }

        Debug.Log("intent= " + intent_str + ", entity_str=" + entity_str);
        
        return (intent_str, groups, tile, building);
    }

    private void OnNluRespond(ReceiveMessageJson response) 
    {
        var (intent, groups, tile, building) = ParseResponse(response);

        switch (_promptState)
        {
            case PromptState.Normal:
                _promptState = ProcessResponse(intent, groups, tile, building);
                break;
            case PromptState.PromptIntent:
                _promptState = ProcessResponse(intent, _prevParams.groups, _prevParams.tile, _prevParams.building);
                break;
            case PromptState.PromptBuilding:
                _promptState = ProcessResponse(_prevParams.intent, _prevParams.groups, _prevParams.tile, building);
                break;
            case PromptState.PromptTile:
                _promptState = ProcessResponse(_prevParams.intent, _prevParams.groups, tile, _prevParams.building);
                break;
            case PromptState.PromptGroup:
                _promptState = ProcessResponse(_prevParams.intent, groups, _prevParams.tile, _prevParams.building);
                break;
        }
    }

    private void OnInputFieldEndEdit() 
    {
        string message = InputField.text;

        if (!string.IsNullOrEmpty(message))
        {
            NluManager.instance.Parse(message, OnNluRespond);
        }

        DefocusInputField();
    }

    private void OnSelectInputField()
    {
        FocusInputField();
    }

    private void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        _paused = false;
    }
    
    private void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        _paused = true;
    }
    
    private void ShowControl()
    {
        ControlMenuUI.SetActive(true);
        PauseMenuUI.SetActive(false);
    }

    private void HideControl()
    {
        ControlMenuUI.SetActive(false);
        PauseMenuUI.SetActive(true);
    }

    private void BackMain()
    {
        SceneManager.LoadScene("Start");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (Input.GetKeyDown(FocusOnInputFieldKey))
        {
            FocusInputField();
        }

        if (Input.GetKeyDown(FocusOnMovementKey))
        {
            DefocusInputField();
        }
    }

    public void Start()
    {
        UpdateBaseHealth(6f * 20, Party.Player);
        UpdateBaseHealth(6f * 20, Party.CPU);

        ResumeButton.onClick.AddListener(Resume);
        ControlButton.onClick.AddListener(ShowControl);
        HideControlButton.onClick.AddListener(HideControl);
        MainMenuButton.onClick.AddListener(BackMain);
        QuitButton.onClick.AddListener(QuitGame);
        InputField.onSelect.AddListener(delegate{OnSelectInputField();});
        InputField.onEndEdit.AddListener(delegate{OnInputFieldEndEdit();});

        ToastNotification.Show("Type what you'd like to do in the prompt box to start interacting with the game.", 15f, "info");
        DefocusInputField();

        Resume();
    }

    public void Awake()
    {
        instance = this;
    }
}