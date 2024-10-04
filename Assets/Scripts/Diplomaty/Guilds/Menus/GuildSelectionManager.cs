using UnityEngine;
using UnityEngine.UI;

public class GuildSelectionManager : MonoBehaviour
{
	[HideInInspector] public static GuildSelectionManager _selectedGuild;

	public Image _flagImage;
	public string _itemName;
	public Type _type;

	public void SetUp(Sprite _icon, string _name, Type type)
	{
		_flagImage.sprite = _icon;
		_itemName = _name;
		_type = type;
	}

	public void OnClick()
	{
		if (_type == Type.Guild)
		{
            _selectedGuild = this;

            GuildMenu.Instance.Enable();
            GuildManageMenu.Instance.start_dialog.SetActive(false);

			return;
        }	
	}

	public enum Type
	{
		Guild,
		Country,
		ChangeFlag
	}
}