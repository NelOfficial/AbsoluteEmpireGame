using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

public class ProgressManager : MonoBehaviour
{
    public Multiplayer multiplayer;

    public GameObject countryMovePanel;
    public Image countryMoveImage;
    public TMP_Text countryMoveName;

    [HideInInspector] public int progressIndex = 0;

    public void NextProgress()
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == multiplayer.m_ReadyPlayers)
            {
                this.GetComponent<PhotonView>().RPC("RPC_NextMove", RpcTarget.All);

                for (int i = 0; i < PhotonNetwork.CurrentRoom.Players.Count; i++)
                {
                    this.GetComponent<PhotonView>().RPC("RPC_SetReady", RpcTarget.All, PhotonNetwork.CurrentRoom.Players[i].NickName);
                }
            }
            else
            {
                if (!this.GetComponent<PhotonView>())
                {
                    this.gameObject.AddComponent<PhotonView>().ViewID = 502;
                }
                this.GetComponent<PhotonView>().RPC("RPC_SetReady", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            }
        }
        else
        {
            ReferencesManager.Instance.dateManager.currentDate[0] += 10;

            ReferencesManager.Instance.dateManager.UpdateUI();
            ReferencesManager.Instance.dateManager.CheckGameEvents();

            List<UnitMovement> units = new List<UnitMovement>();

            try
            {
                for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
                {
                    for (int unitIndex = 0; unitIndex < ReferencesManager.Instance.countryManager.countries[i].countryUnits.Count; unitIndex++)
                    {
                        ReferencesManager.Instance.countryManager.countries[i].countryUnits.RemoveAll(item => item == null);
                        units.Add(ReferencesManager.Instance.countryManager.countries[i].countryUnits[unitIndex]);
                    }
                }
            }
            catch (System.Exception)
            {

            }

            for (int i = 0; i < units.Count; i++)
            {
                int motorized = 0;
                units[i]._movePoints = 1;
                units[i].firstMove = true;

                for (int unitIndex = 0; unitIndex < units[i].unitsHealth.Count; unitIndex++)
                {
                    UnitMovement.UnitHealth unit = units[i].unitsHealth[unitIndex];

                    if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED ||
                        unit.unit.type == UnitScriptableObject.Type.TANK)
                    {
                        motorized++;
                    }
                }

                if (motorized >= 6)
                {
                    units[i]._movePoints = 2;
                }

                units[i].UpdateInfo();

                //if (ReferencesManager.Instance.diplomatyUI.
                //    FindCountriesRelation(units[i].currentProvince.currentCountry, units[i].currentCountry).war)
                //{
                //    ReferencesManager.Instance.AnnexRegion(units[i].currentProvince, units[i].currentCountry);
                //}
            }

            ReferencesManager.Instance.regionManager.DeselectRegions();
            if (!ReferencesManager.Instance.gameSettings.spectatorMode) ReferencesManager.Instance.countryManager.currentCountry.UpdateCapitulation();

            if (ReferencesManager.Instance.technologyManager.currentTech != null && ReferencesManager.Instance.technologyManager.currentTech.tech != null)
            {
                if (ReferencesManager.Instance.technologyManager.currentTech != null && ReferencesManager.Instance.technologyManager.currentTech.tech != null)
                {
                    ReferencesManager.Instance.technologyManager.currentTech.moves--;
                    ReferencesManager.Instance.technologyManager.SetResearchState(true);
                }

                if (ReferencesManager.Instance.technologyManager.currentTech.moves <= 0)
                {
                    WarningManager.Instance.Warn($"Успешно исследована технология: {ReferencesManager.Instance.technologyManager.currentTech.tech._name}");
                    ReferencesManager.Instance.technologyManager.SetResearchState(false);

                    ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies.Add(ReferencesManager.Instance.technologyManager.currentTech.tech);
                    ReferencesManager.Instance.technologyManager.currentTech = null;
                }
            }


            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = (int)(ReferencesManager.Instance.countryManager.countries[i].inflation * ReferencesManager.Instance.countryManager.countries[i].money / 100);

                if (ReferencesManager.Instance.countryManager.countries[i].inflationDebuff < 0)
                {
                    try
                    {
                        ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = Mathf.CeilToInt(ReferencesManager.Instance.countryManager.countries[i].inflationDebuff);
                        ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = Mathf.Abs(ReferencesManager.Instance.countryManager.countries[i].inflationDebuff);
                    }
                    catch (System.Exception)
                    {

                    }

                }

                //if (ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI <= 0 &&
                //    ReferencesManager.Instance.countryManager.countries[i].money < 0)
                //{
                //    if (ReferencesManager.Instance.countryManager.countries[i].money / ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI >= 20)
                //    {
                //        ReferencesManager.Instance.countryManager.countries[i].inflationDebuff *= 50;
                //        ReferencesManager.Instance.countryManager.countries[i].inflation *= 50;
                //    }
                //}
            }

            ReferencesManager.Instance.countryInfo.currentCreditMoves--;
            ReferencesManager.Instance.countryInfo.CheckCredit();

            foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
            {

                for (int _i = 0; _i < region.buildingsQueue.Count; _i++)
                {
                    int buildSpeed = 1;

                    if (region.infrastructure_Amount < 4) buildSpeed = 1;
                    else if (region.infrastructure_Amount == 4) buildSpeed = 2;
                    else if (region.infrastructure_Amount == 6) buildSpeed = 3;
                    else if (region.infrastructure_Amount == 8) buildSpeed = 4;
                    else if (region.infrastructure_Amount == 10) buildSpeed = 5;

                    region.buildingsQueue[_i].movesLasts -= buildSpeed;
                }

                region.CheckRegionUnits(region);
                region.CheckBuildedBuildignsUI(region);

                CheckQueue(region);

                region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.2f;
                region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.2f;
                region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.2f;
                region.selectedColor.a = 0.5f;

                region.hoverColor.r = region.currentCountry.country.countryColor.r + 0.3f;
                region.hoverColor.g = region.currentCountry.country.countryColor.g + 0.3f;
                region.hoverColor.b = region.currentCountry.country.countryColor.b + 0.3f;
                region.hoverColor.a = 0.5f;
            }

            ReferencesManager.Instance.aiManager.OnNextMove();
            progressIndex++;

            foreach (TradeBuff tradeBuff in ReferencesManager.Instance.gameSettings.diplomatyUI.globalTrades)
            {
                if (tradeBuff.sender.moneyNaturalIncome > 0 && tradeBuff.sender.foodNaturalIncome > 0)
                {
                    tradeBuff.senderMoneyTrade = Mathf.Abs(tradeBuff.sender.moneyNaturalIncome / 100 * 2);
                    tradeBuff.senderFoodTrade = Mathf.Abs(tradeBuff.sender.foodNaturalIncome / 100 * 2);
                }

                if (tradeBuff.receiver.moneyNaturalIncome > 0 && tradeBuff.receiver.foodNaturalIncome > 0)
                {
                    tradeBuff.receiverMoneyTrade = Mathf.Abs(tradeBuff.receiver.moneyNaturalIncome / 100 * 2);
                    tradeBuff.receiverFoodTradee = Mathf.Abs(tradeBuff.receiver.foodNaturalIncome / 100 * 2);
                }

                tradeBuff.sender.moneyTradeIncome = tradeBuff.receiverMoneyTrade;
                tradeBuff.sender.foodTradeIncome = tradeBuff.receiverFoodTradee;

                tradeBuff.receiver.moneyTradeIncome = tradeBuff.senderFoodTrade;
                tradeBuff.receiver.foodTradeIncome = tradeBuff.senderFoodTrade;
            }

            for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
            {
                CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

                country.inflationDebuff = Mathf.Abs(country.inflationDebuff);

                country.moneyNaturalIncome = country.civFactories * ReferencesManager.Instance.gameSettings.fabric.goldIncome + country.farms * ReferencesManager.Instance.gameSettings.farm.goldIncome + country.chemicalFarms * ReferencesManager.Instance.gameSettings.chefarm.goldIncome;
                country.moneyIncomeUI = country.moneyNaturalIncome + country.moneyTradeIncome - Mathf.FloorToInt(country.inflationDebuff) - country.regionCosts;

                country.foodNaturalIncome = country.farms * ReferencesManager.Instance.gameSettings.farm.foodIncome + country.chemicalFarms * ReferencesManager.Instance.gameSettings.chefarm.foodIncome;
                country.foodIncomeUI = country.foodNaturalIncome + country.foodTradeIncome;

                country.money += country.moneyIncomeUI;
                country.food += country.foodIncomeUI;
                country.recroots += country.recrootsIncome;
            }
            ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
            ReferencesManager.Instance.countryManager.UpdateValuesUI();
        }

        List<RegionInfoCanvas> regionInfoCanvases = new List<RegionInfoCanvas>();
        regionInfoCanvases = FindObjectsOfType<RegionInfoCanvas>().ToList();

        for (int i = 0; i < regionInfoCanvases.Count; i++)
        {
            ReferencesManager.Instance.mainCamera.regionInfos.Remove(regionInfoCanvases[i]);
            Destroy(regionInfoCanvases[i].gameObject);
        }
    }

    [PunRPC]
    private void RPC_SetReady(string playerNickName)
    {
        foreach (var player in ReferencesManager.Instance.gameSettings.multiplayer.roomPlayers)
        {
            if (player.currentNickname == playerNickName)
            {
                if (player.readyToMove)
                {
                    player.readyToMove = false;
                    multiplayer.m_ReadyPlayers--;
                }
                else
                {
                    player.readyToMove = true;
                    multiplayer.m_ReadyPlayers++;
                }
            }
        }
        ReferencesManager.Instance.gameSettings.multiplayer.UpdatePlayerListUI();
    }

    [PunRPC]
    private void RPC_NextMove()
    {
        ReferencesManager.Instance.dateManager.currentDate[0] += 10;

        ReferencesManager.Instance.dateManager.UpdateUI();
        ReferencesManager.Instance.dateManager.CheckGameEvents();

        UnitMovement[] units = FindObjectsOfType<UnitMovement>();

        for (int i = 0; i < units.Length; i++)
        {
            int motorized = 0;
            units[i]._movePoints = 1;
            units[i].firstMove = true;

            for (int unitIndex = 0; unitIndex < units[i].unitsHealth.Count; unitIndex++)
            {
                UnitMovement.UnitHealth unit = units[i].unitsHealth[unitIndex];

                if (unit.unit.type == UnitScriptableObject.Type.SOLDIER_MOTORIZED)
                {
                    motorized++;
                }
            }

            if (motorized >= 6)
            {
                units[i]._movePoints = 2;
            }

            //CAN BE REMOVED CAUSE OF OPTIMZATON
            units[i].currentProvince = units[i].transform.parent.GetComponent<RegionManager>();
        }

        ReferencesManager.Instance.regionManager.DeselectRegions();

        if (ReferencesManager.Instance.technologyManager.currentTech != null && ReferencesManager.Instance.technologyManager.currentTech.tech != null)
        {
            if (ReferencesManager.Instance.technologyManager.currentTech != null && ReferencesManager.Instance.technologyManager.currentTech.tech != null)
            {
                ReferencesManager.Instance.technologyManager.currentTech.moves--;
                ReferencesManager.Instance.technologyManager.SetResearchState(true);
            }

            if (ReferencesManager.Instance.technologyManager.currentTech.moves <= 0)
            {
                if (ReferencesManager.Instance.gameSettings.onlineGame)
                {
                    for (int i = 0; i < ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies.Count; i++)
                    {
                        if (ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies[i] == ReferencesManager.Instance.technologyManager.currentTech.tech)
                        {
                            Multiplayer.Instance.AddTechnology(ReferencesManager.Instance.countryManager.currentCountry.country._id, i);
                        }
                    }
                }
                else
                {
                    WarningManager.Instance.Warn($"Успешно исследована технология: {ReferencesManager.Instance.technologyManager.currentTech.tech._name}");
                    ReferencesManager.Instance.technologyManager.SetResearchState(false);
                    ReferencesManager.Instance.countryManager.currentCountry.countryTechnologies.Add(ReferencesManager.Instance.technologyManager.currentTech.tech);
                    ReferencesManager.Instance.technologyManager.currentTech = null;
                }
            }
        }


        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI / 100 * (int)ReferencesManager.Instance.countryManager.countries[i].inflation;
            ReferencesManager.Instance.countryManager.countries[i].inflationDebuff = Mathf.Abs(ReferencesManager.Instance.countryManager.countries[i].inflationDebuff);

            if (ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI != 0)
            {
                if (ReferencesManager.Instance.countryManager.countries[i].money / ReferencesManager.Instance.countryManager.countries[i].moneyIncomeUI >= 20)
                {
                    ReferencesManager.Instance.countryManager.countries[i].inflationDebuff *= 50;
                    ReferencesManager.Instance.countryManager.countries[i].inflation *= 50;
                }
            }

            Multiplayer.Instance.SetCountryValues(
                ReferencesManager.Instance.countryManager.countries[i].country._id,
                ReferencesManager.Instance.countryManager.countries[i].money,
                ReferencesManager.Instance.countryManager.countries[i].food,
                ReferencesManager.Instance.countryManager.countries[i].recroots);
        }

        ReferencesManager.Instance.countryInfo.currentCreditMoves--;
        ReferencesManager.Instance.countryInfo.CheckCredit();

        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.2f;
            region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.2f;
            region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.2f;
            region.selectedColor.a = 0.5f;

            region.hoverColor.r = region.currentCountry.country.countryColor.r + 0.3f;
            region.hoverColor.g = region.currentCountry.country.countryColor.g + 0.3f;
            region.hoverColor.b = region.currentCountry.country.countryColor.b + 0.3f;
            region.hoverColor.a = 0.5f;

            for (int _i = 0; _i < region.buildingsQueue.Count; _i++)
            {
                int buildSpeed = 1;

                if (region.infrastructure_Amount < 4) buildSpeed = 1;
                else if (region.infrastructure_Amount == 4) buildSpeed = 2;
                else if (region.infrastructure_Amount == 6) buildSpeed = 3;
                else if (region.infrastructure_Amount == 8) buildSpeed = 4;
                else if (region.infrastructure_Amount == 10) buildSpeed = 5;

                region.buildingsQueue[_i].movesLasts -= buildSpeed;
            }

            CheckQueue(region);
        }

        ReferencesManager.Instance.aiManager.OnNextMove();
        progressIndex++;

        foreach (TradeBuff tradeBuff in ReferencesManager.Instance.gameSettings.diplomatyUI.globalTrades)
        {

            Multiplayer.Instance.SetCountryValues(
                tradeBuff.sender.country._id,
                tradeBuff.sender.money,
                tradeBuff.sender.food,
                tradeBuff.sender.recroots);

            Multiplayer.Instance.SetCountryValues(
                tradeBuff.receiver.country._id,
                tradeBuff.receiver.money,
                tradeBuff.receiver.food,
                tradeBuff.receiver.recroots);
        }

        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];
            country.inflationDebuff = Mathf.Abs(country.inflationDebuff);

            country.money += country.moneyIncomeUI;
            country.moneyNaturalIncome = country.civFactories * ReferencesManager.Instance.gameSettings.fabric.goldIncome;
            country.moneyIncomeUI = country.moneyNaturalIncome + country.moneyTradeIncome - Mathf.FloorToInt(country.inflationDebuff) - country.regionCosts;

            Multiplayer.Instance.SetCountryValues(country.country._id, country.money, country.food, country.recroots);
            Multiplayer.Instance.SetCountryIncomeValues(country.country._id, country.moneyNaturalIncome, country.foodNaturalIncome, country.recrootsIncome);
            Multiplayer.Instance.SetCountryNaturalIncomeValues(country.country._id, country.moneyNaturalIncome, country.foodNaturalIncome);
        }

        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
    }

    private void CheckQueue(RegionManager region)
    {
        for (int _i = 0; _i < region.buildingsQueue.Count; ++_i)
        {
            if (region.buildingsQueue[_i].movesLasts <= 0)
            {
                region.BuildBuilding(region.buildingsQueue[_i].building, region.buildingsQueue[_i].region, true);
                region.buildingsQueue.Remove(region.buildingsQueue[_i]);

                Multiplayer.Instance.SetRegionValues(region._id, region.population, region.hasArmy, region.goldIncome, region.foodIncome,
                    region.civFactory_Amount, region.infrastructure_Amount, region.farms_Amount, region.cheFarms, region.regionScore);
            }
        }
    }
}
