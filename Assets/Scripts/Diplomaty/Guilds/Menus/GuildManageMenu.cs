using UnityEngine;
using System.Linq;

public class GuildManageMenu : MonoBehaviour
{
	public static GuildManageMenu Instance;

	[SerializeField] private Transform view;
    [SerializeField] private GameObject cell;
    public GameObject start_dialog;

	private static CountryManager countryManager;

    private void Start()
    {
        Instance = this;
    }

    public void Enable()
	{
        countryManager = ReferencesManager.Instance.countryManager;

        UpdateGuildsList_UI();

        GuildMenu.Instance.gameObject.SetActive(false);
		CreateGuildMenu.Instance.gameObject.SetActive(false);
		menu3.Instance.gameObject.SetActive(false);
		CreateOfferMenu.Instance.gameObject.SetActive(false);

		FlagSelectionMenu.Instance.gameObject.SetActive(false);

        start_dialog.SetActive(true);
    }

    public void Disable()
    {
        GuildMenu.Instance.gameObject.SetActive(false);
        CreateGuildMenu.Instance.gameObject.SetActive(false);
        menu3.Instance.gameObject.SetActive(false);
        CreateOfferMenu.Instance.gameObject.SetActive(false);

        FlagSelectionMenu.Instance.gameObject.SetActive(false);
        GuildManageMenu.Instance.gameObject.SetActive(false);
    }

	public void UpdateGuildsList_UI()
	{
        foreach (Transform item in view)
        {
            Destroy(item.gameObject);
        }

        foreach (Guild guild in countryManager.currentCountry.guilds)
        {
            GuildSelectionManager obj = Instantiate(cell, view).GetComponent<GuildSelectionManager>();
            obj.SetUp(guild._icon, guild._name, GuildSelectionManager.Type.Guild);
        }

        if (Guild._guilds != null)
        {
            foreach (Guild guild in Guild._guilds)
            {
                if (!countryManager.currentCountry.guilds.Any(item => item._id == guild._id))
                {
                    GuildSelectionManager obj = Instantiate(cell, view).GetComponent<GuildSelectionManager>();
                    obj.SetUp(guild._icon, guild._name, GuildSelectionManager.Type.Guild);
                }
            }
        }
    }
}