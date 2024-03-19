using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Settings : MonoBehaviour
{
    private RegionUI regionUI;
    private GameSettings gameSettings;

    public Camera mainCamera;
    public Camera renderCamera;
    public Color[] colors;
    public Color[] colorsHigh;
    public Color[] colorsDeco;
    public Sprite[] maps;

    [SerializeField] Image waterImage;
    [SerializeField] Image waterDecoImage;
    [SerializeField] Image mapImage;
    [SerializeField] Image seaMapImage;

    public TMP_Text songNameText;
    public Image buttonPlayIcon;

    public Sprite playIcon;
    public Sprite pauseIcon;

    public AudioSource musicSource;
    public AudioClip[] audioClips;
    [HideInInspector] public int currentClip;

    [SerializeField] RectTransform renderCanvas;

    public GameObject settingsContainer;
    public bool paused;

    [SerializeField] GameObject borderMap;

    public Material defaultMaterial;
    public Material outlineMaterial;

    private void Start()
    {
        songNameText.text = musicSource.clip.name;
        songNameText.text = musicSource.clip.name;

        regionUI = FindObjectOfType<RegionUI>();
        gameSettings = FindObjectOfType<GameSettings>();

        CheckButtonUI();
    }

    public void OpenUI()
    {
        regionUI.CloseAllUI();
        settingsContainer.SetActive(true);
        BackgroundUI_Overlay.Instance.OpenOverlay(settingsContainer);
    }

    public void CloseUI()
    {
        if (settingsContainer.activeSelf)
        {
            settingsContainer.GetComponent<UI_Panel>().ClosePanel();
        }
        BackgroundUI_Overlay.Instance.CloseOverlay();
    }

    public void ToggleUI()
    {
        if (!settingsContainer.activeSelf)
        {
            OpenUI();
        }
        else{
            CloseUI();
        }
    }

    public void ChangeOceanColor(int index)
    {
        waterImage.color = colors[index];
        waterDecoImage.color = colorsDeco[index];
        seaMapImage.color = colorsHigh[index];
        UISoundEffect.Instance.PlayAudio(regionUI.click_01);
    }

    public void ChangeMapSprite(int index)
    {
        mapImage.sprite = maps[index];
        UISoundEffect.Instance.PlayAudio(regionUI.click_01);
    }

    public void ChangeZoomSpeed(float speed)
    {
        mainCamera.GetComponent<CameraMovement>().Map_CloseUpSpeed = speed;
    }

    public void ChangeMoveSpeed(float speed)
    {
        mainCamera.GetComponent<CameraMovement>().CameraSpeed = speed;
    }

    public void SetTexturesSize(int index)
    {
        QualitySettings.globalTextureMipmapLimit = index;
    }

    public void ChangeOutlineState(bool borders)
    {
        foreach (RegionManager province in ReferencesManager.Instance.countryManager.regions)
        {
            if (!borders)
            {
                province.GetComponent<SpriteRenderer>().material = outlineMaterial;
                borderMap.SetActive(false);
            }
            else
            {
                province.GetComponent<SpriteRenderer>().material = outlineMaterial;
                borderMap.SetActive(true);
            }
        }
    }

    public void CameraRestart()
    {
        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, -10);
    }

    public void ChangeMusicVolume(float vol)
    {
        PlayerPrefs.SetFloat("musicVolume", vol);
        musicSource.volume = vol;
    }

    public void MainMenu()
    {
        PlayerPrefs.DeleteKey("currentCountryIndex");

        ReferencesManager.Instance.gameSettings.loadGame.value = false;
        ReferencesManager.Instance.gameSettings.playMod.value = false;
        ReferencesManager.Instance.gameSettings.playTestingMod.value = false;

        if (gameSettings.onlineGame)
        {
            PhotonNetwork.LeaveRoom();
        }
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        MusicChanger();
    }

    private void MusicChanger()
    {
        if (!paused && !musicSource.isPlaying)
        {
            ChangeMusic(1);
        }
    }

    public void CheckButtonUI()
    {
        // Stop
        if (musicSource.isPlaying)
        {
            buttonPlayIcon.sprite = pauseIcon;
        }
        // Play
        else if (!musicSource.isPlaying)
        {
            buttonPlayIcon.sprite = playIcon;
        }
    }

    public void PlayPauseMusic()
    {
        // Stop
        if (musicSource.isPlaying || !paused)
        {
            musicSource.Pause();
            buttonPlayIcon.sprite = playIcon;
            paused = true;
        }
        // Play
        else if (!musicSource.isPlaying || paused)
        {
            musicSource.Play();
            buttonPlayIcon.sprite = pauseIcon;
            paused = false;
        }
    }

    public void Play()
    {
        musicSource.Play();
        buttonPlayIcon.sprite = pauseIcon;
        paused = false;
    }

    public void ChangeMusic(int increment)
    {
        currentClip += increment;

        if (currentClip >= audioClips.Length)
        {
            currentClip = 0;
        }

        else if (currentClip == -1)
        {
            currentClip = audioClips.Length - 1;
        }

        musicSource.clip = audioClips[currentClip];
        musicSource.Play();
        paused = false;
        try
        {
            if (songNameText != null || musicSource != null)
            {
                songNameText.text = musicSource.clip.name;
            }
        }
        catch (System.Exception)
        {

        }


        CheckButtonUI();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
