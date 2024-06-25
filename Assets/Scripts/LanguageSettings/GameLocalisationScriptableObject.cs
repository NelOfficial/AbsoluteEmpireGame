using UnityEngine;

[CreateAssetMenu(fileName = "NewLocalisationFile", menuName = "Values/LocalisationFile")]
public class GameLocalisationScriptableObject : ScriptableObject
{
    [Header("IDEOLOGY")]
    public string NEUTRAL_IDEOLOGY;
    public string DEMOCRACY_IDEOLOGY;
    public string MONARCHY_IDEOLOGY;
    public string FASCISM_IDEOLOGY;
    public string COMMUNISM_IDEOLOGY;

    [Header("MOBILISATION LAW")]
    public string UNARMED_COUNTRY_LAW;
    public string VOLUNTEER_ARMY_LAW;
    public string PARTIAL_APPEAL_LAW;
    public string MANDATORY_SERVICE_LAW;
    public string ALL_ADULTS_ARE_SERVING_LAW;
    public string TOTAL_MOBILIZATION_LAW;

    [Header("INFO PANELS")]
    public string SETTINGS_INFO_TEXT;
    public string DIPLOMATY_INFO_TEXT;
    public string BUILDING_INFO_TEXT;
    public string OFFLINE_GAME_INFO_TEXT;
}
