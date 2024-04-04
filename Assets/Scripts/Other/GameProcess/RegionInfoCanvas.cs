using UnityEngine;
using TMPro;

public class RegionInfoCanvas : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TMP_Text[] texts;

    public void UpdateSize()
    {
        float newScale = Camera.main.orthographicSize * 1.5f / 5;
        rectTransform.localScale = new Vector3(newScale, newScale);
    }

    public void UpdateUI(RegionManager region)
    {
        if (region.civFactory_Amount > 0)
        {
            texts[0].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        if (region.farms_Amount > 0)
        {
            texts[1].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        if (region.infrastructure_Amount > 0)
        {
            texts[2].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        if (region.cheFarms > 0)
        {
            texts[3].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        texts[0].text = $"{region.civFactory_Amount}";
        texts[1].text = $"{region.farms_Amount}";
        texts[2].text = $"{region.infrastructure_Amount}/10";
        texts[3].text = $"{region.cheFarms}";
    }
}
