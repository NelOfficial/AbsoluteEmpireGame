using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Aviation_Cell_obj : MonoBehaviour
{
    public Image icon;
    public Slider HP_slider;
    public Slider FuelSlider;
    public TMP_Text text;
    public Aviation_Cell MyObject;
    public Transform parent;

    [SerializeField] private Image _background;

    [SerializeField] private GameObject _disbandButton;
    public bool _interactable;

    public void SetUp(Aviation_Cell obj)
    {
        MyObject = obj;
        icon.sprite = obj.AirPlane.sprite;
        HP_slider.value = obj.hp / obj.AirPlane.maxhp;
        FuelSlider.value = obj.fuel / obj.AirPlane.fuelMax;
        text.text = obj.AirPlane.name;

        this.GetComponent<Button>().interactable = _interactable;

        if (!_interactable)
        {
            _disbandButton.gameObject.SetActive(false);
        }
    }

    public void Select()
    {
        Aviation_UI.SelectedPlane = gameObject;

        Aviation_Cell_obj[] aviation_Cells = FindObjectsOfType<Aviation_Cell_obj>();

        for (int i = 0; i < aviation_Cells.Length; i++)
        {
            aviation_Cells[i]._background.color = Color.gray;
        }

        this._background.color = Color.green;

        ReferencesManager.Instance.aviationUI.UpdateActionsButtons();
    }

    public void Disband()
    {
        Aviation_Storage airport = ReferencesManager.Instance.regionManager.currentRegionManager.GetComponent<Aviation_Storage>();

        for (int i = 0; i < airport.planes.Count; i++)
        {
            if (airport.planes[i].AirPlane == MyObject.AirPlane && airport.planes[i].hp == MyObject.hp)
            {
                airport.planes.Remove(airport.planes[i]);
                break;
            }
        }

        ReferencesManager.Instance.aviationUI.UpdateMainUI();
    }
}
