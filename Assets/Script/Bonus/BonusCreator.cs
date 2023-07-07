// using System.Collections.Generic;
// using DG.Tweening;

// public class BonusCreator : BaseBonus
// {
//   private int countOpenChar;


//   #region UnityMethods
//   protected override void Awake()
//   {
//     configBonus = _gameManager.ResourceSystem.GetAllBonus().Find(t => t.typeBonus == TypeBonus.Creator);

//     base.Awake();
//   }
//   #endregion

//   public override void SetValue(StateGame state)
//   {
//     int v = 0;

//     // formal run object(for show object).
//     value = 1;

//     if (state.activeDataGame.activeLevel.bonusCount.charInOrder > 0)
//     {
//       v = state.activeDataGame.activeLevel.bonusCount.charInOrder;
//     }
//     countOpenChar = Helpers.HasValueDouble((double)v) ? v : 0;

//     // counterText.text = string.Format("{0}", value);

//     base.SetValue(state);

//     // if (countOpenChar == 0) return;

//     SetValueProgressBar(state);

//     var maxValue = _gameManager.PlayerSetting.bonusCount.charInOrder;

//     if (countOpenChar >= maxValue)
//     {
//       // TODO Create rand bonus.
//     }
//   }

//   public override void SetValueProgressBar(StateGame state)
//   {
//     var maxValue = _gameManager.PlayerSetting.bonusCount.charInOrder;

//     var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)countOpenChar / maxValue;
//     UnityEngine.Debug.Log($"countOpenChar={countOpenChar}| maxValue={maxValue} | newPosition={newPosition}");

//     spriteProgress.transform
//       .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
//       .SetEase(Ease.OutBounce);
//   }

// }
