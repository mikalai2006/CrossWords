using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BonusIndex : BaseBonus
{
  RectTransform _rectProgress;
  private float _maxHeightProgress;

  #region UnityMethods
  protected override void Awake()
  {
    configBonus = _gameManager.ResourceSystem.GetAllBonus().Find(t => t.typeBonus == TypeBonus.Index);

    base.Awake();

    _rectProgress = spriteProgress.GetComponent<RectTransform>();
    _maxHeightProgress = spriteBg.GetComponent<RectTransform>().rect.height;

    StateManager.OnChangeState += SetValue;
  }
  protected override void OnDestroy()
  {
    StateManager.OnChangeState -= SetValue;

    base.OnDestroy();
  }
  #endregion

  public override void SetValue(StateGame state)
  {
    value = state.activeDataGame.bonus.GetValueOrDefault(TypeBonus.Index);

    counterText.text = counterTextBlack.text = string.Format("x{0}", value + 1);

    base.SetValue(state);

    // if (value == 0) return;

    SetValueProgressBar(state);
  }

  public override void SetValueProgressBar(StateGame state)
  {
    // base.SetValueProgressBar(state);


    var maxValue = _gameManager.PlayerSetting.bonusCount.wordInOrder;
    var currentValue = state.activeDataGame.activeLevel.bonusCount.wordInOrder;

    var newPosition = (currentValue * 100f / maxValue) * (_maxHeightProgress / 100f);

    // spriteProgress.transform
    //   .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
    _rectProgress
      .DOSizeDelta(new Vector3(_rectProgress.rect.width, newPosition), _gameSetting.timeGeneralAnimation)
      .SetEase(Ease.OutBounce);
  }

}
