using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// A static class for general helpful methods
/// </summary>
public static class Helpers
{
  /// <summary>
  /// Destroy all child objects of this transform (Unintentionally evil sounding).
  /// Use it like so:
  /// <code>
  /// transform.DestroyChildren();
  /// </code>
  /// </summary>
  public static void DestroyChildren(this Transform t)
  {
    foreach (Transform child in t) UnityEngine.Object.Destroy(child.gameObject);
  }

  // public static int GenerateChance(int start = 0, int end = 100)
  // {
  //   System.Random random = new System.Random();
  //   return random.Next(0, 100);
  // }

  public static Dictionary<Transform, dynamic> GetChildrenHierarchy(this GameObject gameobject)
  {
    Dictionary<Transform, dynamic> children = new Dictionary<Transform, dynamic>();

    foreach (Transform child in gameobject.transform)
    {
      children.Add(child, GetChildrenHierarchy(child.gameObject));
    }

    return children;
  }

  /// <summary>
  /// Clone Dictionary
  /// </summary>
  /// <param name="original"></param>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  /// <returns></returns>
  public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
 (Dictionary<TKey, TValue> original) where TValue : ICloneable
  {
    Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                                                            original.Comparer);
    foreach (KeyValuePair<TKey, TValue> entry in original)
    {
      ret.Add(entry.Key, (TValue)entry.Value.Clone());
    }
    return ret;
  }


  public static void LineConnection(LineRenderer lr, GameObject first, GameObject second, float linewidth)
  {
    // var lr = first.GetComponent<LineRenderer>();
    lr.SetPosition(0, first.gameObject.transform.position);
    lr.SetPosition(1, second.gameObject.transform.position);
    lr.startWidth = linewidth;
    lr.endWidth = linewidth;
  }
  public static string GetColorString(string str)
  {
    return " <color=#FFFFAB>" + str + "</color>";
  }


  public async static UniTask<string> GetLocaledString(LocalizedString localizedString)
  {
    var t = localizedString.GetLocalizedStringAsync();
    await t.Task;
    return t.Result;
  }


  public async static UniTask<string> GetLocaledString(string key)
  {
    var t = new LocalizedString(Constants.LanguageTable.LANG_TABLE_LOCALIZE, key).GetLocalizedStringAsync();
    await t.Task;
    return t.Result;
  }


  public static IEnumerable<T> IntersectWithRepetitons<T>(this IEnumerable<T> first,
    IEnumerable<T> second)
  {
    var lookup = second.GroupBy(x => x).ToDictionary(group => group.Key, group => group.Count());
    foreach (var item in first)
      if (lookup.ContainsKey(item) && lookup[item] > 0)
      {
        yield return item;
        lookup[item]--;
      }
  }


  public async static UniTask<string> GetLocalizedPluralString<T>(string key, Dictionary<string, T> data)
  {
    var localizedString = new LocalizedString(Constants.LanguageTable.LANG_TABLE_LOCALIZE, key);
    var args = new[] { data };
    localizedString.Arguments = args;
    var t = localizedString.GetLocalizedStringAsync(data);
    await t.Task;
    return t.Result;
  }


  public async static UniTask<string> GetLocalizedPluralString<T>(
        LocalizedString localizedString,
        Dictionary<string, T>[] args,
        Dictionary<string, T> dictionary
        )
  {
    if (localizedString.IsEmpty) return "NO_LANG";

    localizedString.Arguments = args;
    var t = localizedString.GetLocalizedStringAsync(dictionary);
    await t.Task;
    return t.Result;
  }

  public static bool HasValueDouble(this double value)
  {
    return !Double.IsNaN(value) && !Double.IsInfinity(value);
  }

  // public async static UniTask<string> GetPlayPrefKey()
  // {
  //   await LocalizationSettings.InitializationOperation.Task;

  //   return string.Format("{0}_{1}",
  //     GameManager.Instance.namePlayPref,
  //     LocalizationSettings.SelectedLocale.Identifier.Code
  //   );
  // }


  public static string StripHTML(string input)
  {
    return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", String.Empty);
  }


  public static async UniTask<Texture2D> LoadTexture(string path)
  {
    Texture2D result = null;

    if (string.IsNullOrEmpty(path)) return result;

    if (Application.internetReachability == NetworkReachability.NotReachable) return result;

    UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(path);

    try
    {
      await webRequest.SendWebRequest();

      Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
      result = texture;
    }
    catch (System.Exception error)
    {
      Debug.Log(error);
      return result;
    }


    // if (webRequest.result == UnityWebRequest.Result.ConnectionError
    //   || webRequest.result == UnityWebRequest.Result.DataProcessingError
    //   || webRequest.result == UnityWebRequest.Result.ProtocolError)
    // {
    //   Debug.Log(webRequest.error);
    //   return result;
    // }
    // else
    // {
    //   Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
    //   result = texture;
    // }

    return result;
  }

  public static async UniTask<string> GetName()
  {
    var _gameManager = GameManager.Instance;

    string userName = string.IsNullOrEmpty(_gameManager.AppInfo.UserInfo.name)
  ? await Helpers.GetLocaledString(_gameManager.GameSettings.noName.title)
  : _gameManager.AppInfo.UserInfo.name;
    return userName;
  }
  // public static Dictionary<string, List<string>> GetDictionaryCompleteLevel(List<string> list)
  // {
  //   Dictionary<string, List<string>> result = new();

  //   foreach (var item in list)
  //   {
  //     var splitString = item.Split(":");
  //     if (!result.ContainsKey(splitString[0]))
  //     {
  //       result[splitString[0]] = new List<string>() {
  //         splitString[1]
  //       };
  //     }
  //     else
  //     {
  //       result[splitString[0]].Add(splitString[1]);
  //     }
  //   }

  //   return result;
  // }
  /// <summary>
  ///  Get the probability of getting an item
  /// </summary>
  /// <param name="item">Items for search</param>
  /// <returns>result item or first item<T></returns>
  public static ResultProbabiliti<T> GetProbabilityItem<T>(List<ItemProbabiliti<T>> items)
  {
    double p = new System.Random().NextDouble();
    double accumulator = 0.0;
    var result = new ResultProbabiliti<T>()
    {
      Item = items[0].Item,
      index = 0
    };
    for (int i = 0; i < items.Count; i++)
    {
      ItemProbabiliti<T> item = items[i];
      accumulator += item.probability;
      if (p <= accumulator)
      {
        result.Item = item.Item;
        result.index = i;
        break;
      }
    }
    return result;
  }
}

[System.Serializable]
public struct ItemProbabiliti<T>
{
  public T Item;
  [Range(0, 1)] public double probability;
}

[System.Serializable]
public struct ResultProbabiliti<T>
{
  public T Item;
  public int index;
}