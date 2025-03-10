﻿using UnityEngine;
using System.Collections.Generic;

public class GameSettings : MonoBehaviour
{

			/*
		⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
		⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠛⠛⠉⠉⠉⠀⠀⠀⠉⠉⠉⠛⠛⠛⠿⣿⣿⣿⣿⣿
		⣿⣿⣿⣿⠿⠋⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⢿⣿⣿
		⣿⣿⡆⢀⣤⡄⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⣿⣿
		⣿⣿⣇⡀⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡼⠟⢫⣿⣿
		⣿⣿⣿⣿⣿⣶⣄⣀⣀⣀⠀⣀⣠⣤⡤⠤⠀⠀⠀⠀⠀⠀⠀⡴⠊⠁⠀⣼⣿⣿
		⣿⣿⣿⡇⠀⠉⠙⠻⠿⠿⠿⣿⣿⡀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠁⠀⠀⢠⣿⣿⣿
		⣿⣿⣿⠁⠀⠀⠀⠀⠀⢀⣾⣿⣿⣿⣶⣤⣀⡀⠀⠀⠶⣿⣷⣶⣶⣿⣿⣿⣿⣿
		⣿⣿⣿⣄⣀⣀⣀⣤⣶⣿⣿⣿⣿⣿⣿⣿⣿⣷⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿
		⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
		⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
		⠀⠀⣀⣴⣴⡾⠿⠶⣶⣶⣶⣦⣤⠀⠀⠀⢠⣤⣴⣶⣶⣶⠶⢶⣶⣴⣄⠀⠀⠀
		⠀⠀⠑⠋⠁⢀⣐⣒⣒⣒⣒⣢⡄⠀⠀⠀⠀⣀⢶⣶⣶⣶⣶⣀⠀⠉⠓⠁⠀⠀
		⠀⠀⠀⠀⠶⢭⣸⣿⣿⣟⣹⡁⠏⠀⠀⠀⠘⠇⣹⣙⣿⣿⣿⣨⠷⠄⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⠋⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠀⣰⠃⠀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⣦⢤⣤⣤⠤⠤⠤⠤⠴⠒⠒⠋⠁⠀⠀⡇⠀⠀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠇⠀⠀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
		⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ 
		*/

    public enum Resource
    {
        Oil,
        Steel,
        Alluminum,
        Rubber
    }

    public bool developerMode;
	public bool spectatorMode;
	public bool developerCheats;
	public bool diplomatyCheats;
	public bool onlineGame;
	public bool jsonTest;
	public bool regionSelectionMode;

	public BoolValue _isPremium;

	public string regionSelectionModeType;

	public List<RegionManager> provincesList = new List<RegionManager>();
	public Color provincesListColor;
	public int provincesListMax;

	public UnitMovement.BattleInfo currentBattle;

	public StringValue _currentGameMode;
	public StringValue _currentTournamentCountries;

    [Header("DEBUG")]
	public bool _DEBUG_REGIONS_IDS;
	public GameObject debugText;

	public Color _yellowColor = new(0.81f, 0.26f, 0.125f, 1f);
	public Color greenColor;
	public Color blueColor = new (0.21f, 0.78f, 0.1f, 1f);
	public Color redColor;

	public GameObject playersListsPanel;
	public Transform playersListContent;
	public GameObject playerListPrefab;
	public GameObject playerListButton;

	public UnitScriptableObject soldierLVL1;
	public UnitScriptableObject soldierLVL2;
	public UnitScriptableObject soldierLVL3;

	public UnitScriptableObject artileryLVL1;
	public UnitScriptableObject artileryLVL2;

	public UnitScriptableObject tankLVL1;
	public UnitScriptableObject tankLVL2;
	public UnitScriptableObject tankLVL3;

	public UnitScriptableObject motoLVL1;
	public UnitScriptableObject motoLVL2;

	public UnitScriptableObject antitankLVL1;
	public UnitScriptableObject antitankLVL2;

    public UnitScriptableObject antiairLVL1;
    public UnitScriptableObject antiairLVL2;

    public UnitScriptableObject cavLVL1;
    public UnitScriptableObject cavLVL2;

    public BuildingScriptableObject fabric;
	public BuildingScriptableObject farm;
	public BuildingScriptableObject chefarm;
	public BuildingScriptableObject researchLab;
	public BuildingScriptableObject dockyard;

	public List<UnitHealth> currentDefenseUnits_FirstLevel = new(10);

	[HideInInspector]
	public DiplomatyUI diplomatyUI;

	[HideInInspector]
	public CountryManager countryManager;

	[TextArea(0, 30)]
	public string DIPLOMATY_UI_INFO_TEXT;

	[TextArea(30, 30)]
	public string BUILDING_INFO_TEXT;

	[TextArea(0, 30)]
	public string COUNTRY_UI_INFO_TEXT;

	[TextArea(0, 30)]
	public string NO_RECROOTS;

	[TextArea(0, 30)]
	public string NO_GOLD;

	[TextArea(0, 30)]
	public string NO_MOVEPOINTS;

	public int[] mobilizationPercent;
	public float _regionOpacity;

	public int fuelPerOil = 65;

    public enum Language
    {	
        EN,
		RU
    }

	public Language _language;

    [Header("SFX Sounds")]
	public AudioClip[] m_heavy_move;
	public AudioClip[] m_infantry_move;
	public AudioClip[] m_motorized_infantry_move;
	public AudioClip m_new_event_01;
	public AudioClip m_paper_01;

	[Header("Game Events")]
	public bool allowGameEvents;
	public List<EventScriptableObject> gameEvents = new List<EventScriptableObject>();

	public List<ScenarioEvents> _scenariosEvents = new List<ScenarioEvents>();

	[Header("Technologies")]
	public TechnologyScriptableObject[] technologies;

	[Header("DateDisplayMonths")]
	public string[] monthsDisplay;
	public string[] monthsDisplayEN;

	[Header("Multiplayer")]
	[SerializeField]
	private BoolValue isOnlineGame;

	public BoolValue playTestingMod;
    public BoolValue playMod;
    public BoolValue loadGame;
    public BoolValue marchEvent;
    public BoolValue isTutorialMode;
    public StringValue _playerCountrySelected;

    public StringValue editingModString;
	public StringValue difficultyValue;

    [SerializeField]
	private string jsonURL;

	[HideInInspector]
	public Multiplayer multiplayer;

	[SerializeField] private BoolValue devMode;
	public BoolValue _regionsDebugMode;
	public BoolValue _gameConsole;

	[Header("Fight settings")]
	public float fortDebuff = 5;

	public SeaRegion[] _seaRegions;
	public Color _seaSelectedColor;
	public Color _seaDefaultColor;

	[Header("AI Settings:")]
	public List<Division> _divisionTemplates = new();

	[Header("Value Settings:")]
	public int dockyardProduction = 15;
	public int _bunkerCost = 400;
	public int _bunkersMaxLevel = 15;
	public int _marineBaseMaxLevel = 15;
	public int _marineBaseCost = 700;
	public int _airBaseMaxLevel = 4;
	public int _airBaseCost = 1000;

	[Header("Fleet:")]
	public FleetScriptableObject SubmarineLVL1;

	[Header("Aviation:")]
	public Aviation_ScriptableObj[] _planes;
	public GameObject _exposionPrefab;

    [Header("Translate")]
	public Translate translate;

	public GameObject consolePanel;


    private void Awake()
	{
        Application.targetFrameRate = 60;
		onlineGame = isOnlineGame.value;

        diplomatyUI = FindObjectOfType<DiplomatyUI>();
		multiplayer = FindObjectOfType<Multiplayer>();
		countryManager = FindObjectOfType<CountryManager>();

		_DEBUG_REGIONS_IDS = _regionsDebugMode.value;

        developerCheats = devMode.value;

		for (int i = 0; i < technologies.Length; i++)
		{
			if (technologies[i].startReasearched)
			{
				CountrySettings[] countries = countryManager.countries.ToArray();
				for (int j = 0; j < countries.Length; j++)
				{
					countries[j].countryTechnologies.Add(technologies[i]);
				}
			}
		}

		if (onlineGame)
		{
			playerListButton.SetActive(true);
			return;
		}

		consolePanel.SetActive(_gameConsole.value);

		playerListButton.SetActive(false);

		if (PlayerPrefs.HasKey("IS_PREMIUM"))
		{
			if (PlayerPrefs.GetString("IS_PREMIUM") == "TRUE")
			{
				_isPremium.value = true;
			}
			else
			{
                _isPremium.value = false;
            }
        }

        _regionOpacity = PlayerPrefs.GetFloat("REGION_OPACITY", 0.5f);

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
			_language = Language.EN;
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            _language = Language.RU;
        }

		Guild._guilds.Clear();
    }

	public void SetGameEvents()
	{

    }

    public void TogglePlayerList()
	{
		if (!playersListsPanel.activeSelf)
		{
			OpenPlayerList();
			return;
		}
		ClosePlayerList();
	}

	private void OpenPlayerList()
	{
		BackgroundUI_Overlay.Instance.OpenOverlay(playersListsPanel);
		Multiplayer.Instance.UpdatePlayerListUI();
		playersListsPanel.SetActive(true);
	}

	private void ClosePlayerList()
	{
		BackgroundUI_Overlay.Instance.CloseOverlay();
		playersListsPanel.SetActive(false);
	}

    private void OnApplicationQuit()
    {
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.loadGame.value = false;
        ReferencesManager.Instance.gameSettings.difficultyValue.value = "EASY";

        PlayerPrefs.DeleteKey("currentCountryIndex");
    }

	[System.Serializable]
	public class ScenarioEvents
	{
		public int _id;
        public List<EventScriptableObject> _events = new List<EventScriptableObject>();
    }

    [System.Serializable]
    public class Division
    {
		public List<UnitScriptableObject> batalions = new List<UnitScriptableObject>();
    }
}