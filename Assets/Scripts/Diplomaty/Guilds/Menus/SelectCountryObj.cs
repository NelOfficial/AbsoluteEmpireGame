using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectCountryObj : MonoBehaviour
{
	public static SelectCountryObj selected;

	public Image flag;
	public TMP_Text text;
	public TMP_Text role;
	public CountrySettings country;

	public void SetUp(CountrySettings country)
	{
		flag.sprite = country.country.countryFlag;
		text.text = country.country.name;
		Guild.Country cou = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName).GetCountry(country);
		if (cou != null)
		{
			role.text = cou.role.ToString();
		}
		this.country = country;
	}

	public void OnClick()
	{
		if (selected != null)
		{
			selected.GetComponent<Image>().color = Color.white;
		}
		selected = this;
		GetComponent<Image>().color = Color.green; // цвет поменяешь)
    }
}
