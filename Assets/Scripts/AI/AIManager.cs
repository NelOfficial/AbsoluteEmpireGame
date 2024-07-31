using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AIManager : MonoBehaviour
{
    public List<CountrySettings> AICountries = new List<CountrySettings>();

    public ProgressManager progressManager;
    public GameObject losePanel;

    [SerializeField] private float delay;

    private int mode;

    private void Start()
    {
        if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "historic")
        {
            mode = 0;
            ReferencesManager.Instance.gameSettings.allowGameEvents = true;
        }
        else if (ReferencesManager.Instance.gameSettings._currentGameMode.value == "nonhistoric")
        {
            mode = 1;
            ReferencesManager.Instance.gameSettings.allowGameEvents = false;
        }
        else
        {
            mode = 0;
        }
    }

    public void OnNextMove()
    {
        StartCoroutine(Coroutine());
    }

    IEnumerator Coroutine()
    {
        progressManager.countryMovePanel.SetActive(true);

        for (int a = 0; a < AICountries.Count; a++)
        {
            CountrySettings countrySettings = AICountries[a];

            if (countrySettings.myRegions.Count > 0)
            {
                progressManager.countryMoveName.text = ReferencesManager.Instance.languageManager.GetTranslation(countrySettings.country._nameEN);
                progressManager.countryMoveImage.sprite = countrySettings.country.countryFlag;

                if (countrySettings.TryGetComponent<CountryAIManager>(out var aiManager))
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
                yield return new WaitForSecondsRealtime(0);
            }
        }
        progressManager.countryMovePanel.SetActive(false);

        yield break;
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
