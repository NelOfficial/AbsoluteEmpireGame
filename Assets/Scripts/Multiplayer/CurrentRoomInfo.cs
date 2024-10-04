using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentRoomInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _roomScenarioText;
    [SerializeField] private Transform _playerListTransform;

    [SerializeField] private GameObject _playerListPrefab;

    public GameObject _startGameButton;

    [SerializeField] private Transform _countriesListTransform;

    private Launcher _launcher;

    private void Awake()
    {
        _launcher = ReferencesManager.Instance.launcher;
    }

    public void UpdateRoomUI()
    {
        _roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        _roomScenarioText.text = PhotonNetwork.CurrentRoom.CustomProperties["scenario_display"].ToString();

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in _playerListTransform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(_playerListPrefab, _playerListTransform).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        _startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void UpdateCountriesList_UI()
    {
        foreach (Transform item in _countriesListTransform)
        {
            if (item.GetComponent<SelectCountryButton>())
            {
                Destroy(item.gameObject);
            }
        }

        for (int i = 0; i < _launcher.countriesList.Length; i++)
        {
            GameObject spawnedCountryItem = Instantiate(ReferencesManager.Instance.offlineGameSettings.selectCountryButtonPrefab, _countriesListTransform);

            SelectCountryButton selectCountryButton = spawnedCountryItem.GetComponent<SelectCountryButton>();

            selectCountryButton.country_ScriptableObject = _launcher.countriesList[i];
            selectCountryButton.UpdateUI();

            spawnedCountryItem.GetComponent<Button>().onClick.AddListener(delegate
            {
                ChangeCountry(selectCountryButton.country_ScriptableObject._id);
            });
        }
    }

    private void ChangeCountry(int countryId)
    {
        Hashtable playerProperties = new()
        {
            ["playerCountryId"] = countryId
        };

        PhotonNetwork.SetPlayerCustomProperties(playerProperties);

        Debug.Log($"changed country to {countryId}");
    }
}
