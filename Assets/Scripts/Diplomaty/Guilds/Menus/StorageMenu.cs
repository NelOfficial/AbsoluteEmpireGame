using UnityEngine;
using TMPro;
using UnityEngine.PlayerLoop;

public class StorageMenu : MonoBehaviour
{
	public Action action;

	[SerializeField] private TMP_Text header;
	[SerializeField] private TMP_Dropdown res_dropdown;
	[SerializeField] private TMP_InputField count_inputfield;

    [SerializeField] private StorageMenu2 _storageUI;

    public void ChangeAcrion(int i)
    {
        action = i == 0 ? Action.Ask : Action.Send;
    }

	public void OnEnable()
	{
		header.text = action == Action.Ask ? "Попросить" : "Пожертвовать";
	}

	public void Execute()
	{
		int count = int.Parse(count_inputfield.text);
		int res = res_dropdown.value;

        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);
        if (count <= 0)
        {
            WarningManager.Instance.Warn("Нельзя ввести отрицательное количество ресурсов лол");
            return;
        }

        if (action == Action.Ask)
		{
			if (res_dropdown.value == 0)
			{
				if (count > guild._storage.gold)
				{
                    WarningManager.Instance.Warn("В казне нет столько ресурсов!");
                    return;
                }
			}
			else if (res_dropdown.value == 1)
			{
                if (count > guild._storage.food)
                {
                    WarningManager.Instance.Warn("В казне нет столько ресурсов!");
                    return;
                }
            }
			else if (res_dropdown.value == 2)
			{
                if (count > guild._storage.recruits)
                {
                    WarningManager.Instance.Warn("В казне нет столько ресурсов!");
                    return;
                }
            }
			else if (res_dropdown.value == 3)
			{
                if (count > guild._storage.fuel)
                {
                    WarningManager.Instance.Warn("В казне нет столько ресурсов!");
                    return;
                }
            }			
			guild._offers.Add(new Guild.Offer
			(
				guild,
				ReferencesManager.Instance.countryManager.currentCountry,
				count,
				res_dropdown.value == 0 ? Guild.Action.AskGold : res_dropdown.value == 1 ? Guild.Action.AskFood : res_dropdown.value == 2 ? Guild.Action.AskRecruits : Guild.Action.AskFuel
			));
            WarningManager.Instance.Warn("Ваш запрос добавлен в голосования");
        }
		else
		{
            if (res_dropdown.value == 0)
            {
                if (count > ReferencesManager.Instance.countryManager.currentCountry.money)
                {
                    WarningManager.Instance.Warn("У вас нет столько ресурсов!");
                    return;
                }
                else
                {
                    ReferencesManager.Instance.countryManager.currentCountry.money -= count;
                    guild._storage.gold += count;
                }
            }
            else if (res_dropdown.value == 1)
            {
                if (count > ReferencesManager.Instance.countryManager.currentCountry.food)
                {
                    WarningManager.Instance.Warn("У вас нет столько ресурсов!");
                    return;
                }
                else
                {
                    ReferencesManager.Instance.countryManager.currentCountry.food -= count;
                    guild._storage.food += count;
                }
            }
            else if (res_dropdown.value == 2)
            {
                if (count > ReferencesManager.Instance.countryManager.currentCountry.recruits)
                {
                    WarningManager.Instance.Warn("У вас нет столько ресурсов!");
                    return;
                }
                else
                {
                    ReferencesManager.Instance.countryManager.currentCountry.recruits -= count;
                    guild._storage.recruits += count;
                }
            }
            else if (res_dropdown.value == 3)
            {
                if (count > ReferencesManager.Instance.countryManager.currentCountry.fuel)
                {
                    WarningManager.Instance.Warn("У вас нет столько ресурсов!");
                    return;
                }
                else
                {
                    ReferencesManager.Instance.countryManager.currentCountry.fuel -= count;
                    guild._storage.fuel += count;
                }
            }
            WarningManager.Instance.Warn("Вы успешно пожертвовали ресурсы в казну");
            _storageUI.UpdateUI();

            ReferencesManager.Instance.countryManager.UpdateValuesUI();
            ReferencesManager.Instance.countryManager.UpdateIncomeValuesUI();
        }
	}

	[System.Serializable]
	public enum Action
	{
		Ask,
		Send
	}
}
