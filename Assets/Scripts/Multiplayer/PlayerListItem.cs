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

    public int countriesIndex;

    private void OnNameChanged(string oldName, string newName)
    {
        nicknameText.text = newName;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer)
        {
            CmdSetPlayerName(PlayerPrefs.GetString("nickname"));
        }
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        _nickname = name;
    }

    public void SelectCountry(int index)
    {
    }

    public void SetUp()
    {
        launcher = FindObjectOfType<Launcher>();

        nicknameText.text = _nickname;

        SelectCountry(0);
    }

    public void ChangeCountrySelection()
    {
        if (PlayerPrefs.GetString("nickname") == this._nickname)
        {
            SelectCountry(countriesIndex);
        }
    }
}
