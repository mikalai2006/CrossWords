using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ButtonStar : BaseButton
{
  [SerializeField] private TMPro.TextMeshProUGUI _countChars;

  #region UnityMethods
  protected override void Awake()
  {
    configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Star);

    base.Awake();

    StateManager.OnChangeState += SetValue;
  }
  protected override void OnDestroy()
  {
    base.OnDestroy();

    StateManager.OnChangeState -= SetValue;
  }
  #endregion


  public override void SetValue(StateGame state)
  {
    value = state.activeDataGame.hints.GetValueOrDefault(TypeEntity.Star);

    base.SetValue(state);
    _countChars.text = string.Format(
      "{0}--{1}",
      state.activeDataGame.activeLevel.countOpenChars,
      state.activeDataGame.activeLevel.bonusCount.charStar
    );
  }

  // public void CreateStar(DataGame data, StatePerk statePerk)
  // {
  //   // if (data.activeLevel.star <= 0) return;

  //   var potentialGroup = _levelManager.ManagerHiddenWords.GridHelper
  //     .GetGroupNodeChars()
  //     // .OrderBy(t => UnityEngine.Random.value)
  //     .FirstOrDefault();
  //   if (potentialGroup.Value != null && potentialGroup.Value.Count > 0)
  //   {
  //     var node = potentialGroup.Value.First();
  //     if (node != null)
  //     {
  //       // data.activeLevel.star -= 1;
  //       var starEntity = _levelManager.ManagerHiddenWords.AddEntity(node.arrKey, TypeEntity.Star);
  //       if (gameObject != null)
  //       {
  //         // node.StateNode |= StateNode.Entity;
  //         starEntity.RunEffect(_levelManager.ManagerHiddenWords.tilemap.WorldToCell(gameObject.transform.position));
  //       }
  //       else
  //       {
  //         Debug.LogWarning($"Not found {name}");
  //       }
  //     }
  //   }
  // }


  public override void SetValueProgressBar(StateGame state)
  {
    var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)state.activeDataGame.activeLevel.bonusCount.charStar / _gameManager.PlayerSetting.bonusCount.charStar;
    // spriteProgress.transform.localPosition
    //   = new Vector3(spriteProgress.transform.localPosition.x, newPosition);
    spriteProgress.transform
      .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
      .SetEase(Ease.OutBounce);
  }

  public async override void RunHint()
  {
    var node = _levelManager.ManagerHiddenWords.GridHelper.GetRandomNodeWithHiddenChar();

    if (node == null)
    {
      // Show dialog - not found node for entity
      var message = await Helpers.GetLocaledString("notfoundnodehiddenchar");
      var dialog = new DialogProvider(new DataDialog()
      {
        sprite = configEntity.sprite,
        message = message,
        showCancelButton = false
      });

      _gameManager.InputManager.Disable();
      await dialog.ShowAndHide();
      _gameManager.InputManager.Enable();
      _gameManager.ChangeState(GameState.StopEffect);
      return;
    }

    var newEntity = await _levelManager.AddEntity(node.arrKey, TypeEntity.Star, false);

    node.SetHint();

    newEntity.Move(_levelManager.ManagerHiddenWords.tilemap.WorldToCell(gameObject.transform.position), new() { node }, true);

    _stateManager.UseHint(-1, configEntity.typeEntity);
  }


}
