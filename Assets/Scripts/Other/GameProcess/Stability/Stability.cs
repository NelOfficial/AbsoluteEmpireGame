using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using R = ReferencesManager;

[System.Serializable]
public class Stability
{
	public CountrySettings country;
	public float value
	{
		get { return Count(); }
		private set { return; }
	}
	public float base_value = 0;
	public List<Stability_buff> buffs = new(5);	

	public Stability(CountrySettings country)
	{
		this.country = country;
		if (country.isPlayer)
		{
			switch (R.Instance.gameSettings.difficultyValue.value)
			{
				case "EASY":
					base_value += 10f;
					break;
				case "NORMAL":
					base_value += 0f;
					break;
				case "HARD":
					base_value -= 5f;
					break;
				case "INSANE":
					base_value -= 10f;
					break;
				case "HARDCORE":
					base_value -= 15f;
					buffs.Add(new Stability_buff("Я вижу ты хардкорщик...", -10f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}" }, null, R.Instance.sprites.Find("Soldier_1lvl")));
					break;
			}
		}
		if (country.ideology == "Демократия")
		{
			buffs.Add(new Stability_buff("Бюрократические издержки", -5f, new List<string>() { $"date_before;{R.Instance.dateManager.currentDate[0]}-{R.Instance.dateManager.currentDate[1]}-{R.Instance.dateManager.currentDate[2] + 1}"}, null, R.Instance.sprites.Find("bureaucratic_costs")));

			buffs.Add(new Stability_buff("Свобода слова", 10f,new List<string>() { $"not;is_ideology;{country.country._id};d" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{country.country._id}" }, R.Instance.sprites.Find("freedom_of_speech")));
		}
		if (country.ideology == "Фашизм")
		{
			buffs.Add(new Stability_buff("Сила нации", 15f, new List<string>() { $"not;is_ideology;{country.country._id};f" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{country.country._id}" }, R.Instance.sprites.Find("power_of_nation")));
		}
		if (country.ideology == "Коммунизм")
		{
			buffs.Add(new Stability_buff("Плановая экономика", 7.5f, new List<string>() { $"not;is_ideology;{country.country._id};c" }, new List<string>() { $"DEV_UPD_IDEOLOGY_STABILITY_BUFFS;{country.country._id}" }, R.Instance.sprites.Find("planned_economy")));
		}
		if (country.ideology == "Неопределено")
		{

		}
        base_value = new System.Random().Next(50, 70);
    }

	public float Count()
	{
		float result = base_value;
		foreach (Stability_buff buff in buffs)
		{
			result += buff.value;
		}
		return result <= 0f ? 0.001f : result > 100f ? 100f : result;
	}

	public void Check()
	{
		List<Stability_buff> this_buffs = new(buffs);

		for (int i = 0; i < this_buffs.Count; i++)
		{
            if (this_buffs[i].Check())
            {
				buffs.Remove(this_buffs[i]);

                if (this_buffs[i] != null && this_buffs[i].actions != null)
                {
                    foreach (string act in this_buffs[i].actions)
                    {
						if (act != null && !act.IsNullOrEmpty())
						{
							R.Instance.eventUI.ExecuteEvent(act);
						}
						else
						{
							Debug.LogError($"Stability Actions is null: {this_buffs[i]}");
                        }
                    }
                }
            }
        }
	}
}
[System.Serializable]
public class Stability_buff
{
	public Sprite icon;
	public string _name;
	public float value;
	public List<string> conditions;
    public List<string> actions = new(5);

    public Stability_buff(string name, float value, List<string> conditions, List<string> actions, Sprite sprite)
	{
		_name = name;
		this.value = value;
		this.conditions = conditions;
		this.actions = actions;
		this.icon = sprite;
	}

	public bool Check()
	{	
		foreach (string cond in conditions)
		{			
			if (!R.Instance.eventUI.CheckConditions(cond))
			{
				return false;
			}
		}
		return true;
	}
}