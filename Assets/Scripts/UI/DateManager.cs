using UnityEngine;
using TMPro;

public class DateManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text dateText;

    public int[] currentDate;

    private GameSettings gameSettings;

    [SerializeField]
    private GameEventUI gameEventUI;

	private void Awake()
	{
		this.gameSettings = Object.FindObjectOfType<GameSettings>();
		this.UpdateUI();
		this.CheckGameEvents();
		for (int i = 0; i < this.gameSettings.gameEvents.Count; i++)
		{
			this.gameSettings.gameEvents[i]._checked = false;
		}
	}

	public void UpdateUI()
	{
		int _localYear = this.currentDate[2];
		int _localMonth = this.currentDate[1];
		int _localDay = this.currentDate[0];

		if (_localMonth == 1)
		{
			if (_localDay > 31)
			{
				_localDay -= 31;
				_localMonth++;
			}
		}
		else if (_localMonth == 2)
		{
			if (_localYear % 4 == 0 && _localYear % 100 == 0 && _localYear % 400 == 0)
			{
				if (_localDay > 29)
				{
					_localDay -= 29;
					_localMonth++;
				}
			}
			else if (_localDay > 28)
			{
				_localDay -= 28;
				_localMonth++;
			}
		}
		else if (_localMonth == 3)
		{
			if (_localDay > 31)
			{
				_localDay -= 31;
				_localMonth++;
			}
		}
		else if (_localMonth == 4)
		{
			if (_localDay > 30)
			{
				_localDay -= 30;
				_localMonth++;
			}
		}
		else if (_localMonth == 5)
		{
			if (_localDay > 31)
			{
				_localDay -= 31;
				_localMonth++;
			}
		}
		else if (_localMonth == 6)
		{
			if (_localDay > 30)
			{
				_localDay -= 30;
				_localMonth++;
			}
		}
		else if (_localMonth == 7)
		{
			if (_localDay > 31)
			{
				_localDay -= 31;
				_localMonth++;
			}
		}
		else if (_localMonth == 8)
		{
			if (_localDay > 31)
			{
				_localDay -= 31;
				_localMonth++;
			}
		}
		else if (_localMonth == 9)
		{
			if (_localDay > 30)
			{
				_localDay -= 30;
				_localMonth++;
			}
		}
		else if (_localMonth == 10)
		{
			if (_localDay > 31)
			{
				_localDay -= 31;
				_localMonth++;
			}
		}
		else if (_localMonth == 11)
		{
			if (_localDay > 30)
			{
				_localDay -= 30;
				_localMonth++;
			}
		}
		else if (_localMonth == 12 && _localDay > 31)
		{
			_localDay -= 31;
			_localMonth = 1;
			_localYear++;
		}
		string arg = this.gameSettings.monthsDisplay[_localMonth];
		this.dateText.text = string.Format("{0} {1} {2}", _localDay, arg, _localYear);
		this.currentDate[2] = _localYear;
		this.currentDate[1] = _localMonth;
		this.currentDate[0] = _localDay;
	}

	public void CheckGameEvents()
	{
		for (int i = 0; i < this.gameSettings.gameEvents.Count; i++)
		{
			EventScriptableObject eventScriptableObject = this.gameSettings.gameEvents[i];

            if (!ReferencesManager.Instance.gameSettings.allowGameEvents)
            {
				if (eventScriptableObject.IS_GAME_MAIN_EVENT == false)
                {
					StartEvent(eventScriptableObject);
                }
            }
            else
            {
				StartEvent(eventScriptableObject);
            }
		}
	}

	private void StartEvent(EventScriptableObject eventScriptableObject)
    {
		string[] array = eventScriptableObject.date.Split(new char[] { '-' });
		int num = int.Parse(array[2]);
		int num2 = int.Parse(array[1]);
		int num3 = int.Parse(array[0]);

		if (currentDate[0] >= num3 && num2 <= this.currentDate[1] && this.currentDate[2] >= num && !eventScriptableObject._checked)
		{
			if (eventScriptableObject.receivers.Count > 0)
			{
				if (eventScriptableObject.receivers.Contains(ReferencesManager.Instance.countryManager.currentCountry.country._id))
				{
					if (!eventScriptableObject.silentEvent)
					{
						gameEventUI.gameObject.SetActive(true);
						UISoundEffect.Instance.PlayAudio(this.gameSettings.m_new_event_01);
						UISoundEffect.Instance.PlayAudio(this.gameSettings.m_paper_01);
					}

					gameEventUI.currentGameEvent = eventScriptableObject;
					eventScriptableObject._checked = true;
					gameEventUI.UpdateUI();
				}
			}
			else
			{
				if (!eventScriptableObject.silentEvent)
				{
					gameEventUI.gameObject.SetActive(true);
					UISoundEffect.Instance.PlayAudio(this.gameSettings.m_new_event_01);
					UISoundEffect.Instance.PlayAudio(this.gameSettings.m_paper_01);
				}

				gameEventUI.currentGameEvent = eventScriptableObject;
				eventScriptableObject._checked = true;
				gameEventUI.UpdateUI();
			}
		}
	}
}
