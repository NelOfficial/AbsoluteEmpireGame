using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingsManager : MonoBehaviour
{
    [SerializeField] private GameObject View;
    [SerializeField] private GameObject Cell;
    [SerializeField] private GameObject Arrow;
    [SerializeField] private TMP_Text CountText;
    [SerializeField] private GameObject BuildType;

    [SerializeField] private BuildingScriptableObject[] Buildobjs;
    private int pointer = 0;

    public int mode = 0;

    [SerializeField] private TMP_Text factoriesText;
    [SerializeField] private TMP_Text farmsText;
    [SerializeField] private TMP_Text chefarmsText;
    [SerializeField] private TMP_Text reslabsText;
    [SerializeField] private TMP_Text docksText;

    [SerializeField] private TMP_Text factoriesIncome;
    [SerializeField] private TMP_Text farmsIncome;
    [SerializeField] private TMP_Text chefarmsIncome;
    [SerializeField] private TMP_Text reslabsIncome;
    [SerializeField] private TMP_Text docksIncome;

    private int factoryIncome = 0;
    private int farmIncome = 0;
    private int chefarmIncome = 0;
    private int reslabIncome = 0;
    private int dockIncome = 0;

    private int factory = 0;
    private int farm = 0;
    private int chefarm = 0;
    private int reslab = 0;
    private int dock = 0;

    public void ToggleUi(bool modechange)
    {

        if (modechange)
        {
            if (mode == 0)
            {
                mode = 1;
            }
            else if (mode == 1)
            {
                mode = 0;
            }
        }

        foreach (Transform child in View.transform)
        {
            Destroy(child.gameObject);
        }

        CountrySettings country = ReferencesManager.Instance.countryManager.currentCountry;

        List<RegionManager.BuildingQueueItem> unsortlist = new List<RegionManager.BuildingQueueItem>();
        List<RegionManager.BuildingQueueItem> sortlist = new List<RegionManager.BuildingQueueItem>();

        factoryIncome = 0;
        farmIncome = 0;
        chefarmIncome = 0;
        reslabIncome = 0;
        dockIncome = 0;

        foreach (RegionManager region in country.myRegions)
        {
            foreach (RegionManager.BuildingQueueItem build in region.buildingsQueue)
            {
                unsortlist.Add(build);
                if (build.building._name == "CFR")
                {
                    factoryIncome++;
                }
                else if (build.building._name == "FAR")
                {
                    farmIncome++;
                }
                else if (build.building._name == "CHF")
                {
                    chefarmIncome++;
                }
                else if (build.building._name == "REL")
                {
                    reslabIncome++;
                }
                else if (build.building._name == "DOC")
                {
                    dockIncome++;
                }
            }
        }

        factory = country.civFactories;
        farm = country.farms;
        chefarm = country.chemicalFarms;
        reslab = country.researchLabs;
        dock = country.dockyards;

        // очень неоптимизированный метод сортировки, думаю его стоит преписать но я не умею :/
        if (mode == 0)
        {
            Arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
            while (unsortlist.Count != 0)
            {
                RegionManager.BuildingQueueItem maxvalue = unsortlist[0];
                foreach (RegionManager.BuildingQueueItem build in unsortlist)
                {
                    if ((build.building.moves - build.movesLasts) / build.building.moves > (maxvalue.building.moves - maxvalue.movesLasts) / maxvalue.building.moves)
                    {
                        maxvalue = build;
                    }
                }
                sortlist.Add(maxvalue);
                unsortlist.Remove(maxvalue);
            }
        }
        else
        {
            Arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
            while (unsortlist.Count != 0)
            {
                RegionManager.BuildingQueueItem minvalue = unsortlist[0];
                foreach (RegionManager.BuildingQueueItem build in unsortlist)
                {
                    if ((build.building.moves - build.movesLasts) / build.building.moves < (minvalue.building.moves - minvalue.movesLasts) / minvalue.building.moves)
                    {
                        minvalue = build;
                    }
                }
                sortlist.Add(minvalue);
                unsortlist.Remove(minvalue);
            }
        }
        foreach (RegionManager.BuildingQueueItem build in sortlist)
        {
            GameObject obj = Instantiate(Cell, View.transform);
            obj.GetComponent<BuildingsObj>().buildData = build;
            obj.GetComponent<BuildingsObj>().SetUp();
        }

        factoriesText.text = factory.ToString();
        factoriesIncome.text = $"+{factoryIncome}";

        farmsText.text = farm.ToString();
        farmsIncome.text = $"+{farmIncome}";

        chefarmsText.text = chefarm.ToString();
        chefarmsIncome.text = $"+{chefarmIncome}";

        reslabsText.text = reslab.ToString();
        reslabsIncome.text = $"+{reslabIncome}";

        docksText.text = dock.ToString();
        docksIncome.text = $"+{dockIncome}";

        CountText.text = "0";
        gameObject.SetActive(true);

        ReferencesManager.Instance.countryManager.UpdateValuesUI();
        ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }

    public void AddToText(int num)
    {
        CountText.text = (int.Parse(CountText.text) + num).ToString();
        if (int.Parse(CountText.text) < 0) {
            CountText.text = "0";
        }
    }

    public void AddToPoint(int num)
    {
        pointer += num;
        if (pointer < 0) 
        {
            pointer = Buildobjs.Length - 1; 
        }
        else if (pointer >= Buildobjs.Length)
        {
            pointer = 0;
        }
        BuildType.GetComponent<Image>().sprite = Buildobjs[pointer].icon;
    }

    public void Build()
    {
        CountrySettings currentCountry = ReferencesManager.Instance.countryManager.currentCountry;
        if (Buildobjs[pointer].goldCost * int.Parse(CountText.text) > currentCountry.money)
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NotEnoughtResources"));
            return;
        }
        int counter = int.Parse(CountText.text);
        
        int buildcount = 0;
        if (counter <= 0)
        {
            return;
        }
        foreach (RegionManager reg in currentCountry.myRegions)
        {
            int slots = 4 - reg.buildings.Count - reg.buildingsQueue.Count;
            if (slots > 0)
            {
                while (slots > 0)
                {
                    RegionManager.BuildingQueueItem item = new RegionManager.BuildingQueueItem();
                    item.building = Buildobjs[pointer];
                    item.movesLasts = Buildobjs[pointer].moves;
                    item.region = reg;

                    reg.buildingsQueue.Add(item);
                    slots--;
                    counter--;
                    buildcount++;
                    currentCountry.money -= Buildobjs[pointer].goldCost;
                    if (counter <= 0)
                    {
                        break;
                    }
                }
                if (counter <= 0)
                {
                    break;
                }
            }

        }
        ToggleUi(false);
        CountText.text = "0";

        WarningManager.Instance.Warn($"{ReferencesManager.Instance.languageManager.GetTranslation("BuildingManager.WasAdded_1")} {buildcount} {ReferencesManager.Instance.languageManager.GetTranslation("BuildingManager.WasAdded_2")}");
    }
}
