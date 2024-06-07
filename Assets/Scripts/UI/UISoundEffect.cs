using UnityEngine;

public class UISoundEffect : MonoBehaviour
{
    [SerializeField] AudioSource uiAudioSource;

    public static UISoundEffect Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayAudio(AudioClip audioClip)
    {
        uiAudioSource.PlayOneShot(audioClip);
    }
}
