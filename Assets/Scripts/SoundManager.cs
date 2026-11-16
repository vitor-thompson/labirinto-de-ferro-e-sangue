using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Fontes de Áudio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clipes de Efeitos Sonoros")]
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip attackSound;
    public AudioClip playerHitSound;
    public AudioClip gameOverSound;

    [Header("Variação de Tom")]
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantém o som vivo ao trocar de cena
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destrói cópias duplicadas se você voltar ao menu
        }
    }

    // Toca um som único
    public void PlaySingle(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clip);
    }

    // Escolhe aleatoriamente entre dois sons e altera levemente o tom (ótimo para passos e comidas)
    public void RandomizeSfx(params AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || sfxSource == null) return;

        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        sfxSource.pitch = randomPitch;
        sfxSource.PlayOneShot(clips[randomIndex]);
    }
}