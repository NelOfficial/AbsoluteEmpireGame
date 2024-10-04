using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;


public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    private CountryManager countryManager;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        countryManager = FindObjectOfType<CountryManager>();
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
        {
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), Vector3.zero, Quaternion.identity);
        }
    }
}
