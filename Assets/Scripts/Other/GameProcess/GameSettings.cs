using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public class GameSettings : MonoBehaviour
{
	public bool developerMode;
	public bool spectatorMode;
	public bool developerCheats;
	public bool diplomatyCheats;
	public bool onlineGame;
	public bool jsonTest;

	[Header("DEBUG")]
	public bool _DEBUG_REGIONS_IDS;
	public GameObject debugText;

	public Color greenColor;
	public Color blueColor;
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

	public UnitScriptableObject motoLVL1;
	public UnitScriptableObject motoLVL2;

	public BuildingScriptableObject fabric;
	public BuildingScriptableObject farm;
	public BuildingScriptableObject chefarm;

	public List<UnitScriptableObject> currentDefenseUnits_FirstLevel = new List<UnitScriptableObject>();
	public List<UnitScriptableObject> currentDefenseUnits_SecondLevel = new List<UnitScriptableObject>();
	public List<UnitScriptableObject> currentDefenseUnits_ThirdLevel = new List<UnitScriptableObject>();

	private string buttonDesingType;

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

	[Header("SFX Sounds")]
	public AudioClip[] m_heavy_move;
	public AudioClip[] m_infantry_move;
	public AudioClip[] m_motorized_infantry_move;
	public AudioClip m_new_event_01;
	public AudioClip m_paper_01;

	[Header("Game Events")]
	public bool allowGameEvents;
	public List<EventScriptableObject> gameEvents = new List<EventScriptableObject>();

	[Header("Technologies")]
	public TechnologyScriptableObject[] technologies;

	[Header("DateDisplayMonths")]
	public string[] monthsDisplay;

	[Header("Multiplayer")]
	[SerializeField]
	private BoolValue isOnlineGame;
	[SerializeField]
	private BoolValue jsonTestFile;
    public BoolValue playTestingMod;
    [SerializeField]
	private string jsonURL;

	[HideInInspector]
	public Multiplayer multiplayer;

	[SerializeField] BoolValue devMode;
	//[SerializeField] RewardedAds rewardedAds;


	private void Awake()
	{
		Application.targetFrameRate = 120;
		onlineGame = isOnlineGame.value;
		jsonTest = jsonTestFile.value;

		diplomatyUI = FindObjectOfType<DiplomatyUI>();
		multiplayer = FindObjectOfType<Multiplayer>();
		countryManager = FindObjectOfType<CountryManager>();

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
		playerListButton.SetActive(false);

		//GetJsonFromURL();
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
}