using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AIManager : MonoBehaviour
{
    public List<CountrySettings> AICountries = new List<CountrySettings>();

    public ProgressManager progressManager;
    public GameObject losePanel;
    [SerializeField] private float delay;
    [SerializeField] private StringValue modestr;
    private int mode;
    private void Awake()
    {
        if (modestr.value == "historic")
        {
            mode = 0;
        }
        else if (modestr.value == "nonhistoric")
        {
            mode = 1;
        }
        else
        {
            mode = 0;
        }
    }

    public void OnNextMove()
    {
        progressManager.countryMovePanel.SetActive(true);

        foreach (CountrySettings countrySettings in AICountries)
        {
            progressManager.countryMoveName.text = countrySettings.country._name;
            progressManager.countryMoveImage.sprite = countrySettings.country.countryFlag;

            var aiManager = countrySettings.GetComponent<CountryAIManager>();
            if (aiManager != null)
            {
                aiManager.Process(countrySettings, mode);

                if (aiManager.currentTech?.tech != null)
                {
                    aiManager.currentTech.moves--;

                    if (aiManager.currentTech.moves <= 0)
                    {
                        if (ReferencesManager.Instance.gameSettings.onlineGame)
                        {
                            for (int i = 0; i < countrySettings.countryTechnologies.Count; i++)
                            {
                                if (countrySettings.countryTechnologies[i] == aiManager.currentTech.tech)
                                {
                                    Multiplayer.Instance.AddTechnology(countrySettings.country._id, i);
                                }
                            }
                        }
                        else
                        {
                            countrySettings.countryTechnologies.Add(aiManager.currentTech.tech);
                            aiManager.currentTech = null;
                            aiManager.researching = false;
                        }
                    }
                }
            }

            countrySettings.UpdateCapitulation();
        }

        progressManager.countryMovePanel.SetActive(false);
    }

    

    public void DisableAI(CountrySettings countryToDisable)
    {
        if (countryToDisable != null)
        {
            AICountries.Remove(countryToDisable);
            Destroy(countryToDisable.GetComponent<CountryAIManager>());
        }
    }

    public void EnableAI(CountrySettings countryToEnable)
    {
        if (countryToEnable != null)
        {
            countryToEnable.gameObject.AddComponent<CountryAIManager>();
            AICountries.Add(countryToEnable);
        }
    }
}
