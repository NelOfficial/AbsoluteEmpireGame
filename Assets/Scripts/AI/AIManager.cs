using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AIManager : MonoBehaviour
{
    public List<CountrySettings> AICountries = new List<CountrySettings>();
    private CountrySettings currentAICountry;

    public ProgressManager progressManager;

    public GameObject losePanel;

    [SerializeField] float delay;

	public void OnNextMove()
	{
		progressManager.countryMovePanel.SetActive(true);
		foreach (CountrySettings countrySettings in this.AICountries)
		{
			progressManager.countryMoveName.text = countrySettings.country._name;
			progressManager.countryMoveImage.sprite = countrySettings.country.countryFlag;
			if (countrySettings.gameObject.GetComponent<CountryAIManager>())
			{
				countrySettings.GetComponent<CountryAIManager>().gameObject.GetComponent<CountryAIManager>().Process(countrySettings);
				if (countrySettings.GetComponent<CountryAIManager>().currentTech != null && countrySettings.GetComponent<CountryAIManager>().currentTech.tech != null)
				{
					if (countrySettings.GetComponent<CountryAIManager>().currentTech != null && countrySettings.GetComponent<CountryAIManager>().currentTech.tech != null)
					{
						countrySettings.GetComponent<CountryAIManager>().currentTech.moves--;
					}
					if (countrySettings.GetComponent<CountryAIManager>().currentTech.moves <= 0)
					{
						if (ReferencesManager.Instance.gameSettings.onlineGame)
						{
							for (int i = 0; i < countrySettings.countryTechnologies.Count; i++)
							{
								if (countrySettings.countryTechnologies[i] == countrySettings.GetComponent<CountryAIManager>().currentTech.tech)
								{
									Multiplayer.Instance.AddTechnology(countrySettings.country._id, i);
								}
							}
						}
						else
						{
							countrySettings.countryTechnologies.Add(countrySettings.GetComponent<CountryAIManager>().currentTech.tech);
							countrySettings.GetComponent<CountryAIManager>().currentTech = null;
							countrySettings.GetComponent<CountryAIManager>().researching = false;
						}
					}
				}
			}
			countrySettings.UpdateCapitulation();
		}
		progressManager.countryMovePanel.SetActive(false);
	}

	public void DisableAI()
	{
		CountrySettings _countryToDisable = ReferencesManager.Instance.diplomatyUI.receiver;

		if (_countryToDisable != null)
		{
			_countryToDisable.isPlayer = false;

			AICountries.Remove(_countryToDisable);

			Destroy(_countryToDisable.GetComponent<CountryAIManager>());
        }
    }

    public void EnableAI()
    {
        CountrySettings _countryToEnable = ReferencesManager.Instance.diplomatyUI.receiver;

        if (_countryToEnable != null)
        {
            _countryToEnable.isPlayer = false;

			_countryToEnable.AddComponent<CountryAIManager>();

            AICountries.Add(_countryToEnable);
        }
    }
}
