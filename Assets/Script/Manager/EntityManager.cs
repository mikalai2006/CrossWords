using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
  private GameManager _gameManager => GameManager.Instance;
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private StateManager _stateManager => GameManager.Instance.StateManager;
  private void Awake()
  {
    DataManager.OnAddHintExtern += AddHint;
    DataManager.OnAddBonusExtern += AddBonus;
  }

  private void OnDestroy()
  {
    DataManager.OnAddHintExtern -= AddHint;
    DataManager.OnAddBonusExtern -= AddBonus;

  }

  private async void AddBonus(ShopAdvBuyItem<TypeBonus> item)
  {
    // var title = await Helpers.GetLocaledString("dailycoins_t");
    var message = await Helpers.GetLocalizedPluralString("successgetbonus", new Dictionary<string, object>() {
      { "count", item.count },
    });
    var configBonus = _gameManager.ResourceSystem.GetAllBonus().Find(t => t.typeBonus == item.typeItem);
    var bonuses = new List<ShopItem<GameBonus>>() {
        new ShopItem<GameBonus>(){
        entity = configBonus,
        cost = 0,
        count = item.count
        },
    };

    var dialog = new DialogProvider(new DataDialog()
    {
      //   title = title,
      message = message,
      bonuses = bonuses,
      showCancelButton = false,
    });

    _gameManager.InputManager.Disable();
    var result = await dialog.ShowAndHide();
    if (result.isOk)
    {
      foreach (var entityItem in bonuses)
      {
        _gameManager.StateManager.BuyBonus(entityItem);
      }
    }
    _gameManager.InputManager.Enable();
  }

  private async void AddHint(ShopAdvBuyItem<TypeEntity> item)
  {
    // var title = await Helpers.GetLocaledString("dailycoins_t");
    var message = await Helpers.GetLocalizedPluralString("successbuy", new Dictionary<string, object>() {
      { "count", item.count },
    });
    var configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == item.typeItem);
    var entities = new List<ShopItem<GameEntity>>() {
        new ShopItem<GameEntity>(){
        entity = configEntity,
        cost = 0,
        count = item.count
        },
    };

    var dialog = new DialogProvider(new DataDialog()
    {
      //   title = title,
      message = message,
      entities = entities,
      showCancelButton = false,
    });

    _gameManager.InputManager.Disable();
    var result = await dialog.ShowAndHide();
    if (result.isOk)
    {
      foreach (var entityItem in entities)
      {
        if (entityItem.entity.typeEntity == TypeEntity.Coin)
        {
          _gameManager.StateManager.IncrementTotalCoin(entityItem.count);
        }
        else
        {
          _gameManager.StateManager.BuyHint(entityItem);
        }
      }
    }
    _gameManager.InputManager.Enable();
  }

}
