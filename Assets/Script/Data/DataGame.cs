using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateGame
{
  public int rate;
  public List<StateGameItem> items;
  [System.NonSerialized] public DataGame activeDataGame;
  public int coins;
  public string lastTime;
  public int countRound;

  public StateGame()
  {
    items = new();
  }
}

[System.Serializable]
public class StateGameItem
{
  public string lang;
  public DataGame dataGame;
}

[System.Serializable]
public class DataGame
{
  public int rate;
  public string rank;
  // public int coins;
  public SerializableDictionary<TypeEntity, int> hints;
  public SerializableDictionary<TypeBonus, int> bonus;
  public List<DataLevel> levels;
  public List<string> completed;
  [System.NonSerialized] public DataLevel activeLevel;
  public string lastWord;

  public DataGame()
  {
    levels = new();
    completed = new();
    hints = new();
    bonus = new();
  }
}


[System.Serializable]
public class DataLevel
{
  public string id;
  public float index;
  public int coins;
  public BonusCount bonusCount;
  public SerializableDictionary<Vector3, string> openChars;
  public SerializableDictionary<TypeEntity, int> hints; // [System.NonSerialized]
  public SerializeEntity ent;
  public List<string> openWords;
  public List<string> crossWords;
  public string word;
  public int countCrossWords;
  public int countOpenChars;
  public List<string> openCrossWords;

  // public int countDopWords;

  public DataLevel()
  {
    openWords = new();
    crossWords = new();
    openCrossWords = new();
    openChars = new();
    ent = new();
    hints = new();
    bonusCount = new();
  }
}

[System.Serializable]
public struct JsonDateTime
{
  public long value;
  public static implicit operator System.DateTime(JsonDateTime jdt)
  {
    // Debug.Log("Converted to time");
    return System.DateTime.FromFileTimeUtc(jdt.value);
  }
  public static implicit operator JsonDateTime(System.DateTime dt)
  {
    // Debug.Log("Converted to JDT");
    JsonDateTime jdt = new JsonDateTime();
    jdt.value = dt.ToFileTimeUtc();
    return jdt;
  }
}
