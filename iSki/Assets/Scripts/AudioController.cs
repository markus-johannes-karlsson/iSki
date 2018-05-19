using UnityEngine;

/// <summary>
/// Loops and fades in/out ambient music
/// </summary>
public class AudioController : MonoBehaviour {

    public AudioSource ambient;
    public float ambientMaxVolume = 0.2f;
    public float ambientFadeTime = 6.0f;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    private void Start()
    {
        StartPlaying();
    }
    
    private void StartPlaying()
    {
        ambient.volume = 0.0f;
        isFadingIn = true;

        ambient.Play();

        Invoke("StartFadeOut", ambient.clip.length - ambientFadeTime);
    }

    private void StartFadeOut()
    {
        isFadingOut = true;
    }

    private void Update()
    {
        if (isFadingIn)
        {
            ambient.volume += (ambientMaxVolume / (ambientFadeTime * (1 / Time.deltaTime)));

            if (ambient.volume >= ambientMaxVolume) {
                ambient.volume = ambientMaxVolume;
                isFadingIn = false;
            }
        }

        if (isFadingOut)
        {
            ambient.volume -= (ambientMaxVolume / (ambientFadeTime * (1 / Time.deltaTime)));

            if (ambient.volume <= 0.0f)
            {
                ambient.volume = 0.0f;
                isFadingOut = false;

                StartPlaying();
            }
        }
    }
}
