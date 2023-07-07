using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class GameSetting : ScriptableObject
{
  public GameAudio Audio;
  public CharMB PrefabSymbol;

  public GameTheme ThemeDefault;

  public List<GamePlayerSetting> PlayerSettings;

  [Space(5)]
  [Header("Game")]
  public int countHoursDailyGift;
  [Range(0.1f, 1f)] public float lineWidth;
  [Range(0.5f, 3f)] public float radius;
  [Range(0, 3)] public int addinitiallyRow;
  [Range(10, 200)] public int maxCountHiddenChar;
  [Range(10, 20)] public int minCountHiddenChar;

  [Space(5)]
  [Header("Animations")]
  [Range(0.1f, 2f)] public float timeGeneralAnimation;

  [Space(5)]
  [Header("Particle System")]
  public ParticleSystem Boom;
  public ParticleSystem BoomLarge;

  [Space(5)]
  [Header("Assets")]
  // public Sprite spriteCoin;
  // public Sprite spriteStar;
  // public Sprite spriteBomb;
  public Sprite spriteShuffle;
  public Sprite spriteFlask;
  public Sprite spriteDirectory;
  public Sprite spriteCog;
  // public Sprite spriteLevels;
  // public Sprite spriteCheck;
  // public Sprite spriteHint;
  // public Sprite spriteBox;
  // public Sprite spriteLighting;
  // public Sprite spriteLock;
  // public GameObject PrefabCoin;
  // public GameObject PrefabBomb;
  // public GameObject PrefabStar;
  // public GameObject PrefabLighting;
  public Sprite spriteRate;
  public Sprite spriteShop;
  public Sprite spriteAdv;
  public Sprite spriteBuy;
  public Sprite spriteInfo;
  public Sprite spriteDouble;
  public Sprite spriteUser;

  [Space(5)]
  [Header("System")]
  public int debounceTime;
  [Range(10, 1000)] public int timeDelayOverChar;
  public List<GameLevel> GameLevels;

  [Space(5)]
  [Header("Shop")]
  public List<ShopItem<GameEntity>> ShopItems;
  public List<ShopItem<GameBonus>> ShopItemsBonus;


  [Space(5)]
  [Header("Texts")]
  public TextLocalize noName;

  [Space(5)]
  [Header("API Directory")]
  public APIDirectory APIDirectory;

  [Space(5)]
  [Header("Directory words")]
  public List<WordsFile> wordFiles;


  [Space(5)]
  [Header("Ads")]
  public int adsPerTime;

  [Space(5)]
  [Header("Rate")]
  public int minRateForReview;
  public int countCoinForReview;
}

[System.Serializable]
public struct APIDirectory
{
  // public string host;
  public string token;
  // public string expression;
  public string pathExpression;
}


[System.Serializable]
public struct ShopItem<T>
{
  public T entity;
  public int count;
  public int cost;
}

[System.Serializable]
public struct ShopAdvBuyItem<T>
{
  public T typeItem;
  public int count;
}
