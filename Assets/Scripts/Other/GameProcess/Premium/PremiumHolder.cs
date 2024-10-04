using UnityEngine;

public class PremiumHolder : MonoBehaviour
{
    [SerializeField] private GameObject[] _objects;

    private void Awake()
    {
        CheckPremium();
    }

    private void CheckPremium()
    {
        for (int i = 0; i < _objects.Length; i++)
        {
            _objects[i].SetActive(!ReferencesManager.Instance.gameSettings._isPremium.value);
        }
    }
}
