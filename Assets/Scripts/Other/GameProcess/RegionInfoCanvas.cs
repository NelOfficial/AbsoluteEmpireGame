using UnityEngine;
using TMPro;

public class RegionInfoCanvas : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TMP_Text[] texts;

    [SerializeField] GameObject[] _tabs;

    public void UpdateSize()
    {
        float newScale = Camera.main.orthographicSize * 1.5f / 4.5f;
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
            texts[5].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        if (region.cheFarms > 0)
        {
            texts[2].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        if (region.researchLabs > 0)
        {
            texts[3].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        if (region.dockyards> 0)
        {
            texts[4].color = ReferencesManager.Instance.regionUI.victoryColor;
        }

        texts[0].text = $"{region.civFactory_Amount}";
        texts[1].text = $"{region.farms_Amount}";
        texts[2].text = $"{region.cheFarms}";
        texts[3].text = $"{region.researchLabs}";
        texts[4].text = $"{region.dockyards}";
        texts[5].text = $"{region.infrastructure_Amount}/10";

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].text == "0" || texts[i].text == "0/10")
            {
                _tabs[i].gameObject.SetActive(false);
            }
            else if(texts[i].text != "0" || texts[i].text != "0/10")
            {
                _tabs[i].gameObject.SetActive(true);
            }
        }
    }
}
