using UnityEngine;

public class AudioManager : StaticInstance<AudioManager>
{
  [SerializeField] private AudioSource _musicSource;
  public AudioSource MusicSource => _musicSource;
  [SerializeField] private AudioSource _effectSource;
  public AudioSource EffectSource => _effectSource;
  [SerializeField] private GameSetting GameSetting;
  protected override void Awake()
  {
    base.Awake();
    _musicSource.volume = GameSetting.Audio.volumeMusic;
    _effectSource.volume = GameSetting.Audio.volumeEffect;
  }

  public void PlayClipMusic(AudioClip clip)
  {
    MusicSource.PlayOneShot((AudioClip)clip);
  }

  public void PlayClipEffect(AudioClip clip)
  {
    EffectSource.PlayOneShot((AudioClip)clip);
    // AudioSource.PlayClipAtPoint(clip, transform.position, GameSetting.Audio.volumeEffect);
  }

  public void Click()
  {
    EffectSource.PlayOneShot((AudioClip)GameSetting.Audio.clickButton);
  }
}
