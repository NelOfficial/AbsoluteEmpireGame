using UnityEngine;
using TMPro;

public class SmallNewsManager : MonoBehaviour
{
    public static SmallNewsManager Instance;
    [SerializeField] TMP_Text headerText_TMP;
    [SerializeField] TMP_Text messageText_TMP;
    [SerializeField] FillCountryFlag countryFlag;

    [HideInInspector] public CountryScriptableObject countrySender;
    [HideInInspector] public string message;

    [SerializeField] GameObject container;


    private void Awake()
    {
        Instance = this;
    }

    public void UpdateUI()
    {
        container.SetActive(true);

        if (PlayerPrefs.GetInt("languageId") == 0)
        {
            headerText_TMP.text = $"Message from state: {countrySender._nameEN}";
        }
        else if (PlayerPrefs.GetInt("languageId") == 1)
        {
            headerText_TMP.text = $"Послание от государства: {countrySender._name}";
        }

        countryFlag.country = countrySender;
        countryFlag.FillInfo();

        messageText_TMP.text = message;
    }
}
