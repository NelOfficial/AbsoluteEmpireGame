using UnityEngine;

public class FlagSelectionMenu : MonoBehaviour
{
	public static FlagSelectionMenu Instance;

	public Transform view;
	public GameObject cell;

	private void Awake()
	{
		Instance = this;
		gameObject.SetActive(false);
	}

	public void Enable()
	{		
		if (Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).
			GetCountry(ReferencesManager.Instance.countryManager.currentCountry).role !=
			Guild.Role.Owner)
		{
			WarningManager.Instance.Warn("Вы должны быть владельцем альянса, чтобы изменить его флаг!");

			gameObject.SetActive(false);

			return;
		}

		foreach (Transform child in view)
		{
			Destroy(child.gameObject);
		}

        foreach (Guild.FlagSprite flag in ReferencesManager.Instance.guildImages)
        {
            FlagItem obj = Instantiate(cell, view).GetComponent<FlagItem>();

            obj.SetUp(flag);
        }

        foreach (CountryScriptableObject cou in ReferencesManager.Instance.globalCountries)
		{
            FlagItem obj = Instantiate(cell, view).GetComponent<FlagItem>();

			obj.SetUp(new Guild.FlagSprite() {
				guild_name = cou.name,
				sprite = cou.countryFlag
			});
		}

		gameObject.SetActive(true);
	}
}
