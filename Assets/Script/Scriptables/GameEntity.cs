using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class GameEntity : ScriptableObject
{
  public string idEntity;
  public TypeEntity typeEntity;
  public TextLocalize text;
  public Sprite sprite;
  [SerializeField] public AssetReferenceGameObject prefab;
  public ParticleSystem activateEffect;
  public ParticleSystem MoveEffect;
  public bool isUseGenerator;
  public AudioClip soundRunEntity;
  public AudioClip soundAddEntity;
}


[System.Serializable]
public enum TypeEntity
{
  None = 0,
  Bomb = 1,
  Lighting = 2,
  Star = 3,
  Coin = 4,
  OpenWord = 5,
  Frequency = 6,
  Letter = 7
}
