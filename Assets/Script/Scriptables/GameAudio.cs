using UnityEngine;

[CreateAssetMenu]
public class GameAudio : ScriptableObject
{
  [Range(0, 1)] public float volumeEffect;
  [Range(0, 1)] public float volumeMusic;
  public AudioClip addChar;
  public AudioClip removeChar;
  public AudioClip openHiddenChar;
  public AudioClip openHintChar;
  public AudioClip existWord;
  public AudioClip noWord;
  public AudioClip addToFlask;
  public AudioClip addCoin;
  public AudioClip calculateCoin;
  public AudioClip clickButton;

  public AudioClip openWord;
  [Header("Bonus")]
  public AudioClip addBonus;
}
