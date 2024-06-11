using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    private Player player;
    private Launcher launcher;

    public string _nickname;

    public Image selectedCountryFlag;
    public TMP_Text nicknameText;

    public int countriesIndex;

    Hashtable playerProperties = new Hashtable();

    public void SelectCountry(int index)
    {
        playerProperties["playerCountryIndex"] = index;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void SetUp(Player _player)
    {
        launcher = FindObjectOfType<Launcher>();

        player = _player;
        _nickname = player.NickName;
        nicknameText.text = _nickname;

        playerProperties["playerNickname"] = _nickname;

        SelectCountry(0);
        UpdatePlayerCountry(_player);
    }

    public void ChangeCountrySelection()
    {
        if (PlayerPrefs.GetString("nickname") == this._nickname)
        {
            SelectCountry(countriesIndex);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            countriesIndex++;
            int k = countriesIndex + 1;
            if (k > launcher.countriesList.Length)
            {
                countriesIndex = 0;
            }

            selectedCountryFlag.sprite = launcher.countriesList[countriesIndex].countryFlag;

            UpdatePlayerCountry(targetPlayer);
        }
    }

    private void UpdatePlayerCountry(Player _player)
    {
        if (_player.CustomProperties.ContainsKey("playerCountryIndex"))
        {
            selectedCountryFlag.sprite = launcher.countriesList[(int)_player.CustomProperties["playerCountryIndex"]].countryFlag;
            playerProperties["playerCountryIndex"] = _player.CustomProperties["playerCountryIndex"];
        }
        else
        {
            int randomCountry = Random.Range(0, launcher.countriesList.Length);

            foreach (Player playerItem in PhotonNetwork.PlayerList)
            {
                if (playerItem.CustomProperties.ContainsKey("playerCountryIndex") && (int)playerItem.CustomProperties["playerCountryIndex"] != randomCountry)
                {
                    playerProperties["playerCountryIndex"] = randomCountry;
                }
            }
        }
    }
}
