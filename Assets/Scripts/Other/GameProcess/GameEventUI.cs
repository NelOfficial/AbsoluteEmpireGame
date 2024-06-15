using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    private void Awake()
    {
        gameSettings = FindObjectOfType<GameSettings>();
        countryManager = FindObjectOfType<CountryManager>();
        diplomatyUI = FindObjectOfType<DiplomatyUI>();
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
        gameSettings = FindObjectOfType<GameSettings>();
        countryManager = FindObjectOfType<CountryManager>();
        diplomatyUI = FindObjectOfType<DiplomatyUI>();

        int conditionsAccepted = 0;
        int conditionsAmount = 0;
        bool allowEvent = false;

        conditionsAmount = currentGameEvent.conditions.Count;

        for (int c = 0; c < currentGameEvent.conditions.Count; c++)
        {
            string[] condition = currentGameEvent.conditions[c].Split(';');

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
                        conditionsAccepted++;
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
                        conditionsAccepted++;
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
                    conditionsAccepted++;
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
                    conditionsAccepted++;
                }
            }
            else if (condition[0] == "ongoing_war")
            {
                for (int i = 0; i < countryManager.countries.Count; i++)
                {
                    if (countryManager.countries[i].country._id == int.Parse(condition[1]))
                    {
                        attacker = countryManager.countries[i];
                    }
                } // Asign countries

                if (attacker.inWar)
                {
                    conditionsAccepted++;
                }
            }
            else if (condition[0] == "date_before")
            {
                string[] dateSecond = condition[1].Split('-');

                if (ReferencesManager.Instance.dateManager.currentDate[0] < int.Parse(dateSecond[0]))
                {
                    if (ReferencesManager.Instance.dateManager.currentDate[1] < int.Parse(dateSecond[1]))
                    {
                        if (ReferencesManager.Instance.dateManager.currentDate[2] < int.Parse(dateSecond[2]))
                        {
                            conditionsAccepted++;
                        }
                    }
                }
            }
            else if (condition[0] == "date_after")
            {
                string[] dateSecond = condition[1].Split('-');

                if (ReferencesManager.Instance.dateManager.currentDate[0] > int.Parse(dateSecond[0]))
                {
                    if (ReferencesManager.Instance.dateManager.currentDate[1] > int.Parse(dateSecond[1]))
                    {
                        if (ReferencesManager.Instance.dateManager.currentDate[2] > int.Parse(dateSecond[2]))
                        {
                            conditionsAccepted++;
                        }
                    }
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
                    conditionsAccepted++;
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
                    conditionsAccepted++;
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
                    conditionsAccepted++;
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
                        conditionsAccepted++;
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
                        conditionsAccepted++;
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
                        conditionsAccepted++;
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
                        conditionsAccepted++;
                        break;
                    }
                }
            }
            else if (condition[0] == "is_ideology")
            {
                for (int i = 0; i < countryManager.countries.Count; i++)
                {
                    if (countryManager.countries[i].country._id == int.Parse(condition[1]))
                    {
                        attacker = countryManager.countries[i];
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
                    conditionsAccepted++;
                }
            }
            else if (condition[0] == "")
            {
                conditionsAccepted++;
            }

            if (conditionsAccepted >= conditionsAmount)
            {
                allowEvent = true;
            }
            else
            {
                allowEvent = false;
            }
        }

        return allowEvent;
    }

    public void ProceedEvent(int b)
    {
        //try
        //{
            if (currentGameEvent != null)
            {

                for (int a = 0; a < currentGameEvent.buttons[b].actions.Count; a++)
                {
                    if (!string.IsNullOrEmpty(currentGameEvent.buttons[b].actions[a]) ||
                    !string.IsNullOrWhiteSpace(currentGameEvent.buttons[b].actions[a]))
                    {

                        string[] act = currentGameEvent.buttons[b].actions[a].Split(';');

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

                            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {attacker.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Annexed")} {act.Length - 2} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.ProvincesOf")} {regionsToAnnex[0].currentCountry.country._name}";

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
                            foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
                            {
                                if (country._id == int.Parse(act[1]))
                                {
                                    ReferencesManager.Instance.CreateCountry(country, act[2]);
                                }
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
                                            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Claimed")} {countryManager.regions[i].currentCountry.country._name}";
                                        }
                                        catch (System.Exception){}

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
                                        SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.UnClaimed")} {countryManager.regions[i].currentCountry.country._name}";

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
                                attacker.recroots += int.Parse(act[3]);
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
                                attacker.recroots = int.Parse(act[3]);
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
                    }
                }
            }

        //catch (System.Exception ex)
        //{
        //    Debug.LogError($"Exception: {ex.Message}");
        //}

        countryManager.UpdateIncomeValuesUI();
        countryManager.UpdateValuesUI();
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

            sender.enemy = receiver;
            receiver.enemy = sender;

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

            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.DeclareWar")} {receiver.country._name}";

            SmallNewsManager.Instance.UpdateUI();
        }

        if (offer == "peace")
        {
            Relationships.Relation senderToReceiver = diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.war = false;

            sender.enemy = null;
            receiver.enemy = null;

            sender.inWar = false;
            receiver.inWar = false;

            receiverToSender.war = false;

            senderToReceiver.relationship += 60;
            receiverToSender.relationship += 60;

            SmallNewsManager.Instance.countrySender = sender.country;
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Peace")} {receiver.country._name}";

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

            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Trade")} {receiver.country._name}";

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
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.UnTrade")} {receiver.country._name}";

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
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Pact")} {receiver.country._name}";

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
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.UnPact")} {receiver.country._name}";

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
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.Union")} {receiver.country._name}";

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
            SmallNewsManager.Instance.message = $"{ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.State")} {sender.country._name} {ReferencesManager.Instance.languageManager.GetTranslation("SmallNews.DeUnion")} {receiver.country._name}";

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
}
