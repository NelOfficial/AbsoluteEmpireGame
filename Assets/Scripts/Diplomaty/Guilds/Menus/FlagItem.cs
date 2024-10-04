using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FlagItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _countryNameText;
    
    public void SetUp(Guild.FlagSprite flag)
    {
        _countryNameText.text = flag.guild_name;
        _image.sprite = flag.sprite;
    }

    public void OnClick()
    {
        Guild guild = Guild.GetGuild(GuildSelectionManager._selectedGuild._itemName);
        guild._icon = _image.sprite;

        FlagSelectionMenu.Instance.gameObject.SetActive(false);

        GuildManageMenu.Instance.UpdateGuildsList_UI();
        GuildMenu.Instance.Enable();
    }
}
