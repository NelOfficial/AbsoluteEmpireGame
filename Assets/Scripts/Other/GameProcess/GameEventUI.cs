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
            descriptionText.text = currentGameEvent.description;
            titleText.text = currentGameEvent._name;
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
                spawnedButton.GetComponent<EventButtonUI>()._buttonName = currentGameEvent.buttons[i].name;
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

    public void ProceedEvent(int b)
    {
        bool allowEvent = false;

        for (int a = 0; a < currentGameEvent.buttons[b].actions.Count; a++)
        {
            if (!string.IsNullOrEmpty(currentGameEvent.buttons[b].actions[a]) ||
            !string.IsNullOrWhiteSpace(currentGameEvent.buttons[b].actions[a]))
            {

                string[] act = currentGameEvent.buttons[b].actions[a].Split(';');
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

                        attacker.UpdateCapitulation();

                        if (attacker.exist)
                        {
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
                        }
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

                        attacker.UpdateCapitulation();

                        if (!attacker.exist)
                        {
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
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
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
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
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
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
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
                        }
                    }
                    else if (condition[0] == "date_before")
                    {
                        string[] dateSecond = condition[1].Split('-');

                        if (ReferencesManager.Instance.dateManager.currentDate[0] < int.Parse(dateSecond[0]))
                        {
                            allowEvent = true;
                        }
                        if (ReferencesManager.Instance.dateManager.currentDate[1] < int.Parse(dateSecond[1]))
                        {
                            allowEvent = true;
                        }
                        if (ReferencesManager.Instance.dateManager.currentDate[2] < int.Parse(dateSecond[2]))
                        {
                            allowEvent = true;
                        }
                    }
                    else if (condition[0] == "date_after")
                    {
                        string[] dateSecond = condition[1].Split('-');

                        if (ReferencesManager.Instance.dateManager.currentDate[0] > int.Parse(dateSecond[0]))
                        {
                            allowEvent = true;
                        }
                        if (ReferencesManager.Instance.dateManager.currentDate[1] > int.Parse(dateSecond[1]))
                        {
                            allowEvent = true;
                        }
                        if (ReferencesManager.Instance.dateManager.currentDate[2] > int.Parse(dateSecond[2]))
                        {
                            allowEvent = true;
                        }
                    }
                    else if (condition[0] == "in_union")
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

                        if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).union)
                        {
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
                        }
                    }
                    else if (condition[0] == "not_in_union")
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

                        if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).union)
                        {
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
                        }
                    }
                    else if (condition[0] == "in_war_with")
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

                        if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).war)
                        {
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
                        }
                    }
                    else if (condition[0] == "not_in_war_with")
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

                        if (!ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(attacker, defender).war)
                        {
                            allowEvent = true;
                        }
                        else
                        {
                            allowEvent = false;
                        }
                    }
                    else if (condition[0] == "is_country_player")
                    {
                        for (int i = 0; i < countryManager.countries.Count; i++)
                        {
                            if (countryManager.countries[i].country._id == int.Parse(act[1]))
                            {
                                attacker = countryManager.countries[i];
                            }

                            allowEvent = attacker.isPlayer;
                        }
                    }
                    else if (condition[0] == "is_country_not_player")
                    {
                        for (int i = 0; i < countryManager.countries.Count; i++)
                        {
                            if (countryManager.countries[i].country._id == int.Parse(act[1]))
                            {
                                attacker = countryManager.countries[i];
                            }

                            allowEvent = !attacker.isPlayer;
                        }
                    }
                    else if (condition[0] == "")
                    {
                        allowEvent = true;
                    }

                    if (allowEvent)
                    {
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
                                    regionsToAnnex.Add(countryManager.regions[int.Parse(act[i])]);
                                }

                                SmallNewsManager.Instance.countrySender = attacker.country;
                                SmallNewsManager.Instance.message = $"Государство {attacker.country._name} присоединило {act.Length - 2} провинций государства {countryManager.regions[int.Parse(act[2])].currentCountry.country._name}";
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
                            for (int i = 0; i < countryManager.regions.Count; i++)
                            {
                                if (countryManager.regions[i]._id == int.Parse(act[1]))
                                {
                                    foreach (CountryScriptableObject country in ReferencesManager.Instance.globalCountries)
                                    {
                                        if (country._id == int.Parse(act[2]))
                                        {
                                            countryManager.regions[i].regionClaims.Add(country);

                                            SmallNewsManager.Instance.countrySender = attacker.country;
                                            SmallNewsManager.Instance.message = $"Государство {country._name} заявило претензии на провинции государства {countryManager.regions[i].currentCountry.country._name}";
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
                                            SmallNewsManager.Instance.message = $"Государство {country._name} отозвало претензии на провинции государства {countryManager.regions[i].currentCountry.country._name}";
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
                        else if (act[0] == "create_army")
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

                                if (act[2] == "money")
                                {
                                    attacker.money = int.Parse(act[3]);
                                }
                                else if (act[2] == "food")
                                {
                                    attacker.food = int.Parse(act[3]);
                                }
                                else if (act[2] == "recroots")
                                {
                                    attacker.recroots = int.Parse(act[3]);
                                }
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

                            region.BuildBuilding(building, region, false);
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
                    }
                }
            }
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} объявило войну государству {receiver.country._name}";
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} заключило мир с государством {receiver.country._name}";
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

            SmallNewsManager.Instance.countrySender = sender.country;
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} заключило торговлю с государством {receiver.country._name}";
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} расторгло торговлю с государством {receiver.country._name}";
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} подписало пакт о ненападении с государством {receiver.country._name}";
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} расторгло пакт о ненападении с государством {receiver.country._name}";
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} заключило союз с государством {receiver.country._name}";
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
            SmallNewsManager.Instance.message = $"Государство {sender.country._name} расторгло союз с государством {receiver.country._name}";
            SmallNewsManager.Instance.UpdateUI();
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
