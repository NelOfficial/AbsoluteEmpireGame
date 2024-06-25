using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : NetworkBehaviour
{
    private Launcher launcher;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string _nickname;

    public Image selectedCountryFlag;
    public TMP_Text nicknameText;

    public int countryIndex;

    [SyncVar(hook = nameof(OnCountryChanged))]
    public int countryId;

    private void OnNameChanged(string oldName, string newName)
    {
        nicknameText.text = newName;
    }

    private void OnCountryChanged(int oldCountryIndex, int newCountryIndex)
    {
        countryId = newCountryIndex;
        UpdateCountryFlag();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer)
        {
            CmdSetPlayerName(PlayerPrefs.GetString("nickname"));
        }
    }

    public void ChangeCountry()
    {
        if (isLocalPlayer)
        {
            CmdSelectCountry();
        }
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        _nickname = name;
    }

    [Command]
    public void CmdSelectCountry()
    {
        OfflineGameSettings.Scenario scenario =
            ReferencesManager.Instance.offlineGameSettings.GetScenario(
                ReferencesManager.Instance.offlineGameSettings.currentScenarioId);

        if (countryIndex + 1 < scenario.countries.Length)
        {
            countryIndex++;
        }
        else
        {
            countryIndex = 0;
        }

        countryId = scenario.countries[countryIndex]._id;
    }

    public void SetUp()
    {
        launcher = FindObjectOfType<Launcher>();

        nicknameText.text = _nickname;

        UpdateCountryFlag();
    }

    private void UpdateCountryFlag()
    {
        foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
        {
            if (country._id == countryId)
            {
                selectedCountryFlag.sprite = country.countryFlag;
            }
        }
    }
}
