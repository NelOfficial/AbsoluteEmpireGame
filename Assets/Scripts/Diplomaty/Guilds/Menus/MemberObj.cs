using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MemberObj : MonoBehaviour
{
    [SerializeField] private FillCountryFlag _countryFlag;

    [SerializeField] private TMP_Text _countryName;
    [SerializeField] private TMP_Text _countryRole;
    [SerializeField] private TMP_Text _countryRegionsCount;
    [SerializeField] private TMP_Text _countryIdeology;
    [SerializeField] private Button _kickButton;

    [HideInInspector] public Guild.Country _currentCountry;
    [HideInInspector] public Guild _currentGuild;

    public void SetUp(Guild.Country country, Guild guild)
    {
        _countryFlag.country = country.country.country;
        _countryFlag.FillInfo();

        _countryName.text = ReferencesManager.Instance.languageManager.GetTranslation(country.country.country._nameEN);
        _countryRole.text = country.role.ToString();
        _countryRegionsCount.text = country.country.myRegions.Count.ToString();
        _countryIdeology.text = ReferencesManager.Instance.languageManager.GetTranslation(country.country.ideology);

        _currentCountry = country;
        _currentGuild = guild;
    }

    public void Kick()
    {
        if (_currentCountry.country == ReferencesManager.Instance.countryManager.currentCountry)
        {
            WarningManager.Instance.Warn("Вы не можете выгнать себя...\nЭто не имеет смысла :/");
            return;
        }

        if ((int)_currentGuild.GetCountry(ReferencesManager.Instance.countryManager.currentCountry).role > 2)
        {
            WarningManager.Instance.Warn("Недостаточно прав");
            return;
        }

        if ((int)_currentCountry.role <= (int)_currentGuild.GetCountry(ReferencesManager.Instance.countryManager.currentCountry).role)
        {
            WarningManager.Instance.Warn("Страна имеет роль выше или равную вашей\nвы можете выгнать эту страну только голосованием");
            return;
        }

        _currentGuild.Kick(_currentCountry.country);

        menu3.Instance.EnableMembers();
    }
}
