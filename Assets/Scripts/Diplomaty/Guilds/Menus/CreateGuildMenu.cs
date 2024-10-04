using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateGuildMenu : MonoBehaviour
{
    public static CreateGuildMenu Instance;

    [SerializeField] private TMP_InputField _guildNameInputfield;
    [SerializeField] private TMP_Dropdown _guildIdeologyDropdown;
    [SerializeField] private TMP_Dropdown _guildTypeDropdown;

    [SerializeField] private Toggle _unionRelationToggle;
    [SerializeField] private Toggle _pactRelationToggle;
    [SerializeField] private Toggle _tradeRelationToggle;
    [SerializeField] private Toggle _rightRelationToggle;

    private static CountryManager countryManager;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        countryManager = ReferencesManager.Instance.countryManager;
    }

    public void Create()
    {        
        string name = _guildNameInputfield.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        string ideology = _guildIdeologyDropdown.value switch
        {
            0 => "�����",
            1 => "���������",
            2 => "������",
            3 => "����������",
            4 => "��������",
            _ => "�����",
        };

        Guild.GuildType type = _guildTypeDropdown.value switch
        {
            0 => Guild.GuildType.Organization,
            1 => Guild.GuildType.Alliance,
            _ => Guild.GuildType.Organization,
        };

        if (ideology != "�����")
        {
            if (ideology != countryManager.currentCountry.ideology)
            {
                WarningManager.Instance.Warn("��������� ����������� ������ ��������������� ����� ��� �������� \"�����\"");

                return;
            }
        }

        if (type == Guild.GuildType.Alliance)
        {
            Guild _checkAlliance = Guild._guilds.Find(item => item._type == Guild.GuildType.Alliance);

            if (_checkAlliance != null)
            {
                WarningManager.Instance.Warn("�� �� ������ ������� ������� ������, ���� �� ��� �������� � ������� �������\n������ ����� �� ������ ������� �����������");

                return;
            }
        }

        Guild _nameIsReserved = Guild._guilds.Find(item => item._name == name);

        if (_nameIsReserved != null)
        {
            WarningManager.Instance.Warn("����������� � ����� ��������� ��� ����������!");

            return;
        }

        if (type == Guild.GuildType.Organization)
        {
            if (_unionRelationToggle.isOn)
            {
                WarningManager.Instance.Warn("��� ����������� �� ������������ ���������� \"����\"");
                return;
            }

            if (_rightRelationToggle.isOn)
            {
                WarningManager.Instance.Warn("��� ����������� �� ������������ ���������� \"������ �����\"");
                return;
            }

            if (_pactRelationToggle.isOn)
            {
                WarningManager.Instance.Warn("��� ����������� �� ������������ ���������� \"����\"");
                return;
            }
        }

        Guild.Relations relations = new()
        {
            union = _unionRelationToggle.isOn,
            pact = _pactRelationToggle.isOn,
            trade = _tradeRelationToggle.isOn,
            right = _rightRelationToggle.isOn
        };

        Guild.Create(name, countryManager.currentCountry.country.countryFlag, countryManager.currentCountry, type, relations, ideology);

        ResetUI();

        gameObject.SetActive(false);
        GuildManageMenu.Instance.Enable();
    }

    private void ResetUI()
    {
        _guildNameInputfield.text = string.Empty;
        _guildIdeologyDropdown.value = 0;
        _guildTypeDropdown.value = 0;

        _unionRelationToggle.isOn = false;
        _rightRelationToggle.isOn = false;
        _pactRelationToggle.isOn = false;
        _tradeRelationToggle.isOn = false;
    }
}
