using UnityEngine;
using UnityEngine.UI;

public class IdeologyFlagFill : MonoBehaviour
{
    [SerializeField] Image flagImage;
    [SerializeField] int ideologyId;


    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        flagImage.sprite = ReferencesManager.Instance.countryManager.currentCountry.country.countryIdeologyFlags[ideologyId];
    }
}
