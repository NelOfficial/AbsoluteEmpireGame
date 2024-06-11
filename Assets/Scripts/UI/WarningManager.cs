using TMPro;
using UnityEngine;
using System.Collections;

public class WarningManager : MonoBehaviour
{
    public static WarningManager Instance;

    public GameObject warnPrefab;

    [SerializeField] float warnLifetime = 2f;
    [SerializeField] float warnFadeoutDelay = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    public void Warn(string warnText)
    {
        GameObject spawnedWarn = Instantiate(warnPrefab);

        spawnedWarn.transform.SetParent(this.transform);
        spawnedWarn.transform.localScale = new Vector3(1, 1, 1);
        spawnedWarn.transform.Find("WarningItem_Text").GetComponent<TMP_Text>().text = warnText;

        StartCoroutine(Animation_Co(spawnedWarn.GetComponent<Animator>()));
    }

    private IEnumerator Animation_Co(Animator animator)
    {
        yield return new WaitForSeconds(warnLifetime);
        animator.Play("fadeout");
        Destroy(animator.gameObject, warnFadeoutDelay);
        StopCoroutine(Animation_Co(animator));
    }
}
