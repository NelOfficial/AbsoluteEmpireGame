using System.Collections;
using UnityEngine;

public class AviationManager : MonoBehaviour
{
    [SerializeField] private GameObject _attackMenu;
    public RegionManager[] regions;

    public IEnumerator Explosion_Co(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        foreach (RegionManager reg in regions)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.2f));
            Explosion(reg);
        }

        yield break;
    }

    public void StartExplosionCoroutine(float delay)
    {
        StartCoroutine(Explosion_Co(delay));
    }

    public void Explosion(RegionManager region)
    {
        Instantiate(ReferencesManager.Instance.gameSettings._exposionPrefab, region.transform);
    }
}
