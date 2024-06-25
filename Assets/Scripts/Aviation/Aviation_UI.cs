using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Aviation_UI : MonoBehaviour
{
    [SerializeField] GameObject Cell;
    [SerializeField] Transform View;

    private GameObject airport;
    public static GameObject SelectedPlane;

    public GameObject cancelbutton;

    public static int attackMode;

    [SerializeField] private Button[] _actionsButtons;

    public void ToggleUI()
    {
        SelectedPlane = null;
        airport = ReferencesManager.Instance.regionManager.currentRegionManager.gameObject;

        if (airport.GetComponent<Aviation_Storage>())
        {
            Aviation_Storage storage = airport.GetComponent<Aviation_Storage>();

            for (int i = View.childCount - 1; i >= 0; i--)
            {
                Destroy(View.GetChild(i).gameObject);
            }

            foreach (Aviation_Cell cell in storage.planes)
            {
                GameObject obj2 = Instantiate(Cell, View);
                obj2.GetComponent<Aviation_Cell_obj>()._interactable = true;
                obj2.GetComponent<Aviation_Cell_obj>().SetUp(cell);
            }

            gameObject.SetActive(true);
            BackgroundUI_Overlay.Instance.OpenOverlay(gameObject);
            cancelbutton.SetActive(false);

            UpdateActionsButtons();
        }
        else
        {
            WarningManager.Instance.Warn(ReferencesManager.Instance.languageManager.GetTranslation("Warn.NoAirport"));
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }

    public void Armyattack()
    {
        Color attackColor = ReferencesManager.Instance.gameSettings.redColor;

        ReferencesManager.Instance.gameSettings.regionSelectionMode = true;
        ReferencesManager.Instance.gameSettings.provincesListColor = attackColor;
        ReferencesManager.Instance.gameSettings.regionSelectionModeType = "only_enemies_army";
        ReferencesManager.Instance.gameSettings.provincesListMax = 1;
        ReferencesManager.Instance.gameSettings.provincesList.Clear();

        foreach (RegionManager reg in ReferencesManager.Instance.countryManager.regions)
        {
            reg.canSelect = false;
        }

        foreach (GameObject obj in FindObjectsWithComponent<RegionManager>(new Vector2(airport.transform.position.x, airport.transform.position.y), 0.00375f * SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject.AirPlane.distance))
        {
            RegionManager region = obj.GetComponent<RegionManager>();

            if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(region.currentCountry, ReferencesManager.Instance.countryManager.currentCountry).war)
            {
                region.CheckRegionUnits(region);

                if (region.currentCountry != ReferencesManager.Instance.countryManager.currentCountry && region.hasArmy)
                {
                    region.canSelect = true;
                    obj.GetComponent<SpriteRenderer>().color = attackColor;
                }
            }
        }

        ReferencesManager.Instance.gameSettings.provincesList.Clear();
        ReferencesManager.Instance.regionUI.barContent.SetActive(false);
        RegionManager.aviation = true;
        RegionManager.chooseCount = 1;

        cancelbutton.SetActive(true);
        gameObject.SetActive(false);
        attackMode = 0;
    }

    public void Buildattack()
    {
        Color attackColor = ReferencesManager.Instance.gameSettings.redColor;

        ReferencesManager.Instance.gameSettings.regionSelectionMode = true;
        ReferencesManager.Instance.gameSettings.regionSelectionModeType = "enemies_territories";
        ReferencesManager.Instance.gameSettings.provincesListColor = attackColor;
        ReferencesManager.Instance.gameSettings.provincesListMax = 3;
        ReferencesManager.Instance.gameSettings.provincesList.Clear();

        foreach (RegionManager reg in ReferencesManager.Instance.countryManager.regions)
        {
            reg.canSelect = false;
        }

        foreach (GameObject obj in FindObjectsWithComponent<RegionManager>(new Vector2(airport.transform.position.x, airport.transform.position.y), 0.00375f * SelectedPlane.GetComponent<Aviation_Cell_obj>().MyObject.AirPlane.distance))
        {
            RegionManager region = obj.GetComponent<RegionManager>();

            if (ReferencesManager.Instance.diplomatyUI.FindCountriesRelation(region.currentCountry, ReferencesManager.Instance.countryManager.currentCountry).war)
            {
                if (region.currentCountry != ReferencesManager.Instance.countryManager.currentCountry)
                {
                    region.canSelect = true;
                    obj.GetComponent<SpriteRenderer>().color = attackColor;
                }
            }
        }
        ReferencesManager.Instance.gameSettings.provincesList.Clear();
        ReferencesManager.Instance.regionUI.barContent.SetActive(false);
        RegionManager.aviation = true;
        RegionManager.chooseCount = 3;
        cancelbutton.SetActive(true);
        gameObject.SetActive(false);
        attackMode = 1;
    }

    List<GameObject> FindObjectsWithComponent<T>(Vector2 center, float searchRadius) where T : Component
    {
        // Найти все коллайдеры в радиусе
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, searchRadius);

        List<GameObject> foundObjects = new List<GameObject>();

        // Проверяем каждый найденный коллайдер
        foreach (Collider2D collider in hitColliders)
        {
            // Проверяем, содержит ли объект нужный компонент
            T component = collider.GetComponent<T>();
            if (component != null)
            {
                foundObjects.Add(collider.gameObject);
            }
        }

        return foundObjects;
    }

    public void cancelattack()
    {
        ReferencesManager.Instance.gameSettings.regionSelectionMode = false;
        ReferencesManager.Instance.gameSettings.provincesList.Clear();
        RegionManager.aviation = false;

        foreach (RegionManager region in ReferencesManager.Instance.countryManager.regions)
        {
            region.canSelect = true;
        }

        ReferencesManager.Instance.regionManager.UpdateRegions();

        ToggleUI();
    }

    public void Select_Plane(GameObject plane)
    {
        for (int i = 0; i < View.childCount; i++)
        {
            View.GetChild(i).GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
        plane.GetComponent<Image>().color = new Color(0.65f, 0.65f, 0.65f);
        SelectedPlane = plane;
    }

    public void UpdateActionsButtons()
    {
        bool interactable = false;

        if (SelectedPlane != null)
        {
            interactable = true;
        }
        else
        {
            interactable = false;
        }

        for (int i = 0; i < _actionsButtons.Length; i++)
        {
            _actionsButtons[i].interactable = interactable;
        }
    }
}
