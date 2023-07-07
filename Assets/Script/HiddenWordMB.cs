using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HiddenWordMB : MonoBehaviour
{
  [SerializeField] private HiddenCharMB charMB;
  private DataManager _dataManager = DataManager.Instance;
  public List<HiddenCharMB> Chars;
  public string _word;
  private ManagerHiddenWords _managerHiddenWords;
  //   private InputManager _inputManager;
  //   private Camera _camera;
  //   [SerializeField] private Collider2D _collider;

  private void Awake()
  {
    Chars = new List<HiddenCharMB>();
  }

  public void DrawWord(string word, List<GridNode> nodes)
  {
    _word = word;
    for (int i = 0; i < word.Length; i++)
    {
      var newChar = GameObject.Instantiate(
        charMB,
        Vector3.zero,
        Quaternion.identity,
        gameObject.transform
      );
      newChar.transform.localPosition = new Vector3(i + .5f, 0 + .5f);
      // newChar.transform.localPosition = new Vector3(i * _size, 0, 0);
      var currentChar = word.ElementAt(i);
      // var currentCharMB = Chars.ElementAt(i);
      newChar.gameObject.SetActive(true);
      newChar.SetChar(currentChar);
      Chars.Add(newChar);
      nodes[i].SetOccupiedChar(newChar, this);
    }
  }

  public async UniTask ShowWord()
  {
    foreach (var charObj in Chars)
    {
      await charObj.ShowChar(false, charObj.charTextValue);
      // OpenNeighbours(charObj).Forget();
    }
  }

  public async UniTask AutoOpenWord()
  {
    if (!_managerHiddenWords.OpenWords.ContainsKey(_word)) _managerHiddenWords.OpenWords.Add(_word, 1);
    if (!_managerHiddenWords.OpenNeedWords.ContainsKey(_word)) _managerHiddenWords.OpenNeedWords.Add(_word, 1);

    List<UniTask> tasks = new();
    foreach (var charObj in Chars)
    {
      tasks.Add(charObj.ShowChar(true, charObj.charTextValue));
    }
    await UniTask.WhenAll(tasks);

    await _managerHiddenWords.CheckStatusRound();

  }

  public async UniTask FocusOpenWord()
  {
    List<UniTask> tasks = new();
    foreach (var charObj in Chars)
    {
      tasks.Add(charObj.FocusOpenChar());
    }
    await UniTask.WhenAll(tasks);
  }

  public void Init(ManagerHiddenWords managerHiddenWords)
  {
    _managerHiddenWords = managerHiddenWords;
  }
}
