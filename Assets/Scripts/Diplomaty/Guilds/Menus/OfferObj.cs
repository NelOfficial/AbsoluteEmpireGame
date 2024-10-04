using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferObj : MonoBehaviour
{
    public FillCountryFlag flag;
    public TMP_Text header;
    public TMP_Text subtext;
    Guild.Offer offer = null;
    public bool executed = false;

    [SerializeField] private GameObject[] _buttons;
    [SerializeField] private GameObject _backgroundHolder;

    public void SetUp(Guild.Offer offer)
    {
        this.offer = offer;

        flag.country = offer.starter.country;
        flag.FillInfo();

        string arg1 = "";

        // Используем проверку типов через is для большей безопасности
        if (offer.arg is CountrySettings countrySettings)
        {
            arg1 = countrySettings.country._nameEN;
        }
        else if (offer.arg is Guild.Country guildCountry)
        {
            arg1 = guildCountry.country.country._nameEN;
        }
        else
        {
            arg1 = offer.arg?.ToString() ?? string.Empty; // Проверка на null и приведение к строке
        }

        header.text = Guild.GetText(offer.action, offer.starter.country._nameEN, arg1);
        subtext.text = $"Согласны: {offer.agree.Count}, Против: {offer.disagree.Count}";

        // Выбор цвета на основе количества голосов
        subtext.color = offer.agree.Count > offer.disagree.Count
                        ? Color.green
                        : offer.agree.Count == offer.disagree.Count
                          ? Color.yellow
                          : Color.red;

        if (offer.completed)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].SetActive(false);
            }

            _backgroundHolder.SetActive(true);
        }
        else
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].SetActive(true);
            }

            _backgroundHolder.SetActive(false);
        }
    }

    public void Agree()
    {
        if (executed) return;

        if (offer.Voted(ReferencesManager.Instance.countryManager.currentCountry))
        {
            WarningManager.Instance.Warn("Вы уже проголосовали");
            return;
        }

        for (int i = 0; i < offer.guild._countries.Count; i++)
        {
            if (offer.guild._countries[i].country == ReferencesManager.Instance.countryManager.currentCountry)
            {
                offer.agree.Add(offer.guild._countries[i]);
                if (offer.Execute())
                {
                    GetComponent<Image>().color = Color.green;
                    executed = true;
                }
            }
        }

        SetUp(offer);
    }

    public void DisAgree()
    {
        if (executed) return;

        if (offer.Voted(ReferencesManager.Instance.countryManager.currentCountry))
        {
            WarningManager.Instance.Warn("Вы уже проголосовали");
            return;
        }

        for (int i = 0; i < offer.guild._countries.Count; i++)
        {
            if (offer.guild._countries[i].country == ReferencesManager.Instance.countryManager.currentCountry)
            {
                offer.disagree.Add(offer.guild._countries[i]);
                offer.Execute();
                SetUp(offer);
            }
        }
    }

    public void Decline()
    {
        Guild.Country country = offer.guild.GetCountry(ReferencesManager.Instance.countryManager.currentCountry);

        if (country == null)
        {
            return;
        }

        if ((int)country.role <= 1)
        {
            offer.guild._offers.Remove(offer);
        }

        GetComponent<Image>().color = Color.red;
    }

    public void Accept()
    {
        Guild.Country country = offer.guild.GetCountry(ReferencesManager.Instance.countryManager.currentCountry);
        if (executed)
        {
            WarningManager.Instance.Warn("Предложение уже принято");
            return;
        }

        if (country == null)
        {
            return;
        }

        if ((int)country.role <= 1)
        {
            for (int i = 0; i < offer.guild._countries.Count; i++)
            {
                offer.agree.Add(new Guild.Country());
            }

            offer.Execute();
            GetComponent<Image>().color = Color.green;
        }
    }
}
