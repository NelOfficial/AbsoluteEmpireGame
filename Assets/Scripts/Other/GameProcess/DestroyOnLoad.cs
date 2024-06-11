using UnityEngine;
using System.Collections;

public class DestroyOnLoad : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Coroutine());
    }

    public void Toggle()
    {
        StartCoroutine(Coroutine_F());
        StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine_F()
    {
        this.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        this.gameObject.SetActive(true);
        yield break;
    }

    private IEnumerator Coroutine()
    {
        this.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        this.gameObject.SetActive(false);
        yield break;
    }
}
