using UnityEngine;
using TMPro;

public class StorageMenu2 : MonoBehaviour
{
    [SerializeField] private TMP_Text gold_text;
    [SerializeField] private TMP_Text food_text;
    [SerializeField] private TMP_Text recruits_text;
    [SerializeField] private TMP_Text fuel_text;


    public void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);

        gold_text.text = ReferencesManager.Instance.GoodNumberString(guild._storage.gold);
        food_text.text = ReferencesManager.Instance.GoodNumberString(guild._storage.food);
        recruits_text.text = ReferencesManager.Instance.GoodNumberString(guild._storage.recruits);
        fuel_text.text = ReferencesManager.Instance.GoodNumberString(guild._storage.fuel);
    }
}
