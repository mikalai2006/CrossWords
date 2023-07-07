using System.Collections.Generic;
using DG.Tweening;

public class BonusIndex : BaseBonus
{
  #region UnityMethods
  protected override void Awake()
  {
    configBonus = _gameManager.ResourceSystem.GetAllBonus().Find(t => t.typeBonus == TypeBonus.Index);

    base.Awake();

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

    counterText.text = string.Format("x{0}", value + 1);

    base.SetValue(state);

    // if (value == 0) return;

    SetValueProgressBar(state);
  }

  public override void SetValueProgressBar(StateGame state)
  {
    // base.SetValueProgressBar(state);
    var maxValue = _gameManager.PlayerSetting.bonusCount.wordInOrder;
    var currentValue = state.activeDataGame.activeLevel.bonusCount.wordInOrder;

    var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)currentValue / maxValue;

    spriteProgress.transform
      .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
      .SetEase(Ease.OutBounce);
  }

}
