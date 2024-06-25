using TMPro;
using UnityEngine;

public class CityTranslate : MonoBehaviour
{

    private void Awake()
    {
        var languageManager = ReferencesManager.Instance.languageManager;

        for (int i = 0; i < transform.childCount; i++)
        {
            var cityTransform = transform.GetChild(i).GetChild(1);
            var txt = cityTransform.GetComponent<TMP_Text>();

            if (txt == null)
            {
                continue;
            }

            var cityComponent = cityTransform.GetComponent<City>();
            string translationKey = cityComponent != null ? cityComponent.GetKey() : txt.text.Trim(' ');

            txt.text = languageManager.GetTranslation(translationKey);
        }
    }

}
