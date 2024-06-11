using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MultiplayerTest : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    private CountryManager countryManager;

    public RegionManager regionToSelect;

    public Transform playersTab;
    public GameObject playerItem;

    public int regionIndex;

    public List<CountrySettings> playerCountries = new List<CountrySettings>();

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        countryManager = FindObjectOfType<CountryManager>();

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            playerCountries.Add(countryManager.countries[(int)players[i].CustomProperties["playerCountryIndex"]]);

            CountrySettings addedCountry = countryManager.countries[(int)players[i].CustomProperties["playerCountryIndex"]];
            addedCountry.country._name = addedCountry.country._name + $" ({players[i].NickName})";

            addedCountry.onlinePlayer = true;
            addedCountry._countryPlayer = players[i];
        }

        UpdatePlayersTab();
    }

    public void SetRegion()
    {
        _photonView.RPC("Select_Region", RpcTarget.All, regionIndex);
    }

    [PunRPC]
    public void Select_Region(int _regionIndex)
    {
        regionToSelect.SelectRegionNoHit(countryManager.regions[_regionIndex]);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayersTab();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersTab();
    }

    public void UpdatePlayersTab()
    {
        foreach (Transform child in playersTab)
        {
            Destroy(child.gameObject);
        }

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            GameObject playerItemSpawned = Instantiate(playerItem);
            playerItemSpawned.transform.SetParent(playersTab);
            playerItemSpawned.transform.localScale = new Vector3(1, 1, 1);

            string countryName = countryManager.countries[(int)players[i].CustomProperties["playerCountryIndex"]].country._uiName;
            Sprite countryFlag = countryManager.countries[(int)players[i].CustomProperties["playerCountryIndex"]].country.countryFlag;

            playerItemSpawned.transform.GetChild(0).GetComponent<TMP_Text>().text = $"{players[i].NickName}";
            playerItemSpawned.transform.GetChild(1).GetComponent<Image>().sprite = countryFlag;
        }
    }
}
