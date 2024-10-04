using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GuildMenu : MonoBehaviour
{
    public static GuildMenu Instance;

    [SerializeField] private Transform _view;
    [SerializeField] private GameObject _cell;

    [SerializeField] private TMP_Text guild_name;
    [SerializeField] private TMP_Text guild_type;
    [SerializeField] private TMP_Text guild_ideology;
    [SerializeField] private TMP_Text guild_union;
    [SerializeField] private TMP_Text guild_pact;
    [SerializeField] private TMP_Text guild_trade;
    [SerializeField] private TMP_Text guild_right;
    [SerializeField] private Image guild_flag;

    [SerializeField] private TMP_Text _offersButton;

    private static GameSettings gameSettings;
    private static CountryManager countryManager;


    private void Awake()
    {
        Instance = this;

        gameSettings = ReferencesManager.Instance.gameSettings;
        countryManager = ReferencesManager.Instance.countryManager;

        gameObject.SetActive(false);
    }

    public void Enable()
    {
        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

        if (guild == null)
        {
            Disable();
            GuildManageMenu.Instance.Enable();

            return;
        }

        guild_name.text = guild._name;
        guild_ideology.text = $"{guild._ideology}";

        _offersButton.text = $"Голосования ({guild._offers.Count})";

        if (guild._ideology == "Коммунизм")
        {
            guild_ideology.color = new Color(0.81f, 0.26f, 0.125f, 1f);
        }
        else if (guild._ideology == "Демократия")
        {
            guild_ideology.color = new Color(0.33f, 0.60f, 0.83f, 1f);
        }
        else if (guild._ideology == "Фашизм")
        {
            guild_ideology.color = new Color(0.78f, 0.69f, 0.53f, 1f);
        }
        else if (guild._ideology == "Монархия")
        {
            guild_ideology.color = new Color(0.21f, 0.78f, 0.1f, 1f);
        }
        else
        {
            guild_ideology.color = new Color(0.85f, 0.71f, 0.175f, 1f);
        }

        guild_type.text = $"{guild._type}";

        if (guild._type == Guild.GuildType.Alliance)
        {
            guild_type.color = gameSettings._yellowColor;
        }
        else
        {
            guild_type.color = gameSettings.blueColor;
        }

        if (guild._relations.union)
        {
            guild_union.color = gameSettings.greenColor;
        }
        else
        {
            guild_union.color = gameSettings.redColor;
        }

        if (guild._relations.pact)
        {
            guild_pact.color = gameSettings.greenColor;
        }
        else
        {
            guild_pact.color = gameSettings.redColor;
        }

        if (guild._relations.trade)
        {
            guild_trade.color = gameSettings.greenColor;
        }
        else
        {
            guild_trade.color = gameSettings.redColor;
        }

        if (guild._relations.right)
        {
            guild_right.color = gameSettings.greenColor;
        }
        else
        {
            guild_right.color = gameSettings.redColor;
        }

        guild_flag.sprite = guild._icon;

        foreach (Transform item in _view)
        {
            Destroy(item.gameObject);
        }
        
        foreach (var country in guild._countries)
        {
            FillCountryFlag spawnedItem = Instantiate(_cell, _view).GetComponent<FillCountryFlag>();
            spawnedItem.country = country.country.country;
            spawnedItem.InDiplomatyUI = true;
            spawnedItem.FillInfo();

            spawnedItem.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
        }

        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Join()
    {
        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

        foreach (Guild guild1 in countryManager.currentCountry.guilds)
        {
            if (guild1._type == Guild.GuildType.Alliance)
            {
                WarningManager.Instance.Warn("Вы не можете состоять в двух военных альянсах");

                return;
            }
        }

        if (guild.GetCountry(countryManager.currentCountry) != null)
        {
            WarningManager.Instance.Warn("Вы уже в организации!");

            return;
        }

        foreach (Guild.Offer offer in guild._offers)
        {
            if (offer.action == Guild.Action.Join && offer.starter == countryManager.currentCountry)
            {
                WarningManager.Instance.Warn("Вы уже подали заявку на вступление");

                return;
            }
        }

        if (guild._ideology != "Любая")
        {
            if (guild._ideology != countryManager.currentCountry.ideology)
            {
                WarningManager.Instance.Warn("Ваша идеология не соответствует идеологии организации");
                return;
            }
        }

        Guild.Offer _offer = new Guild.Offer(guild, countryManager.currentCountry, countryManager.currentCountry, Guild.Action.Join);

        guild._offers.Add(_offer);
    }
    public void Leave()
    {
        Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).Kick(countryManager.currentCountry);

        Enable();
    }

    public void Delete()
    {
        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);
        Guild.Country country = guild.GetCountry(countryManager.currentCountry);

        if (country == null)
        {
            WarningManager.Instance.Warn("Вы не состоите в организации");

            return;
        }

        if (country.role != Guild.Role.Owner)
        {
            WarningManager.Instance.Warn("Вы должны быть владельцем для удаления организации");

            return;
        }

        guild.Delete();
        Enable();
    }
}