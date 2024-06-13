using Mirror;

public class PlayerData : NetworkBehaviour
{
    public string currentNickname;
    public int countryIndex;
    public bool readyToMove;

    public int _connectionID;

    public CountrySettings country;


    private void Awake()
    {
        ReferencesManager.Instance.launcher.playersData.Add(this);

        //string _name = gameObject.name;

        //string[] _nameData = _name.Split('=');

        //_connectionID = int.Parse(_nameData[1].Trim(']'));
    }
}
