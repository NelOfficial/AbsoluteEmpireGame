using UnityEngine;

namespace Assets.Scripts.UI
{
    public class GameSettings : MonoBehaviour
    {
        private Settings settings;

        /*private void Start()
        {
            settings = FindObjectOfType<Settings>();

            if (PlayerPrefs.HasKey("musicVolume"))
            {
                if (settings != null)
                {
                    settings.musicSource.volume = PlayerPrefs.GetFloat("musicVolume");
                }
            }
            settings.musicSource.loop = false;

            settings.musicSource.PlayOneShot(settings.audioClips[settings.currentClip]);
            settings.songNameText.text = settings.musicSource.clip.name;

            settings.CheckButtonUI();
        }*/
    }
}