using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Runtime.CompilerServices;
using static GameSettings;

public class Multiplayer : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;

    [HideInInspector] public static Multiplayer Instance;
    public int m_ReadyPlayers = 0;

    public List<PlayerData> roomPlayers = new List<PlayerData>();

    RegionManager fromRegion;
    RegionManager toRegion;

    CountrySettings sender;
    CountrySettings receiver;

    [SerializeField] GameObject _playerListButton;

    private CountryManager countryManager;


    private void Awake()
    {
        Instance = this;
        _photonView = GetComponent<PhotonView>();

        countryManager = ReferencesManager.Instance.countryManager;
    }

    private void Start()
    {
        _playerListButton.SetActive(ReferencesManager.Instance.gameSettings.onlineGame);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
    }

    public void UpdatePlayerListUI()
    {
        roomPlayers = FindObjectsOfType<PlayerData>().ToList();

        foreach (Transform child in ReferencesManager.Instance.gameSettings.playersListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            PlayerData targetPlayer = roomPlayers.Find(x => x.player == player);

            GameObject spawnedPlayerListPrefab = Instantiate(ReferencesManager.Instance.gameSettings.playerListPrefab, ReferencesManager.Instance.gameSettings.playersListContent);
            PlayerListItem_InGame playerListItem = spawnedPlayerListPrefab.GetComponent<PlayerListItem_InGame>();

            playerListItem._country = targetPlayer.country;
            playerListItem._nickname = targetPlayer.currentNickname;
            playerListItem.isReady = targetPlayer.readyToMove;
            playerListItem.UpdateUI();

            spawnedPlayerListPrefab.transform.localScale = new Vector3(1, 1, 1);
        }
    }


    #region CountryValues

    public void SetCountryValues(int countryID, int money, int food, int recroots)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_SetCountryValues", RpcTarget.All, countryID, money, food, recroots);
    }

    public void SetCountryIncomeValues(int countryID, int moneyIncome, int foodIncome, int recrootsIncome)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_SetCountryIncomeValues", RpcTarget.All, countryID, moneyIncome, foodIncome, recrootsIncome);
    }

    public void SetCountryNaturalIncomeValues(int countryID, int moneyIncome, int foodIncome)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_SetCountryNaturalIncomeValues", RpcTarget.All, countryID, moneyIncome, foodIncome);
    }

    [PunRPC]
    private void RPC_SetCountryValues(int countryID, int money, int food, int recroots)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

            if (country.country._id == countryID)
            {
                country.money = money;
                country.food = food;
                country.recruits = recroots;
            }
        }
    }

    [PunRPC]
    private void RPC_SetCountryIncomeValues(int countryID, int moneyIncome, int foodIncome, int recrootsIncome)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

            if (country.country._id == countryID)
            {
                country.moneyNaturalIncome = moneyIncome;
                country.foodNaturalIncome = foodIncome;
                country.recruitsIncome = recrootsIncome;
            }
        }
    }

    [PunRPC]
    private void RPC_SetCountryNaturalIncomeValues(int countryID, int moneyIncome, int foodIncome)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            CountrySettings country = ReferencesManager.Instance.countryManager.countries[i];

            if (country.country._id == countryID)
            {
                country.moneyNaturalIncome = moneyIncome;
                country.foodNaturalIncome = foodIncome;
            }
        }
    }

    #endregion

    #region CountryIdeology

    public void M_UpdateCountryGraphics(int countryID, string ideology)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_UpdateCountryGraphics", RpcTarget.All, countryID, ideology);
    }

    [PunRPC]
    private void RPC_UpdateCountryGraphics(int countryID, string ideology)
    {
        CountrySettings currentCountry = ReferencesManager.Instance.countryManager.FindCountryByID(countryID);

        if (currentCountry.country._id == countryID)
        {
            if (ideology == "Неопределённый")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[1];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[1];
            }
            else if (ideology == "Демократия")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[1];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[1];
            }
            else if (ideology == "Монархия")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[2];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[2];
            }
            else if (ideology == "Фашизм")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[4];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[4];
            }
            else if (ideology == "Коммунизм")
            {
                currentCountry.country.countryColor = currentCountry.country.countryIdeologyColors[5];
                currentCountry.country.countryFlag = currentCountry.country.countryIdeologyFlags[5];
            }
        }

        // Новый цвет
        foreach (RegionManager region in currentCountry.myRegions)
        {
            region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.1f;
            region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.1f;
            region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.1f;
        }

        ReferencesManager.Instance.countryManager.UpdateRegionsColor();
    }

    #endregion

    #region Region

    public void SetRegionValues(int regionID, int population, bool hasArmy, int goldIncome,
        int foodIncome, int civFactory_Amount, int infrastructure_Amount, int farms_Amount, int cheFarms, int regionScore)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_SetRegionValues", RpcTarget.All, regionID, population, hasArmy, goldIncome,
                foodIncome, civFactory_Amount, infrastructure_Amount, farms_Amount, cheFarms, regionScore);
    }

    public void AnnexRegion(int regionID, int newCountryID)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_AnnexRegion", RpcTarget.All, regionID, newCountryID);
    }

    [PunRPC]
    private void RPC_SetRegionValues(int regionID, int population, bool hasArmy, int goldIncome,
        int foodIncome, int civFactory_Amount, int infrastructure_Amount, int farms_Amount, int cheFarms, int regionScore)
    {
        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            if (region._id == regionID)
            {
                region.buildings.Clear();

                region.population = population;
                region.hasArmy = hasArmy;
                region.goldIncome = goldIncome;
                region.foodIncome = foodIncome;

                region.civFactory_Amount = civFactory_Amount;

                for (int i = 0; i < civFactory_Amount; i++)
                {
                    region.buildings.Add(ReferencesManager.Instance.gameSettings.fabric);
                }

                region.infrastructure_Amount = infrastructure_Amount;

                region.farms_Amount = farms_Amount;

                for (int i = 0; i < farms_Amount; i++)
                {
                    region.buildings.Add(ReferencesManager.Instance.gameSettings.farm);
                }

                region.cheFarms = cheFarms;

                for (int i = 0; i < cheFarms; i++)
                {
                    region.buildings.Add(ReferencesManager.Instance.gameSettings.chefarm);
                }

                region.regionScore = regionScore;
            }
        }
    }


    [PunRPC]
    private void RPC_AnnexRegion(int regionID, int newCountryID)
    {
        foreach (var country in ReferencesManager.Instance.countryManager.countries)
        {
            if (country.country._id == newCountryID)
            {
                CountrySettings newCountry = country;

                foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
                {
                    if (region._id == regionID)
                    {
                        region.currentCountry.myRegions.Remove(region);
                        newCountry.myRegions.Add(region);

                        region.currentCountry = newCountry;
                        region.defaultColor = newCountry.country.countryColor;

                        region.gameObject.name = $"AnxReg_{Random.Range(0, 9999)}";
                        region.transform.SetParent(newCountry.transform);
                        region.transform.SetAsLastSibling();

                        region.GetComponent<SpriteRenderer>().color = newCountry.country.countryColor;

                        region.selectedColor.r = region.currentCountry.country.countryColor.r + 0.1f;
                        region.selectedColor.g = region.currentCountry.country.countryColor.g + 0.1f;
                        region.selectedColor.b = region.currentCountry.country.countryColor.b + 0.1f;
                    }
                }
            }
        }
    }

    #endregion

    #region Technology

    public void AddTechnology(int countryID, int technologyID)
    {
        if (ReferencesManager.Instance.gameSettings.onlineGame)
            _photonView.RPC("RPC_AddTechnology", RpcTarget.All, countryID, technologyID);
    }

    [PunRPC]
    private void RPC_AddTechnology(int countryID, int technologyID)
    {
        foreach (CountrySettings country in ReferencesManager.Instance.countryManager.countries)
        {
            if (country.country._id == countryID)
            {
                country.countryTechnologies.Add(ReferencesManager.Instance.gameSettings.technologies[technologyID]);
            }
        }
    }

    #endregion

    #region Army

    public void AddUnitToArmy(string unitName, int regionId)
    {
        _photonView.RPC("RPC_AddUnitToArmy", RpcTarget.All, unitName, regionId);
    }

    public void CreateUnit(int regionId)
    {
        _photonView.RPC("RPC_CreateUnit", RpcTarget.All, regionId);
    }

    public void RemoveUnitFromArmy(int id, int regionId)
    {
        _photonView.RPC("RPC_RemoveUnitFromArmy", RpcTarget.All, id, regionId);
    }

    public void MoveUnit(int fromRegionId, int toRegionId)
    {
        _photonView.RPC("RPC_MoveUnit", RpcTarget.All, fromRegionId, toRegionId);
    }

    public void DisbandDivision(int regionId, int ownerId)
    {
        _photonView.RPC("RPC_DisbandDivision", RpcTarget.All, regionId, ownerId);
    }

    [PunRPC]
    private void RPC_RemoveUnitFromArmy(int unitId, int regionId)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            if (ReferencesManager.Instance.countryManager.regions[i]._id == regionId)
            {
                RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

                Transform unitTransform = region.transform.Find("Unit(Clone)");
                UnitMovement unitMovement;

                unitMovement = unitTransform.GetComponent<UnitMovement>();

                if (unitMovement.unitsHealth.Count > 0)
                {
                    for (int j = 0; j < unitMovement.unitsHealth.Count; j++)
                    {
                        if (unitMovement.unitsHealth[j]._id == unitId)
                        {
                            region.currentCountry.recruits += unitMovement.unitsHealth[j].unit.recrootsCost;
                            region.currentCountry.moneyNaturalIncome += unitMovement.unitsHealth[j].unit.moneyIncomeCost;
                            region.currentCountry.foodNaturalIncome += unitMovement.unitsHealth[j].unit.foodIncomeCost;

                            unitMovement.unitsHealth.Remove(unitMovement.unitsHealth[j]);
                        }
                    }

                    if (unitMovement.unitsHealth.Count <= 0)
                    {
                        region.DeselectRegions();
                        unitTransform.GetComponent<UnitMovement>().currentCountry.countryUnits.Remove(unitTransform.GetComponent<UnitMovement>());
                        Destroy(unitTransform.gameObject);
                        Destroy(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }

                SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recruits);

                ReferencesManager.Instance.countryManager.UpdateValuesUI();
                ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();

                ReferencesManager.Instance.regionUI.UpdateDivisionUnitsIDs(unitMovement);

                ReferencesManager.Instance.regionUI.UpdateUnitsUI(true);
            }
        }
    }

    [PunRPC]
    private void RPC_AddUnitToArmy(string unitName, int regionId)
    {
        UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;

        if (unitName == "INF_01")
        {
            unit = ReferencesManager.Instance.gameSettings.soldierLVL1;
        }
        else if (unitName == "INF_02")
        {
            unit = ReferencesManager.Instance.gameSettings.soldierLVL2;
        }
        else if (unitName == "INF_03")
        {
            unit = ReferencesManager.Instance.gameSettings.soldierLVL3;
        }
        else if (unitName == "ART_01")
        {
            unit = ReferencesManager.Instance.gameSettings.artileryLVL1;
        }
        else if (unitName == "ART_02")
        {
            unit = ReferencesManager.Instance.gameSettings.artileryLVL2;
        }
        else if (unitName == "HVY_01")
        {
            unit = ReferencesManager.Instance.gameSettings.tankLVL1;
        }
        else if (unitName == "HVY_02")
        {
            unit = ReferencesManager.Instance.gameSettings.tankLVL2;
        }
        else if (unitName == "MIF_01")
        {
            unit = ReferencesManager.Instance.gameSettings.motoLVL1;
        }
        else if (unitName == "MIF_02")
        {
            unit = ReferencesManager.Instance.gameSettings.motoLVL2;
        }


        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            if (ReferencesManager.Instance.countryManager.regions[i]._id == regionId)
            {
                RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

                UnitMovement unitMovement = region.transform.Find("Unit(Clone)").GetComponent<UnitMovement>();

                if (unitMovement.unitsHealth.Count < 10)
                {
                    if (region.currentCountry.money >= unit.moneyCost &&
                        region.currentCountry.recruits >= unit.recrootsCost &&
                        region.currentCountry.food >= unit.foodCost)
                    {
                        region.currentCountry.money -= unit.moneyCost;
                        region.currentCountry.food -= unit.foodCost;
                        region.currentCountry.recruits -= unit.recrootsCost;
                        region.currentCountry.moneyNaturalIncome -= unit.moneyIncomeCost;
                        region.currentCountry.foodNaturalIncome -= unit.foodIncomeCost;

                        UnitHealth newUnitHealth = new UnitHealth();
                        newUnitHealth.unit = unit;
                        newUnitHealth.health = unit.health;
                        if (unitMovement.unitsHealth.Count > 0)
                        {
                            newUnitHealth._id = unitMovement.unitsHealth[unitMovement.unitsHealth.Count - 1]._id + 1;
                        }
                        else
                        {
                            newUnitHealth._id = 0;
                        }

                        unitMovement.unitsHealth.Add(newUnitHealth);
                    }
                }

                SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recruits);
            }
        }
    }

    [PunRPC]
    private void RPC_CreateUnit(int regionId)
    {
        UnitScriptableObject unit = ReferencesManager.Instance.gameSettings.soldierLVL1;

        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            if (ReferencesManager.Instance.countryManager.regions[i]._id == regionId)
            {
                RegionManager region = ReferencesManager.Instance.countryManager.regions[i];

                if (region.currentCountry.money >= unit.moneyCost && region.currentCountry.recruits >= unit.recrootsCost)
                {
                    GameObject spawnedUnit = Instantiate(ReferencesManager.Instance.army.unitPrefab, region.transform);

                    spawnedUnit.transform.localScale = new Vector3(
                        ReferencesManager.Instance.army.unitPrefab.transform.localScale.x,
                        ReferencesManager.Instance.army.unitPrefab.transform.localScale.y);

                    spawnedUnit.GetComponent<UnitMovement>().currentCountry = region.currentCountry;
                    spawnedUnit.GetComponent<UnitMovement>().currentProvince = region;
                    spawnedUnit.GetComponent<UnitMovement>().UpdateInfo();
                    region.hasArmy = true;

                    region.currentCountry.countryUnits.Add(spawnedUnit.GetComponent<UnitMovement>());

                    AddUnitToArmy(unit.unitName, regionId);
                }

                region.CheckRegionUnits(region);

                SetCountryValues(
                    region.currentCountry.country._id,
                    region.currentCountry.money,
                    region.currentCountry.food,
                    region.currentCountry.recruits);
            }
        }
    }

    [PunRPC]
    private void RPC_MoveUnit(int fromRegionId, int toRegionId)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.regions.Count; i++)
        {
            if (ReferencesManager.Instance.countryManager.regions[i]._id == fromRegionId)
            {
                fromRegion = ReferencesManager.Instance.countryManager.regions[i];
            }
            if (ReferencesManager.Instance.countryManager.regions[i]._id == toRegionId)
            {
                toRegion = ReferencesManager.Instance.countryManager.regions[i];
            }
        }

        fromRegion.hasArmy = false;
        fromRegion.CheckRegionUnits(fromRegion);

        Transform unitTransform = fromRegion.transform.Find("Unit(Clone)");

        unitTransform.position = toRegion.transform.position;

        unitTransform.SetParent(toRegion.transform);

        UnitMovement division = fromRegion.GetDivision(fromRegion);
        division.UpdateInfo();

        toRegion.hasArmy = true;
        toRegion.CheckRegionUnits(toRegion);
    }

    [PunRPC]
    private void RPC_DisbandDivision(int regionId, int ownerId)
    {
        RegionManager _region = new();
        CountrySettings _owner = countryManager.FindCountryByID(ownerId);

        foreach (RegionManager region in countryManager.regions)
        {
            if (region._id == regionId)
            {
                _region = region;
            }
        }

        foreach (Transform child in _region.transform)
        {
            if (child.GetComponent<UnitMovement>())
            {
                UnitMovement division = child.GetComponent<UnitMovement>();

                for (int i = 0; i < division.unitsHealth.Count; i++)
                {
                    _owner.recruits += Mathf.CeilToInt(division.unitsHealth[i].unit.recrootsCost * 0.7f);
                    _owner.moneyNaturalIncome += division.unitsHealth[i].unit.moneyIncomeCost;
                    _owner.foodNaturalIncome += division.unitsHealth[i].unit.foodIncomeCost;
                }

                division.unitsHealth.Clear();
            }
        }

        ReferencesManager.Instance.regionManager.CheckRegionUnits(ReferencesManager.Instance.regionManager.currentRegionManager);
    }

    #endregion

    #region Diplomaty

    public void SendOffer(int senderId, int receiverId, string offer)
    {
        _photonView.RPC("RPC_SendOffer", RpcTarget.All, senderId, receiverId, offer);
    }

    public void AcceptOffer(int senderId, int receiverId, string offer)
    {
        _photonView.RPC("RPC_AcceptOffer", RpcTarget.All, senderId, receiverId, offer);
    }

    [PunRPC]
    private void RPC_SendOffer(int senderId, int receiverId, string offer)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            int index = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerCountryId"];

            if (ReferencesManager.Instance.countryManager.countries[i].country._id == senderId)
            {
                sender = ReferencesManager.Instance.countryManager.countries[i];
            }
            if (ReferencesManager.Instance.countryManager.countries[i].country._id == receiverId)
            {
                receiver = ReferencesManager.Instance.countryManager.countries[i];
            }

            if (receiver == ReferencesManager.Instance.countryManager.countries[index])
            {
                // Sent to me

                GameObject spawned = Instantiate(ReferencesManager.Instance.regionUI.messageEvent);
                spawned.transform.SetParent(ReferencesManager.Instance.regionUI.messageReceiver);

                spawned.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                spawned.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);


                spawned.GetComponent<EventItem>().sender = sender;
                spawned.GetComponent<EventItem>().receiver = receiver;
                spawned.GetComponent<EventItem>().offer = offer;

                spawned.GetComponent<EventItem>().senderImage.sprite = sender.country.countryFlag;

                if (offer == "Объявить войну")
                {
                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.warSprite;

                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.war = true;
                    senderToReceiver.trade = false;
                    senderToReceiver.right = false;
                    senderToReceiver.pact = false;
                    senderToReceiver.union = false;

                    if (sender.myRegions.Count > 0 && receiver.myRegions.Count > 0)
                    {
                        sender.stability.buffs.Add(new Stability_buff("Наступательная война", (-15 * (receiver.myRegions.Count / sender.myRegions.Count)) * (1 / receiver.enemies.Count), new List<string>() { $"not;ongoing_war;{sender.country._id}" }, null, ReferencesManager.Instance.sprites.Find("offensive_war")));
                        receiver.stability.buffs.Add(new Stability_buff("Оборонительная война", -5f, new List<string>() { $"not;ongoing_war;{receiver.country._id}" }, null, ReferencesManager.Instance.sprites.Find("defensive_war")));
                    }
                    sender.enemies.Add(receiver);
                    receiver.enemies.Add(sender);

                    sender.inWar = true;
                    receiver.inWar = true;

                    receiverToSender.war = true;
                    receiverToSender.trade = false;
                    receiverToSender.right = false;
                    receiverToSender.pact = false;
                    receiverToSender.union = false;

                    senderToReceiver.relationship -= 100;
                    receiverToSender.relationship -= 100;

                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.warSprite;
                }

                else if (offer == "Заключить мир")
                {
                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.war = false;

                    sender.enemies.Remove(receiver);
                    receiver.enemies.Remove(receiver);

                    sender.inWar = false;
                    receiver.inWar = false;

                    receiverToSender.war = false;

                    senderToReceiver.relationship += 78;
                    receiverToSender.relationship += 78;

                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.warSprite;
                }

                else if (offer == "Торговля")
                {
                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.tradeSprite;
                }

                else if (offer == "Прекратить торговлю")
                {
                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.trade = false;
                    receiverToSender.trade = false;

                    senderToReceiver.relationship -= 12;
                    receiverToSender.relationship -= 12;

                    for (int v = 0; v < ReferencesManager.Instance.diplomatyUI.globalTrades.Count; v++)
                    {
                        if (ReferencesManager.Instance.diplomatyUI.globalTrades[v].sender == receiver && ReferencesManager.Instance.diplomatyUI.globalTrades[v].receiver == sender)
                        {
                            TradeBuff trade = ReferencesManager.Instance.diplomatyUI.globalTrades[v];

                            sender.moneyTradeIncome -= trade.senderMoneyTrade;
                            sender.foodTradeIncome -= trade.senderFoodTrade;

                            receiver.moneyTradeIncome -= trade.receiverMoneyTrade;
                            receiver.foodTradeIncome -= trade.receiverFoodTradee;

                            ReferencesManager.Instance.diplomatyUI.globalTrades.Remove(trade);
                        }
                    }
                }

                else if (offer == "Расторгнуть право прохода войск")
                {
                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.right = true;
                    receiverToSender.right = true;

                    senderToReceiver.relationship -= 6;
                    receiverToSender.relationship -= 6;
                }

                else if (offer == "Пакт о ненападении")
                {
                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.pactSprite;
                }

                else if (offer == "Расторгнуть пакт о ненападении")
                {
                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.pact = false;
                    receiverToSender.pact = false;

                    senderToReceiver.relationship -= 18;
                    receiverToSender.relationship -= 18;
                }

                else if (offer == "Право прохода войск")
                {
                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.moveSprite;
                }

                else if (offer == "Союз")
                {
                    spawned.GetComponent<EventItem>().offerImage.sprite = ReferencesManager.Instance.regionUI.unionSprite;
                }

                else if (offer == "Расторгнуть союз")
                {
                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.union = false;
                    receiverToSender.union = false;

                    senderToReceiver.relationship -= 60;
                    receiverToSender.relationship -= 60;
                }

                else if (offer == "Освободить вассала")
                {
                    Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
                    Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

                    senderToReceiver.vassal = false;
                    receiverToSender.vassal = false;

                    senderToReceiver.relationship += 60;
                    receiverToSender.relationship += 60;
                }


                break;
            }
        }
    }

    [PunRPC]
    private void RPC_AcceptOffer(int senderId, int receiverId, string offer)
    {
        for (int i = 0; i < ReferencesManager.Instance.countryManager.countries.Count; i++)
        {
            if (ReferencesManager.Instance.countryManager.countries[i].country._id == senderId)
            {
                sender = ReferencesManager.Instance.countryManager.countries[i];
            }
            if (ReferencesManager.Instance.countryManager.countries[i].country._id == receiverId)
            {
                receiver = ReferencesManager.Instance.countryManager.countries[i];
            }
        }

        if (offer == "Торговля")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.trade = true;
            receiverToSender.trade = true;

            senderToReceiver.relationship += 12;
            receiverToSender.relationship += 12;

            ReferencesManager.Instance.CalculateTradeBuff(sender, receiver);
        }

        else if (offer == "Право прохода войск")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.right = true;
            receiverToSender.right = true;

            senderToReceiver.relationship += 18;
            receiverToSender.relationship += 18;
        }

        else if (offer == "Пакт о ненападении")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.pact = true;
            receiverToSender.pact = true;

            senderToReceiver.relationship += 18;
            receiverToSender.relationship += 18;
        }

        else if (offer == "Союз")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.union = true;
            receiverToSender.union = true;

            senderToReceiver.relationship += 60;
            receiverToSender.relationship += 60;
        }

        else if (offer == "Сделать вассалом")
        {
            Relationships.Relation senderToReceiver = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(sender, receiver);
            Relationships.Relation receiverToSender = ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(receiver, sender);

            senderToReceiver.vassal = true;
            receiverToSender.vassal = false;

            senderToReceiver.relationship += 20;
            receiverToSender.relationship += 20;
        }

        SetCountryValues(
            sender.country._id,
            sender.money,
            sender.food,
            sender.recruits);

        SetCountryValues(
            receiver.country._id,
            receiver.money,
            receiver.food,
            receiver.recruits);
    }

    #endregion

    public void MainProcess()
    {
        _photonView.RPC("RPC_MainProcess", RpcTarget.All);
    }

    public void SetPlayerReadyState(string nickname, bool state)
    {
        _photonView.RPC("RPC_SetPlayerReadyState", RpcTarget.All, nickname, state);
    }

    public void SetReadyPlayersCount(int count)
    {
        _photonView.RPC("RPC_SetReadyPlayersCount", RpcTarget.All, count);
    }

    [PunRPC]
    private void RPC_MainProcess()
    {
        ReferencesManager.Instance.aiManager.progressManager.NextProgress();
    }

    [PunRPC]
    private void RPC_SetPlayerReadyState(string nickname, bool state)
    {
        roomPlayers.Find(x => x.currentNickname == nickname).readyToMove = state;
    }

    [PunRPC]
    private void RPC_SetReadyPlayersCount(int count)
    {
        m_ReadyPlayers = count;
    }
}
