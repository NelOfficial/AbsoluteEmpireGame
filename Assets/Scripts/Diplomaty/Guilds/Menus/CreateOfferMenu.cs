using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CreateOfferMenu : MonoBehaviour
{
    public static CreateOfferMenu Instance;

    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Transform view;
    [SerializeField] private GameObject cell;

    private static CountryManager countryManager;
    private static DiplomatyUI diplomatyUI;


    private void Awake()
    {
        Instance = this;

        countryManager = ReferencesManager.Instance.countryManager;
        diplomatyUI = ReferencesManager.Instance.diplomatyUI;
    }

    public void DropDownUpd()
    {
        dropdown.value = 0;
    }

    public void Enable()
    {
        foreach(Transform child in view)
        {
            Destroy(child.gameObject);
        }

        List<CountrySettings> countries = new();

        if (dropdown.value == 0) // ������
        {
            countries.Clear();
        }

        if (dropdown.value == 1) // �������
        {
            Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

            countries = guild.GetCountries();
        }

        if (dropdown.value == 2) // ����������
        {
            Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);
            countries = new(countryManager.countries);

            for (int i = 0; i < countries.Count; i++)
            {
                CountrySettings country = countries[i];

                if (guild.Contains(country))
                {
                    countries.Remove(country);
                }
            }
        }

        if (dropdown.value == 3) // �������
        {
            Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);
            countries = new(countryManager.countries);

            for (int i = 0; i < countries.Count; i++)
            {
                CountrySettings country = countries[i];

                if (guild.Contains(country))
                {
                    countries.Remove(country);
                }
            }
        }

        if (dropdown.value == 4) // ����������
        {
            Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);
            countries = new(countryManager.countries);

            for (int i = 0; i < countries.Count; i++)
            {
                CountrySettings country = countries[i];
                if (guild.Contains(country))
                {
                    countries.Remove(country);
                }

                if (!diplomatyUI.FindCountriesRelation(country, countryManager.currentCountry).war)
                {
                    countries.Remove(country);
                }
            }
        }

        if (dropdown.value == 5) // ��������
        {
            countries = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).GetCountries();
            for (int i = 0; i < countries.Count; i++)
            {
                CountrySettings country = countries[i];
                if (Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).GetCountry(country).role == Guild.Role.Owner)
                {
                    countries.Remove(country);
                }
            }
        }

        if (dropdown.value == 6) // ��������
        {
            countries = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).GetCountries();

            for (int i = 0; i < countries.Count; i++)
            {
                CountrySettings country = countries[i];
                if (Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).GetCountry(country).role == Guild.Role.Default ||
                    Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).GetCountry(country).role == Guild.Role.Puppet)
                {
                    countries.Remove(country);
                }
            }
        }

        foreach (CountrySettings country in countries)
        {
            SelectCountryObj obj = Instantiate(cell, view).GetComponent<SelectCountryObj>();

            string ideology = ReferencesManager.Instance.languageManager.GetTranslation(country.ideology);

            obj.transform.Find("Text (TMP) (1)").GetComponent<TMP_Text>().text = ideology;
            obj.SetUp(country);
        }

        gameObject.SetActive(true);
    }

    public void DropDownChanged()
    {
        SelectCountryObj.selected = null;

        Enable();
    }

    public void Execute()
    {
        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

        if (dropdown.value == 0) // ������
        {
            WarningManager.Instance.Warn("����������� ������� ������!");
        }

        if (dropdown.value == 1) // �������
        {
            guild._offers.Add(new Guild.Offer(guild, countryManager.currentCountry, guild.GetCountry(SelectCountryObj.selected.country), Guild.Action.Kick));
        }

        if (dropdown.value == 2) // ����������
        {
            guild._offers.Add(new Guild.Offer(guild, countryManager.currentCountry, SelectCountryObj.selected.country, Guild.Action.Invite));
        }

        if (dropdown.value == 3) // �������
        {
            guild._offers.Add(new Guild.Offer(guild, countryManager.currentCountry, SelectCountryObj.selected.country, Guild.Action.Attack));
        }

        if (dropdown.value == 4) // ����������
        {
            guild._offers.Add(new Guild.Offer(guild, countryManager.currentCountry, SelectCountryObj.selected.country, Guild.Action.Peace));
        }

        if (dropdown.value == 5) // ��������
        {
            guild._offers.Add(new Guild.Offer(guild, countryManager.currentCountry, guild.GetCountry(SelectCountryObj.selected.country), Guild.Action.Promote));
        }

        if (dropdown.value == 6) // ��������
        {
            guild._offers.Add(new Guild.Offer(guild, countryManager.currentCountry, guild.GetCountry(SelectCountryObj.selected.country), Guild.Action.Demote));
        }

        gameObject.SetActive(false);
        menu3.Instance.EnableOffers();
    }
}
