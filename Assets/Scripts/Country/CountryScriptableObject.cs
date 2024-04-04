using UnityEngine;

[CreateAssetMenu(fileName = "NewCountry", menuName = "Countries/Country")]
public class CountryScriptableObject : ScriptableObject
{
    public enum CountryType
    {
        Poor,
        SemiPoor,
        Middle,
        SemiRich,
        Rich,
        Ussr
    }

    public int _id;
    public string _name;
    public string _nameEN;
    public string _uiName;
    public string _tag;

    public CountryType countryType;

    public Sprite countryFlag;

    public string ally;

    public int capitulateLimit = 20;

    public Color countryColor;

    public Color[] countryIdeologyColors;
    public Sprite[] countryIdeologyFlags;
}
