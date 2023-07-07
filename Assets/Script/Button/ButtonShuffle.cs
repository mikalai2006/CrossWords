using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonShuffle : BaseButton
{
  public static event Action<string> OnShuffleWord;

  #region UnityMethods
  protected override void Awake()
  {
    base.Awake();

    spriteBg.sprite = _gameSetting.spriteShuffle;
    spriteMask.sprite = _gameSetting.spriteShuffle;

    interactible = false;
  }
  #endregion

  public override void ChangeTheme()
  {
    spriteBg.color = _gameManager.Theme.entityColor;
  }

  public override async void OnPointerDown(PointerEventData eventData)
  {
    base.OnPointerDown(eventData);

    await RefreshChars();
  }


  public async UniTask RefreshChars()
  {
    var existChars = _levelManager.Symbols;

    // get all positions.
    Dictionary<CharMB, char> existPositionsChars = new();
    for (int i = 0; i < existChars.Count; i++)
    {
      existPositionsChars.Add(existChars[i], existChars[i].charTextValue);
    }
    // shuffle positions.
    existChars = existChars.OrderBy(t => UnityEngine.Random.value).ToList();

    // Run animation button.
    RotateButton();

    // set new position.
    List<UniTask> tasks = new();
    string newWord = "";
    for (int i = 0; i < existChars.Count; i++)
    {
      tasks.Add(existChars[i].SetPosition(existPositionsChars.ElementAt(i).Key.transform.position));
      newWord += existChars.ElementAt(i).charTextValue;
    }
    OnShuffleWord?.Invoke(newWord);

    await UniTask.WhenAll(tasks);
    // GameManager.Instance.DataManager.Save();
    _stateManager.RefreshData(true);
  }


  private void RotateButton()
  {
    transform
      .DOPunchScale(new Vector3(.2f, .2f, 0), _gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBack)
      .OnComplete(() =>
      {
        transform.localScale = initScale;
      });
  }
}
