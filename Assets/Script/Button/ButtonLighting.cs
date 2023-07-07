using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class ButtonLighting : BaseButton
{
  #region UnityMethods
  protected override void Awake()
  {
    configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Lighting);

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
    value = state.activeDataGame.hints.GetValueOrDefault(TypeEntity.Lighting);
    base.SetValue(state);
  }

  public override void SetValueProgressBar(StateGame state)
  {
    var newPosition = (progressBasePositionY + 1.2f) + progressBasePositionY - progressBasePositionY * (float)state.activeDataGame.activeLevel.bonusCount.charLighting / _gameManager.PlayerSetting.bonusCount.charLighting;
    spriteProgress.transform
      .DOLocalMoveY(newPosition, _gameSetting.timeGeneralAnimation * 2)
      .SetEase(Ease.OutBounce);
  }

  public async override void RunHint()
  {
    var helper = _levelManager.ManagerHiddenWords.GridHelper;
    var nodeWithHiddenChar = helper.GetAllNodeWithHiddenChar();

    if (nodeWithHiddenChar.Count == 0)
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
    };

    Dictionary<GridNode, List<GridNode>> potentialNodes = new();
    foreach (var node in nodeWithHiddenChar)
    {
      List<GridNode> list = new();

      foreach (var n in nodeWithHiddenChar)
      {
        if (node != n && node.x == n.x && !n.StateNode.HasFlag(StateNode.Open))
        {
          list.Add(n);
        }
      }
      potentialNodes.Add(node, list);
    }

    GridNode nodeStartEffect = potentialNodes.OrderBy(t => t.Value.Count).Last().Key;

    List<GridNode> nodesForEffect = potentialNodes[nodeStartEffect];

    var newEntity = await _levelManager.AddEntity(nodeStartEffect.arrKey, TypeEntity.Lighting, false);

    nodeStartEffect.SetHint();

    newEntity.Move(_levelManager.ManagerHiddenWords.tilemap.WorldToCell(gameObject.transform.position), nodesForEffect, true);

    _stateManager.UseHint(-1, configEntity.typeEntity);
  }
}
