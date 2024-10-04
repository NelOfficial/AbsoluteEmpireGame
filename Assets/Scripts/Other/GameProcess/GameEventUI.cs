using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using R = ReferencesManager;
public class GameEventUI : MonoBehaviour
{
	[SerializeField] Animator imageViewAnimator;

	[SerializeField] GameObject imageViewPanel;
	[SerializeField] Image imageView;
	[SerializeField] Image imageMin;
	[SerializeField] TMP_Text descriptionText;
	[SerializeField] TMP_Text titleText;

	public EventScriptableObject currentGameEvent;

	private bool isEnabled = false;

	private GameSettings gameSettings;
	private CountryManager countryManager;
	private DiplomatyUI diplomatyUI;

	private CountrySettings attacker;
	private CountrySettings defender;
	private RegionManager region;

	private UnitMovement division;
	private UnitScriptableObject unitToAdd;

	[SerializeField] GameObject eventButton;
	[SerializeField] Transform buttonsContainer;

	public List<custom_variable> vars = new List<custom_variable>(10);
	private void Awake()
	{
		gameSettings = FindObjectOfType<GameSettings>();
		countryManager = FindObjectOfType<CountryManager>();
		diplomatyUI = FindObjectOfType<DiplomatyUI>();
		if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "historic")
		{
			ReferencesManager.Instance.gameSettings.allowGameEvents = true;
		}
		else if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "nonhistoric")
		{
			ReferencesManager.Instance.gameSettings.allowGameEvents = false;
		}
		vars.Add(new custom_variable("null", -2147483648));
	}

	public void UpdateUI()
	{
		if (currentGameEvent != null)
		{
			if (PlayerPrefs.GetInt("languageId") == 1)
			{
				descriptionText.text = currentGameEvent.description;
				titleText.text = currentGameEvent._name;
			}
			else
			{
				descriptionText.text = currentGameEvent.descriptionEN;
				titleText.text = currentGameEvent._nameEN;
			}

			imageMin.sprite = currentGameEvent.image;

			foreach (Transform button in buttonsContainer)
			{
				Destroy(button.gameObject);
			}

			List<CountrySettings> countriesInActions = new List<CountrySettings>();
			bool ultimatum = false;

			for (int i = 0; i < currentGameEvent.buttons.Count; i++)
			{
				for (int a = 0; a < currentGameEvent.buttons[i].actions.Count; a++)
				{
					string[] act = currentGameEvent.buttons[i].actions[a].Split(';');
					if (act[0] == "annex")
					{
						ultimatum = true;

						for (int ai = 1; ai < act.Length; ai++)
						{
							foreach (RegionManager region in countryManager.regions)
							{
								if (region._id == int.Parse(act[ai]))
								{
									if (!countriesInActions.Contains(region.currentCountry))
									{
										countriesInActions.Add(region.currentCountry);
									}
								}
							}
						}
					}
				}

				if (ultimatum && currentGameEvent.buttons[i].rejectUltimatum)
				{
					if (!countriesInActions.Contains(countryManager.currentCountry))
					{
						return;
					}
				}

				GameObject spawnedButton = Instantiate(eventButton, buttonsContainer.transform);

				spawnedButton.GetComponent<EventButtonUI>().buttonIndex = i;


				if (PlayerPrefs.GetInt("languageId") == 1)
				{
					spawnedButton.GetComponent<EventButtonUI>()._buttonName = currentGameEvent.buttons[i].name;
				}
				else
				{
					spawnedButton.GetComponent<EventButtonUI>()._buttonName = currentGameEvent.buttons[i].nameEN;
				}

				spawnedButton.GetComponent<EventButtonUI>().SetUp();
			}
		}
	}

	public void ToggleView()
	{
		UISoundEffect.Instance.PlayAudio(gameSettings.m_paper_01);

		imageView.sprite = currentGameEvent.image;

		if (isEnabled) // ��������, ������ ���������
		{
			isEnabled = false;
			StartCoroutine(ClosePanel_Co());
		}
		else if (!isEnabled) // ���������, ������ ��������
		{
			isEnabled = true;
			imageViewPanel.SetActive(true);
			imageViewAnimator.Play("imageViewOpen");
		}
	}

	public bool CheckConditions(EventScriptableObject currentGameEvent)
	{
		if (currentGameEvent == null)
		{
			return false;
		}

		try
		{
            return CheckConditions(currentGameEvent.conditions);
        }
        catch (Exception ex)
		{
			Debug.LogError($"Error (CheckConditions EventScriptableObject = {currentGameEvent}): {ex}");
			throw;
		}
	}

	public bool CheckConditions(List<string> str)
	{
		if (str == null)
		{
			return false;
		}

		try
		{
			gameSettings = FindObjectOfType<GameSettings>();
			countryManager = FindObjectOfType<CountryManager>();
			diplomatyUI = FindObjectOfType<DiplomatyUI>();

			int conditionsAccepted = 0;
			int conditionsAmount = 0;
			bool allowEvent = false;

			conditionsAmount = str.Count;

			for (int c = 0; c < str.Count; c++)
			{
				if (CheckConditions(str[c]))
				{
					conditionsAccepted++;
				}
			}
			if (conditionsAccepted >= conditionsAmount)
			{
				return true;
			}
			return false;
		}
        catch (Exception ex)
        {
			Debug.LogError($"Error CheckConditions({str}): {ex}");
            return false;
        }
    }

	public bool CheckConditions(string str)
	{
		try
		{
			string[] condition = str.Split(';');

			if (condition[0] == "not")
			{
				string newstr = (new string(str)).Replace("not;", "");
				bool result = CheckConditions(newstr);
				return !result;
			}
			if (condition[0] == "false")
			{
				return false;
			}
			if (condition[0] == "true")
			{
				return true;
			}

			for (int i = 0; i < condition.Length; i++)
			{
				if (ReferencesManager.Instance.countryManager.botCountry != null)
				{
					condition[i] = condition[i].Replace("current_country", ReferencesManager.Instance.countryManager.botCountry.country._id.ToString());
				}
				else
				{
					condition[i] = condition[i].Replace("current_country", "0");
				}
			}

			if (condition[0] == "country_exist")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				try
				{
					if (attacker.exist && attacker != null && attacker.myRegions.Count > 0)
					{
						return true;
					}
				}
				catch (System.Exception) { }

			}
			else if (condition[0] == "country_not_exist")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				try
				{
					if (!attacker.exist || attacker == null)
					{
						return true;
					}
				}
				catch (System.Exception)
				{

					throw;
				}
			}
			else if (condition[0] == "own_province")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				for (int r = 0; r < attacker.myRegions.Count; r++)
				{
					if (attacker.myRegions[r]._id == int.Parse(condition[2]))
					{
						region = attacker.myRegions[r];
					}
				}

				if (attacker.myRegions.Contains(region))
				{
					return true;
				}
			}
			else if (condition[0] == "not_own_province")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				for (int r = 0; r < attacker.myRegions.Count; r++)
				{
					if (attacker.myRegions[r]._id == int.Parse(condition[2]))
					{
						region = attacker.myRegions[r];
					}
				}

				if (!attacker.myRegions.Contains(region))
				{
					return true;
				}
			}
			else if (condition[0] == "ongoing_war")
			{
				defender = countryManager.FindCountryByID(int.Parse(condition[1]));

				return defender.inWar;
			}
			else if (condition[0] == "date_before")
			{
				string[] dateSecond = condition[1].Split('-');
				if (new DateTime(int.Parse(dateSecond[2]), int.Parse(dateSecond[1]), int.Parse(dateSecond[0])) < new DateTime(ReferencesManager.Instance.dateManager.currentDate[2], ReferencesManager.Instance.dateManager.currentDate[1], ReferencesManager.Instance.dateManager.currentDate[0]))
				{
					return true;
				}
			}
			else if (condition[0] == "date_after")
			{
				string[] dateSecond = condition[1].Split('-');

				if (new DateTime(int.Parse(dateSecond[2]), int.Parse(dateSecond[1]), int.Parse(dateSecond[0])) > new DateTime(ReferencesManager.Instance.dateManager.currentDate[2], ReferencesManager.Instance.dateManager.currentDate[1], ReferencesManager.Instance.dateManager.currentDate[0]))
				{
					return true;
				}
			}
			else if (condition[0] == "in_union")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(condition[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).union)
				{
					return true;
				}
			}
			else if (condition[0] == "not_in_union")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(condition[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).union)
				{
					return true;
				}
			}
			else if (condition[0] == "in_war_with")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(condition[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).war)
				{
					return true;
				}
			}
			else if (condition[0] == "not_in_war_with")
			{
				try
				{
					for (int i = 0; i < countryManager.countries.Count; i++)
					{
						if (countryManager.countries[i].country._id == int.Parse(condition[1]))
						{
							attacker = countryManager.countries[i];
						}
						if (countryManager.countries[i].country._id == int.Parse(condition[2]))
						{
							defender = countryManager.countries[i];
						}
					} // Asign countries

					if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).war)
					{
						return true;
					}
				}
				catch (System.Exception)
				{

				}

			}
			else if (condition[0] == "is_country_player")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}

					if (attacker.isPlayer)
					{
						return true;
					}
				}
			}
			else if (condition[0] == "is_country_not_player")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
				}
				if (attacker != null)
				{
					if (!attacker.isPlayer)
					{
						return true;
					}
				}
			}
			else if (condition[0] == "is_near_to_province")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Assign countries

				for (int r = 0; r < countryManager.regions.Count; r++)
				{
					if (countryManager.regions[r]._id == int.Parse(condition[2]))
					{
						region = countryManager.regions[r];
					}
				}

				foreach (Transform movePoint in region.movePoints)
				{
					if (movePoint.GetComponent<MovePoint>().regionTo.GetComponent<RegionManager>().currentCountry == attacker)
					{
						return true;
					}
				}
			}
			else if (condition[0] == "is_ideology")
			{
				for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
				{
					if (ReferencesManager.Instance.countryManager.countries[i].country._id == int.Parse(condition[1]))
					{
						attacker = ReferencesManager.Instance.countryManager.countries[i];
					}
				} // Assign countries

				string ideology = "";

				if (condition[2] == "n")
				{
					ideology = "Неопределено";
				}
				if (condition[2] == "d")
				{
					ideology = "Демократия";
				}
				if (condition[2] == "c")
				{
					ideology = "Коммунизм";
				}
				if (condition[2] == "f")
				{
					ideology = "Фашизм";
				}
				if (condition[2] == "m")
				{
					ideology = "Монархия";
				}

				if (attacker.ideology == ideology)
				{
					return true;
				}
			}
			//else if (condition[0] == "is_completed_decision") // проверка прожато ли страной решение // 1 - страна, 2 - решение
			//{
			//	foreach (Decisions_ScriptableObj dec in ReferencesManager.Instance.decisions)
			//	{
			//		if (dec.id == Int32.Parse(condition[2]))
			//		{
			//			if (ReferencesManager.Instance.countryManager.FindCountryByID(Int32.Parse(condition[1])).completed_decisions.Contains(dec.id))
			//			{
			//				return true;
			//			}
			//			break;
			//		}
			//	}
			//}
			//else if (condition[0] == "isnt_completed_decision") // проверка не прожато ли страной решение // 1 - страна, 2 - решение
			//{
			//	foreach (Decisions_ScriptableObj dec in ReferencesManager.Instance.decisions)
			//	{
			//		if (dec.id == Int32.Parse(condition[2]))
			//		{
			//			if (!ReferencesManager.Instance.countryManager.FindCountryByID(Int32.Parse(condition[1])).completed_decisions.Contains(dec.id))
			//			{
			//				return true;
			//			}
			//			break;
			//		}
			//	}
			//}
			else if (condition[0] == "random") // рандом // 1 - шанс в прцоентах (0-100)
			{
				if (Int32.Parse(condition[1]) / 100 > UnityEngine.Random.value)
				{
					return true;
				}
			}
			else if (condition[0] == "has_army") // рандом // 1 - страна 2 - ∞ - регионы проверки
			{
				CountrySettings cou = ReferencesManager.Instance.countryManager.FindCountryByID(Int32.Parse(condition[1]));
				List<int> check = new List<int>();
				for (int i = 2; i < condition.Length; i++)
				{
					check.Add(Int32.Parse(condition[i]));
				}
				foreach (RegionManager reg in cou.myRegions)
				{
					if (check.Contains(reg._id))
					{
						if (reg.hasArmy)
						{
							return true;
							break;
						}
					}
				}
			}
			else if (condition[0] == "var_condition")
			{
				foreach (custom_variable var in vars)
				{
					if (var.name == condition[1])
					{
						if (condition[2] == ">")
						{
							if (var.value > Int32.Parse(condition[3]))
							{
								return true;
							}
						}
						else if (condition[2] == "<")
						{
							if (var.value < Int32.Parse(condition[3]))
							{
								return true;
							}
						}
						else if (condition[2] == "<=")
						{
							if (var.value <= Int32.Parse(condition[3]))
							{
								return true;
							}
						}
						else if (condition[2] == ">=")
						{
							if (var.value >= Int32.Parse(condition[3]))
							{
								return true;
							}
						}
						else if (condition[2] == "==")
						{
							if (var.value == Int32.Parse(condition[3]))
							{
								return true;
							}
						}
						else if (condition[2] == "!=")
						{
							if (var.value != Int32.Parse(condition[3]))
							{
								return true;
							}
						}
					}
				}

			}
			else if (condition[0] == "")
			{
				return true;
			}

			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error: {ex}");
			throw;
		}
	}

	public void ProceedEvent(int b)
	{
		try
		{
			if (currentGameEvent != null)
			{

				for (int a = 0; a < currentGameEvent.buttons[b].actions.Count; a++)
				{
					if (!string.IsNullOrEmpty(currentGameEvent.buttons[b].actions[a]) ||
					!string.IsNullOrWhiteSpace(currentGameEvent.buttons[b].actions[a]))
					{

						ExecuteEvent(currentGameEvent.buttons[b].actions[a]);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error: {ex}");
		}

		countryManager.UpdateIncomeValuesUI();
		countryManager.UpdateValuesUI();
	}

	public void ExecuteEvent(string str)
	{
		if (!str.IsNullOrEmpty() && str != null)
		{
			string[] act = str.Split(';');

			for (int i = 0; i < act.Length; i++)
				{
					if (act[i] == "var")
					{
						foreach (custom_variable var in vars)
						{
							if (var.name == act[i + 1])
							{
								str.Replace($"var;{var.name}", var.value.ToString()).Split(';');
							}
						}
					}
				}
			if (act[0] == "annex")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				List<RegionManager> regionsToAnnex = new List<RegionManager>();


				if (act.Length > 2)
				{
					for (int i = 2; i < act.Length; i++)
					{
						foreach (RegionManager _region in countryManager.regions)
						{
							if (_region._id == int.Parse(act[i]))
							{
								regionsToAnnex.Add(_region);
							}
						}
					}

					SmallNewsManager.Instance.countrySender = attacker.country;

					SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(attacker.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Annexed")} {act.Length - 2} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.ProvincesOf")} {ReferencesManager.Instance.languageManager.GetTranslation(regionsToAnnex[0].currentCountry.country._nameEN)}";

					SmallNewsManager.Instance.UpdateUI();

					foreach (RegionManager region in regionsToAnnex)
					{
						ReferencesManager.Instance.AnnexRegion(region, attacker);
					}
				}
			}
			else if (act[0] == "war")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("war", attacker, defender);
			}
			else if (act[0] == "vassal")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("vassal", attacker, defender);
			}
			else if (act[0] == "peace")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				attacker.capitulated = false;
				defender.capitulated = false;

				attacker.exist = true;
				defender.exist = true;

				DiplomatySend("peace", attacker, defender);
			}
			else if (act[0] == "trade")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("trade", attacker, defender);
			}
			else if (act[0] == "untrade")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("untrade", attacker, defender);
			}
			else if (act[0] == "pact")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("pact", attacker, defender);
			}
			else if (act[0] == "unpact")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("unpact", attacker, defender);
			}
			else if (act[0] == "union")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("union", attacker, defender);
			}
			else if (act[0] == "deunion")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				} // Asign countries

				DiplomatySend("deunion", attacker, defender);
			}
			else if (act[0] == "create_country")
			{
				int cId = 0;
				try
				{
					foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
					{
						int.TryParse(act[1], out cId);

						if (country._id == cId)
						{
							ReferencesManager.Instance.CreateCountry(country, act[2]);
						}
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError($"Error: {ex} [{act[1]} ({cId})]");
				}

			}
			else if (act[0] == "add_claim")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				}
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
						{
							if (country._id == int.Parse(act[2]))
							{
								countryManager.regions[i].regionClaims.Add(country);

								SmallNewsManager.Instance.countrySender = country;

								try
								{
									SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Claimed")} {ReferencesManager.Instance.languageManager.GetTranslation(countryManager.regions[i].currentCountry.country._nameEN)}";
								}
								catch (System.Exception) { }

								SmallNewsManager.Instance.UpdateUI();
							}
						}
					}
				}
			}
			else if (act[0] == "remove_claim")
			{
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
						{
							if (country._id == int.Parse(act[2]))
							{
								countryManager.regions[i].regionClaims.Remove(country);

								SmallNewsManager.Instance.countrySender = attacker.country;
								SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.UnClaimed")} {ReferencesManager.Instance.languageManager.GetTranslation(countryManager.regions[i].currentCountry.country._nameEN)}";

								SmallNewsManager.Instance.UpdateUI();
							}
						}
					}
				}
			}
			else if (act[0] == "remove_army")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				for (int i = 0; i < attacker.countryUnits.Count; i++)
				{
					attacker.countryUnits[i].Destroy();
				}

				attacker.countryUnits.Clear();
			}
			else if (act[0] == "create_division")
			{
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						region = countryManager.regions[i];
					}
				}

				ReferencesManager.Instance.army.CreateUnit_NoCheck(region);

				for (int i = 2; i < act.Length; i++)
				{
					string[] army_data = act[i].Split('=');

					if (army_data[0] == "INF_01")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.soldierLVL1;
					}
					else if (army_data[0] == "INF_02")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.soldierLVL2;
					}
					else if (army_data[0] == "INF_03")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.soldierLVL3;
					}
					else if (army_data[0] == "ART_01")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.artileryLVL1;
					}
					else if (army_data[0] == "ART_02")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.artileryLVL2;
					}
					else if (army_data[0] == "HVY_01")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.tankLVL1;
					}
					else if (army_data[0] == "HVY_02")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.tankLVL2;
					}
					else if (army_data[0] == "MIF_01")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.motoLVL1;
					}
					else if (army_data[0] == "MIF_02")
					{
						unitToAdd = ReferencesManager.Instance.gameSettings.motoLVL2;
					}

					for (int ad = 1; ad < int.Parse(army_data[1]); ad++)
					{
						ReferencesManager.Instance.army.AddUnitToArmy_NoCheck(unitToAdd, region);
					}
				}
			}
			else if (act[0] == "war_to_countries_in_regs")
			{
				List<CountrySettings> countriesInActions = new List<CountrySettings>();

				for (int ai = 2; ai < act.Length; ai++)
				{
					foreach (RegionManager region in countryManager.regions)
					{
						if (region._id == int.Parse(act[ai]))
						{
							if (!countriesInActions.Contains(region.currentCountry))
							{
								countriesInActions.Add(region.currentCountry);
							}
						}
					}
				}

				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				} // Asign countries

				foreach (CountrySettings country in countriesInActions)
				{
					DiplomatySend("war", attacker, country);
				}
			}
			else if (act[0] == "add_res")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				}

				if (act[2] == "money")
				{
					attacker.money += int.Parse(act[3]);
				}
				else if (act[2] == "food")
				{
					attacker.food += int.Parse(act[3]);
				}
				else if (act[2] == "recroots")
				{
					attacker.recruits += int.Parse(act[3]);
				}
				else if (act[2] == "research_points")
				{
					attacker.researchPoints += int.Parse(act[3]);
				}
				else if (act[2] == "fuel")
				{
					attacker.fuel += int.Parse(act[3]);
				}
			}
			else if (act[0] == "set_res")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				}

				if (act[2] == "money")
				{
					int setMoney = int.Parse(act[3]);
					attacker.money = setMoney;
				}
				else if (act[2] == "food")
				{
					attacker.food = int.Parse(act[3]);
				}
				else if (act[2] == "recroots")
				{
					attacker.recruits = int.Parse(act[3]);
				}
				else if (act[2] == "research_points")
				{
					attacker.researchPoints = int.Parse(act[3]);
				}
				else if (act[2] == "fuel")
				{
					attacker.fuel = int.Parse(act[3]);
				}
			}
			else if (act[0] == "set_ideology")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				}

				attacker.ideology = act[2];
				attacker.UpdateCountryGraphics(attacker.ideology);

				if (attacker.isPlayer)
				{
					countryManager.UpdateCountryInfo();
				}
			}
			else if (act[0] == "build")
			{
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						region = countryManager.regions[i];
					}
				}

				BuildingScriptableObject building = new BuildingScriptableObject();

				if (act[2] == "FCT") building = ReferencesManager.Instance.gameSettings.fabric;
				else if (act[2] == "FRM") building = ReferencesManager.Instance.gameSettings.farm;
				else if (act[2] == "CFR") building = ReferencesManager.Instance.gameSettings.chefarm;
				else if (act[2] == "INF")
				{
					region.UpgradeInfrastructureForce(region);
				}

				if (act[2] != "INF")
				{
					region.BuildBuilding(building, region, false);
				}
			}
			else if (act[0] == "build_queue")
			{
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						region = countryManager.regions[i];
					}
				}

				BuildingScriptableObject building = new BuildingScriptableObject();

				if (act[2] == "FCT") building = ReferencesManager.Instance.gameSettings.fabric;
				if (act[2] == "FRM") building = ReferencesManager.Instance.gameSettings.farm;
				if (act[2] == "CFR") building = ReferencesManager.Instance.gameSettings.chefarm;

				region.AddBuildingToQueueForce(building, region);
			}
			else if (act[0] == "add_population")
			{
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						region = countryManager.regions[i];
					}
				}

				region.population += int.Parse(act[2]);
			}
			else if (act[0] == "set_population")
			{
				for (int i = 0; i < countryManager.regions.Count; i++)
				{
					if (countryManager.regions[i]._id == int.Parse(act[1]))
					{
						region = countryManager.regions[i];
					}
				}

				region.population = int.Parse(act[2]);
			}
			else if (act[0] == "change")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
						attacker = countryManager.countries[i];
					}
				}

				countryManager.currentCountry.gameObject.AddComponent<CountryAIManager>();
				countryManager.currentCountry.isPlayer = false;
				ReferencesManager.Instance.aiManager.AICountries.Add(countryManager.currentCountry);

				countryManager.currentCountry = attacker;
				countryManager.currentCountry.isPlayer = true;
				ReferencesManager.Instance.aiManager.AICountries.Remove(countryManager.currentCountry);

				Destroy(countryManager.currentCountry.GetComponent<CountryAIManager>());

				ReferencesManager.Instance.countryManager.UpdateCountryInfo();
				ReferencesManager.Instance.countryManager.UpdateValuesUI();
				ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
			}
			else if (act[0] == "set_ai_level")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
				}

				attacker.aiAccuracy = float.Parse(act[2]);
			}
			else if (act[0] == "sync_tech")
			{
				for (int i = 0; i < countryManager.countries.Count; i++)
				{
					if (countryManager.countries[i].country._id == int.Parse(act[1]))
					{
						attacker = countryManager.countries[i];
					}
					if (countryManager.countries[i].country._id == int.Parse(act[2]))
					{
						defender = countryManager.countries[i];
					}
				}

				defender.countryTechnologies = attacker.countryTechnologies;
			}
			else if (act[0] == "create_var")
			{
				bool exist = false;
				foreach (custom_variable var in vars)
				{
					if (var.name == act[1])
					{
						exist = true;
						break;
					}
				}
				if (!exist)
				{
					vars.Add(new custom_variable(act[1], 0));
				}


			}
			else if (act[0] == "var_math")
			{
				for (int i = 0; i < vars.Count; i++)
				{
					if (vars[i].name == act[1])
					{
						if (act[2] == "+")
						{
							vars[i].value += Int32.Parse(act[3]);
						}
						else if (act[2] == "-")
						{
							vars[i].value -= Int32.Parse(act[3]);
						}
						else if (act[2] == "=")
						{
							vars[i].value = Int32.Parse(act[3]);
						}
					}
				}

			}
			else if (act[0] == "DEV_UPD_IDEOLOGY_STABILITY_BUFFS")
			{
				CountrySettings country = countryManager.FindCountryByID(int.Parse(act[1]));

				if (country != null)
				{
					if (country.ideology == "Демократия")
					{
						country.stability.buffs.Add(new Stability_buff("Бюрократические издержки", -5f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("bureaucratic_costs")));
						country.stability.buffs.Add(new Stability_buff("Изменение идеологии", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("ideology_change")));

						country.stability.buffs.Add(new Stability_buff("Свобода слова", 10f, new List<string>() { $"not;is_ideology;{country.country._id};d" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{country.country._id}" }, R.Instance.sprites.Find("freedom_of_speech")));
					}
					else if (country.ideology == "Фашизм")
					{
						country.stability.buffs.Add(new Stability_buff("Сила нации", 15f, new List<string>() { $"not;is_ideology;{country.country._id};f" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{country.country._id}" }, R.Instance.sprites.Find("power_of_nation")));
						country.stability.buffs.Add(new Stability_buff("Изменение идеологии", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("ideology_change")));
					}
					else if (country.ideology == "Коммунизм")
					{
						country.stability.buffs.Add(new Stability_buff("Плановая экономика", 7.5f, new List<string>() { $"not;is_ideology;{country.country._id};c" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{country.country._id}" }, R.Instance.sprites.Find("planned_economy")));
						country.stability.buffs.Add(new Stability_buff("Изменение идеологии", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("ideology_change")));
					}
				}
			}
		}
		else
		{
			Debug.LogError("Event Actions is null");
		}
	}

	private void DiplomatySend(string offer, CountrySettings sender, CountrySettings receiver)
	{
		if (offer == "war")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.war = true;
			senderToReceiver.trade = false;
			senderToReceiver.right = false;
			senderToReceiver.pact = false;
			senderToReceiver.union = false;

			sender.enemies.Add(receiver);
			receiver.enemies.Add(sender);

            if (sender.myRegions.Count > 0 && receiver.myRegions.Count > 0)
            {
                sender.stability.buffs.Add(new Stability_buff("Наступательная война", (-15 * (receiver.myRegions.Count / sender.myRegions.Count)) * (1 / receiver.enemies.Count), new List<string>() { $"not;ongoing_war;{sender.country._id}" }, null, ReferencesManager.Instance.sprites.Find("offensive_war")));
                receiver.stability.buffs.Add(new Stability_buff("Оборонительная война", -5f, new List<string>() { $"not;ongoing_war;{receiver.country._id}" }, null, ReferencesManager.Instance.sprites.Find("defensive_war")));
            }

            sender.inWar = true;
			receiver.inWar = true;

			receiverToSender.war = true;
			receiverToSender.trade = false;
			receiverToSender.right = false;
			receiverToSender.pact = false;
			receiverToSender.union = false;

			senderToReceiver.relationship -= 100;
			receiverToSender.relationship -= 100;

			SmallNewsManager.Instance.countrySender = sender.country;
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.DeclareWar")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "peace")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.war = false;

			sender.enemies.Remove(receiver);
			receiver.enemies.Remove(sender);

			sender.inWar = false;
			receiver.inWar = false;

			receiverToSender.war = false;

			senderToReceiver.relationship += 60;
			receiverToSender.relationship += 60;

			SmallNewsManager.Instance.countrySender = sender.country;
			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Peace")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "trade")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.trade = true;

			receiverToSender.trade = true;

			senderToReceiver.relationship += 18;
			receiverToSender.relationship += 18;

			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Trade")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "untrade")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.trade = false;
			receiverToSender.trade = false;

			senderToReceiver.relationship -= 9;
			receiverToSender.relationship -= 9;

			SmallNewsManager.Instance.countrySender = sender.country;
			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.UnTrade")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "pact")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.pact = true;

			receiverToSender.pact = true;

			senderToReceiver.relationship += 18;
			receiverToSender.relationship += 18;

			SmallNewsManager.Instance.countrySender = sender.country;
			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Pact")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "unpact")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.pact = false;
			receiverToSender.pact = false;

			senderToReceiver.relationship -= 18;
			receiverToSender.relationship -= 18;

			SmallNewsManager.Instance.countrySender = sender.country;
			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.UnPact")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "union")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.union = true;

			receiverToSender.union = true;

			senderToReceiver.relationship += 75;
			receiverToSender.relationship += 75;

			SmallNewsManager.Instance.countrySender = sender.country;
			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Union")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "deunion")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.union = false;
			receiverToSender.union = false;

			senderToReceiver.relationship -= 75;
			receiverToSender.relationship -= 75;

			SmallNewsManager.Instance.countrySender = sender.country;
			SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {ReferencesManager.Instance.languageManager.GetTranslation(sender.country._nameEN)} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.DeUnion")} {ReferencesManager.Instance.languageManager.GetTranslation(receiver.country._nameEN)}";

			SmallNewsManager.Instance.UpdateUI();
		}

		if (offer == "vassal")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.vassal = true;
			receiverToSender.vassal = false;

			senderToReceiver.relationship += 75;
			receiverToSender.relationship += 75;
		}

		if (offer == "devassal")
		{
			Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
			Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

			senderToReceiver.union = false;
			receiverToSender.union = false;

			senderToReceiver.relationship -= 75;
			receiverToSender.relationship -= 75;
		}
	}

	private IEnumerator ClosePanel_Co()
	{
		imageViewAnimator.Play("imageViewClose");
		yield return new WaitForSeconds(0.2f);
		imageViewPanel.SetActive(false);
		yield break;
	}

	public class custom_variable
	{
		public string name;
		public int value;
		public custom_variable(string _name, int _value)
		{
			name = _name;
			value = _value;
		}
	}
}
