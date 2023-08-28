using UnityEngine;
using TMPro;

public class TechnologyButton : MonoBehaviour
{
    public TechnologyScriptableObject technology;

    [SerializeField] TMP_Text buttonText;

    public void SetUp()
    {
        transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = technology._name;
    }
}
