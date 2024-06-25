using UnityEngine;
using UnityEngine.UI;

public class FillCountryFlag : MonoBehaviour
{
    public Image flag;
    public bool InDiplomatyUI;
    public CountryScriptableObject country;

    public DiplomatyUI diplomatyUI;

    public void FillInfo()
    {
        flag.sprite = country.countryFlag;
    }

    public void Action()
    {
        if (InDiplomatyUI)
        {
            diplomatyUI = FindObjectOfType<DiplomatyUI>();
            diplomatyUI.regionUI.CloseAllUI();

            diplomatyUI.OpenUINoClick(country._id);
        }
    }
}
