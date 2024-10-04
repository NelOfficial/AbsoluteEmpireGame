using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class menu3: MonoBehaviour
{
    public static menu3 Instance;

    [SerializeField] private Transform view;
    [SerializeField] private GameObject cell;
    [SerializeField] private GameObject cell2;
    [SerializeField] private GameObject create_button;
    [SerializeField] private GameObject sort_button;
    [SerializeField] private TMP_Dropdown sort_method;
    [SerializeField] private TMP_Dropdown sort_type;

    private static CountryManager countryManager;

    [SerializeField] private Transform _completedOffersHeader;
    [SerializeField] private GameObject _noOffersText;


    private void Awake()
    {
        Instance = this;

        sort_method.value = 0;
        sort_type.value = 0;

        countryManager = ReferencesManager.Instance.countryManager;
    }

    public void EnableOffers()
    {
        sort_button.SetActive(false);
        create_button.SetActive(true);

        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

        if (guild.GetCountry(countryManager.currentCountry) == null)
        {
            WarningManager.Instance.Warn("Вы не состоите в организации");

            return;
        }

        foreach (Transform item in view)
        {
            if (item.gameObject.GetComponent<OfferObj>())
            {
                Destroy(item.gameObject);
            }
        }

        if (guild._offers.Count <= 0 && guild._completedOffers.Count <= 0)
        {
            _noOffersText.gameObject.SetActive(true);
        }
        else
        {
            _noOffersText.gameObject.SetActive(false);
        }

        if (guild._completedOffers.Count <= 0)
        {
            _completedOffersHeader.gameObject.SetActive(false);
        }
        else
        {
            _completedOffersHeader.gameObject.SetActive(true);
        }

        foreach (Guild.Offer offer in guild._offers) 
        {
            OfferObj obj = Instantiate(cell, view).GetComponent<OfferObj>();
            obj.SetUp(offer);
        }

        _completedOffersHeader.SetAsLastSibling();

        foreach (Guild.Offer offer in guild._completedOffers)
        {
            OfferObj obj = Instantiate(cell, view).GetComponent<OfferObj>();
            obj.SetUp(offer);
        }

        gameObject.SetActive(true);
    }

    public void EnableMembers()
    {
        sort_button.SetActive(true);
        create_button.SetActive(false);

        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

        if (guild.GetCountry(countryManager.currentCountry) == null)
        {
            WarningManager.Instance.Warn("Вы не состоите в организации");
            return;
        }

        foreach (Transform item in view)
        {
            Destroy(item.gameObject);
        }

        foreach (Guild.Country country in Sort(guild)) 
        {
            MemberObj obj = Instantiate(cell2, view).GetComponent<MemberObj>();
            obj.SetUp(country, guild);
        }

        gameObject.SetActive(true);
    }

    public List<Guild.Country> Sort(Guild guild)
    {
        IOrderedEnumerable<Guild.Country> result = null;

        switch (sort_method.value)
        {
            case 0:
                result = from country in guild._countries orderby country.country.myRegions.Count select country;
                break;
            case 1:
                result = from country in guild._countries orderby country.country.country._name, country.country.myRegions.Count select country;
                break;
            case 2:
                result = from country in guild._countries orderby country.country.ideology, country.country.myRegions.Count select country;
                break;
            default:
                break;
        }

        List<Guild.Country> result2 = new List<Guild.Country>(10);

        foreach (Guild.Country country in result)
        {
            result2.Add(country);
        }

        if (sort_method.value == 0)
        {
            if (sort_type.value == 0)
            {
                result2.Reverse();
            }
        }

        else
        {
            if (sort_type.value == 1)
            {
                result2.Reverse();
            }
        }

        return result2;
    }
}