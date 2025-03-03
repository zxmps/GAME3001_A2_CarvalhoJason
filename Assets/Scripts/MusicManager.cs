using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance; 
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); 
        }

        audioSource.loop = true; 
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
