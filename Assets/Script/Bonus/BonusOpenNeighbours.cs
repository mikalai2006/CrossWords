using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class BonusOpenNeighbours : BaseBonus
{
  #region UnityMethods
  protected override void Awake()
  {
    configBonus = _gameManager.ResourceSystem.GetAllBonus().Find(t => t.typeBonus == TypeBonus.OpenNeighbours);
    // order.sortingOrder = 25;

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
    value = state.activeDataGame.bonus.GetValueOrDefault(TypeBonus.OpenNeighbours);

    // counterText.text = string.Format("{0}", value);

    base.SetValue(state);

    if (value == 0) return;

    // SetValueProgressBar(state);
  }

  // public override void SetValueProgressBar(StateGame state)
  // {
  //   var valueBonus = state.activeDataGame.bonus.Where(t => t.Key == configBonus.typeBonus);
  //   if (valueBonus != null)
  //   {

  //     var val = valueBonus.First().Value;
  //     var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)val / configBonus.value;

  //     spriteProgress.transform
  //       .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
  //       .SetEase(Ease.OutBounce);
  //   }
  // }

}
