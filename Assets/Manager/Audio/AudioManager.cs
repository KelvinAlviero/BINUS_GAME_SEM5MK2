using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource soundFXObject;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        float length = audioSource.clip.length;

        Destroy(audioSource.gameObject, length);

    }

    public void PlaySoundFXClipWithRandomPitch(AudioClip audioClip, Transform spawnTransform, float volume, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(minPitch, maxPitch); // Random pitch
        audioSource.Play();

        float length = audioSource.clip.length;
        Destroy(audioSource.gameObject, length);
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
