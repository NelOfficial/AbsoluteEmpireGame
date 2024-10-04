using TMPro;
using UnityEngine;

public class PlayerListItem_InGame : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNicknameText;
    [SerializeField] private FillCountryFlag _countryFlag;

    [HideInInspector] public CountrySettings _country;

    [HideInInspector] public string _nickname;

    public GameObject _readyToMove;

    [HideInInspector] public bool isReady;

    private void Awake()
    {
        _countryFlag.InDiplomatyUI = true;
    }

    public void UpdateUI()
    {
        UpdateFlag();
        UpdateNicknameUI();
        ReadyUpdateUI();
    }

    public void UpdateFlag()
    {
        _countryFlag.country = _country.country;
        _countryFlag.FillInfo();
    }

    public void UpdateNicknameUI()
    {
        _playerNicknameText.text = _nickname;
    }

    public void ReadyUpdateUI()
    {
        _readyToMove.SetActive(isReady);
    }
}
