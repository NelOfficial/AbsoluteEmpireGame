using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingsObj : MonoBehaviour
{
    public Image icon;
    public TMP_Text title;
    public Slider slider;
    public GameObject button;

    public float buildvalue;

    public RegionManager.BuildingQueueItem buildData;

    public void SetUp()
    {
        icon.sprite = buildData.building.icon;
        title.text = ReferencesManager.Instance.languageManager.GetTranslation($"Buildings.{buildData.building.uiTitleEN}");
        buildvalue = (buildData.building.moves - buildData.movesLasts) / buildData.building.moves;

        slider.value = buildvalue;
    }

    public void Remove()
    {
        buildData.region.buildingsQueue.Remove(buildData);
        Destroy(gameObject);
    }
}
