using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Loader;
using UnityEngine;
using UnityEngine.UIElements;

public class UIShop : UILocaleBase
{
  [SerializeField] private UIDocument _uiDoc;
  [SerializeField] private VisualTreeAsset _shopItem;
  [SerializeField] private VisualTreeAsset _userBalanceItem;
  [SerializeField] private VisualElement _root;
  private GameObject _enviromnment;
  [SerializeField] private VisualElement _listItems;
  private TaskCompletionSource<DataDialogResult> _processCompletionSource;
  private DataDialogResult _result;

  private void Awake()
  {

    LevelManager.OnInitLevel += Hide;
    GameManager.OnChangeTheme += ChangeTheme;
  }

  private void OnDestroy()
  {

    LevelManager.OnInitLevel -= Hide;
    GameManager.OnChangeTheme -= ChangeTheme;
  }

  public virtual void Start()
  {
    _root = _uiDoc.rootVisualElement;

    var exitBtn = _root.Q<Button>("ExitBtn");
    exitBtn.clickable.clicked += () => ClickClose();

    _listItems = _root.Q<VisualElement>("ListItems");

    ChangeTheme();

  }


  private void ChangeTheme()
  {
    _root.Q<VisualElement>("ShopBlokWrapper").style.backgroundColor = new StyleColor(_gameManager.Theme.bgColor);

    FillItems();
    DrawBalance();

    base.Initialize(_root);
  }


  private void Hide()
  {
    _root.style.display = DisplayStyle.None;
  }

  private void DrawBalance()
  {
    var wrapperBalance = _root.Q<VisualElement>("WrapperBalance");
    wrapperBalance.Clear();
    var blokBalance = _userBalanceItem.Instantiate();
    var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
    blokBalance.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(configCoin.sprite);
    blokBalance.Q<VisualElement>("Img").style.unityBackgroundImageTintColor
      = _gameManager.Theme.colorPrimary;
    blokBalance.Q<Label>("Coin").text = string.Format("{0}", _gameManager.StateManager.stateGame.coins);
    wrapperBalance.Add(blokBalance);

    base.Theming(wrapperBalance);
  }

  private async void FillItems()
  {
    var configCoin = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);
    _listItems.Clear();

    foreach (var item in _gameSetting.ShopItems)
    {
      var blokItem = _shopItem.Instantiate();
      blokItem.style.flexGrow = 1;
      blokItem.AddToClassList("w-50");
      blokItem.Q<VisualElement>("ShopItem").style.backgroundColor = _gameManager.Theme.bgColor;
      // blokItem.Q<VisualElement>("ImgWrapper").style.backgroundColor = _gameSettings.Theme.colorPrimary;
      // blokItem.Q<VisualElement>("ImgWrapper").Q<VisualElement>("Img").style.unityBackgroundImageTintColor = _gameSettings.Theme.bgColor;
      var title = await Helpers.GetLocaledString(item.entity.text.title);
      blokItem.Q<Label>("Name").text = title;

      var textCost = await Helpers.GetLocalizedPluralString(
          "coin",
           new Dictionary<string, object> {
            {"count",  item.cost},
          }
        );
      blokItem.Q<Label>("Price").text = string.Format("{0}", item.cost);
      blokItem.Q<VisualElement>("CoinSmallImg").style.backgroundImage = new StyleBackground(configCoin.sprite);
      blokItem.Q<VisualElement>("CoinSmallImg").style.unityBackgroundImageTintColor = _gameManager.Theme.colorSecondary;

      // var description = await Helpers.GetLocaledString(item.entity.text.description);
      // blokItem.Q<Label>("Description").text = string.Format(
      //   "{0}",
      //   // textCountWords,
      //   description
      //   );

      // Button info.
      var buttonInfo = blokItem.Q<Button>("InfoBtn");
      buttonInfo.Q<VisualElement>("InfoImg").style.backgroundImage = new StyleBackground(_gameSetting.spriteInfo);
      buttonInfo.Q<VisualElement>("InfoImg").style.unityBackgroundImageTintColor
        = new StyleColor(_gameManager.Theme.colorSecondary);
      buttonInfo.clickable.clicked += async () =>
        {
          AudioManager.Instance.Click();

          _gameManager.InputManager.Disable();

          var message = await Helpers.GetLocaledString(item.entity.text.description);
          var dialog = new DialogProvider(new DataDialog()
          {
            sprite = item.entity.sprite,
            title = title,
            message = message
          });

          await dialog.ShowAndHide();
          _gameManager.InputManager.Enable();

        };

      blokItem.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(item.entity.sprite);
      blokItem.Q<VisualElement>("Img").style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.entityColor);


      // Button buy for coin.
      var buttonForCoin = blokItem.Q<Button>("Buy");
      buttonForCoin.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(_gameSetting.spriteBuy);
      buttonForCoin.Q<VisualElement>("Img").style.unityBackgroundImageTintColor
        = new StyleColor(_gameManager.Theme.entityColor);
      buttonForCoin.clickable.clicked += async () =>
      {
        await BuyEnity(item);
      };

      var textPriceCoin = await Helpers.GetLocalizedPluralString(
          "costitemtext",
           new Dictionary<string, object> {
            {"count",  item.count},
            {"cost", item.cost},
            {"name", title},
          }
        );
      var textButtonForCoin = buttonForCoin.Q<Label>("Text");
      if (_gameManager.StateManager.stateGame.coins < item.cost)
      {
        buttonForCoin.SetEnabled(false);
        textButtonForCoin.text = await Helpers.GetLocaledString("lackscoin");
      }
      else
      {
        buttonForCoin.SetEnabled(true);
        textButtonForCoin.text = await Helpers.GetLocalizedPluralString(
          "buyforcoin",
           new Dictionary<string, object> {
            {"cost",  item.cost},
            {"count", item.count}
          }
        );
      }

      // Button buy for coin.
      var buttonForAdv = blokItem.Q<Button>("Adv");
      buttonForAdv.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(_gameSetting.spriteAdv);
      buttonForAdv.Q<VisualElement>("Img").style.unityBackgroundImageTintColor
        = new StyleColor(_gameManager.Theme.entityColor);
      // buttonForAdv.Q<Label>("Text").text = await Helpers.GetLocalizedPluralString(
      //   "buyadv",
      //    new Dictionary<string, object> {
      //       {"count", item.count}
      //   }
      // );
      buttonForAdv.clickable.clicked += async () =>
        {
          // Buy for adv.
          AudioManager.Instance.Click();

          var messageConfirm = await Helpers.GetLocalizedPluralString(
            "buyadv",
            new Dictionary<string, string> {
                {"name", title}
            }
          );
          var dialogConfirm = new DialogProvider(new DataDialog()
          {
            // title = title,
            message = messageConfirm,
            showCancelButton = true
          });

          var result = await dialogConfirm.ShowAndHide();
          if (result.isOk)
          {
            var addedHint = new ShopAdvBuyItem<TypeEntity>()
            {
              typeItem = item.entity.typeEntity,
              count = item.count,
            };
            _gameManager.AdManager.AddHintByAdv(addedHint);
          }
        };

      _listItems.Add(blokItem);
    }


    foreach (var item in _gameSetting.ShopItemsBonus)
    {
      var blokItem = _shopItem.Instantiate();
      blokItem.style.flexGrow = 1;
      blokItem.AddToClassList("w-50");
      blokItem.Q<VisualElement>("ShopItem").style.backgroundColor = _gameManager.Theme.bgColor;
      var title = await Helpers.GetLocaledString(item.entity.text.title);
      blokItem.Q<Label>("Name").text = title;

      var textCost = await Helpers.GetLocalizedPluralString(
          "coin",
           new Dictionary<string, object> {
            {"count",  item.cost},
          }
        );

      blokItem.Q<Label>("Price").text = string.Format("{0}", item.cost);
      blokItem.Q<VisualElement>("CoinSmallImg").style.backgroundImage = new StyleBackground(configCoin.sprite);
      blokItem.Q<VisualElement>("CoinSmallImg").style.unityBackgroundImageTintColor = _gameManager.Theme.colorSecondary;
      // var description = await Helpers.GetLocaledString(item.entity.text.description);
      // blokItem.Q<Label>("Description").text = string.Format(
      //   "{0}",
      //   // textCountWords,
      //   description
      //   );

      // Button info.
      var buttonInfo = blokItem.Q<Button>("InfoBtn");
      buttonInfo.Q<VisualElement>("InfoImg").style.backgroundImage = new StyleBackground(_gameSetting.spriteInfo);
      buttonInfo.Q<VisualElement>("InfoImg").style.unityBackgroundImageTintColor
        = new StyleColor(_gameManager.Theme.colorSecondary);
      buttonInfo.clickable.clicked += async () =>
        {
          AudioManager.Instance.Click();

          _gameManager.InputManager.Disable();

          var message = await Helpers.GetLocaledString(item.entity.text.description);
          var dialog = new DialogProvider(new DataDialog()
          {
            sprite = item.entity.sprite,
            title = title,
            message = message
          });

          await dialog.ShowAndHide();
          _gameManager.InputManager.Enable();

        };

      blokItem.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(item.entity.sprite);
      blokItem.Q<VisualElement>("Img").style.unityBackgroundImageTintColor = new StyleColor(_gameManager.Theme.entityColor);

      // Button buy for coin.
      var buttonForCoin = blokItem.Q<Button>("Buy");
      buttonForCoin.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(_gameSetting.spriteBuy);
      buttonForCoin.Q<VisualElement>("Img").style.unityBackgroundImageTintColor
        = new StyleColor(_gameManager.Theme.entityColor);
      buttonForCoin.clickable.clicked += async () =>
      {
        await BuyBonus(item);
      };

      var textPriceCoin = await Helpers.GetLocalizedPluralString(
          "costitemtext",
           new Dictionary<string, object> {
            {"count",  item.count},
            {"cost", item.cost},
            {"name", title},
          }
        );
      var textButtonForCoin = buttonForCoin.Q<Label>("Text");
      if (_gameManager.StateManager.stateGame.coins < item.cost)
      {
        buttonForCoin.SetEnabled(false);
        textButtonForCoin.text = await Helpers.GetLocaledString("lackscoin");
      }
      else
      {
        buttonForCoin.SetEnabled(true);
        textButtonForCoin.text = await Helpers.GetLocalizedPluralString(
          "buyforcoin",
           new Dictionary<string, object> {
            {"cost",  item.cost},
            {"count", item.count}
          }
        );
      }

      var buttonForAdv = blokItem.Q<Button>("Adv");
      buttonForAdv.Q<VisualElement>("Img").style.backgroundImage = new StyleBackground(_gameSetting.spriteAdv);
      buttonForAdv.Q<VisualElement>("Img").style.unityBackgroundImageTintColor
        = new StyleColor(_gameManager.Theme.entityColor);
      // buttonForAdv.Q<Label>("Text").text = await Helpers.GetLocalizedPluralString(
      //   "buyadv",
      //    new Dictionary<string, object> {
      //       {"count", item.count}
      //   }
      // );
      buttonForAdv.clickable.clicked += async () =>
        {
          // Buy bonus for adv.
          AudioManager.Instance.Click();
          var messageConfirm = await Helpers.GetLocalizedPluralString(
            "buyadv",
            new Dictionary<string, string> {
              {"name", title}
            }
          );
          var dialogConfirm = new DialogProvider(new DataDialog()
          {
            // title = title,
            message = messageConfirm,
            showCancelButton = true
          });

          var result = await dialogConfirm.ShowAndHide();
          if (result.isOk)
          {
            var addedBonus = new ShopAdvBuyItem<TypeBonus>()
            {
              typeItem = item.entity.typeBonus,
              count = item.count,
            };
            _gameManager.AdManager.AddBonusByAdv(addedBonus);
          };
        };

      _listItems.Add(blokItem);
    }

  }

  private async UniTask BuyBonus(ShopItem<GameBonus> item)
  {
    // TODO Buy for coin.
    AudioManager.Instance.Click();
    _gameManager.InputManager.Disable();


    DataDialogResult resultConfirm = new()
    {
      isOk = true,
    };

    // DoDialog.
    if (_gameManager.AppInfo.setting.dod)
    {
      _gameManager.InputManager.Disable();

      var nameConfirm = await Helpers.GetLocaledString(item.entity.text.title);
      var messageConfirm = await Helpers.GetLocalizedPluralString("confirm_buybonus", new Dictionary<string, object>() {
            {"name", nameConfirm},
            {"count", item.count},
            {"count2", item.cost * item.count}
          });
      var title = await Helpers.GetLocaledString("confirm_title");
      var dialogConfirm = new DialogProvider(new DataDialog()
      {
        title = title,
        sprite = item.entity.sprite,
        message = messageConfirm,
        showCancelButton = true
      });

      resultConfirm = await dialogConfirm.ShowAndHide();
      _gameManager.InputManager.Enable();
    }

    if (resultConfirm.isOk)
    {

      _gameManager.StateManager.BuyBonus(item);

      var message = await Helpers.GetLocalizedPluralString("successgetbonus", new Dictionary<string, int>() {
      {"count", item.count}
    });
      var dialog = new DialogProvider(new DataDialog()
      {
        sprite = item.entity.sprite,
        message = message,
        showCancelButton = true
      });

      await dialog.ShowAndHide();
    }

    DrawBalance();

    _gameManager.InputManager.Enable();
  }

  private async UniTask BuyEnity(ShopItem<GameEntity> item)
  {
    // TODO Buy for coin.
    AudioManager.Instance.Click();
    _gameManager.InputManager.Disable();

    DataDialogResult resultConfirm = new()
    {
      isOk = true,
    };

    // DoDialog.
    if (_gameManager.AppInfo.setting.dod)
    {
      _gameManager.InputManager.Disable();

      var nameConfirm = await Helpers.GetLocaledString(item.entity.text.title);
      var messageConfirm = await Helpers.GetLocalizedPluralString("confirm_runbuyhint", new Dictionary<string, object>() {
            {"name", nameConfirm},
            {"count", item.count},
            {"count2", item.cost * item.count}
          });
      var title = await Helpers.GetLocaledString("confirm_title");
      var dialogConfirm = new DialogProvider(new DataDialog()
      {
        title = title,
        sprite = item.entity.sprite,
        message = messageConfirm,
        showCancelButton = true
      });

      resultConfirm = await dialogConfirm.ShowAndHide();
      _gameManager.InputManager.Enable();
    }

    if (resultConfirm.isOk)
    {
      _gameManager.StateManager.BuyHint(item);

      var message = await Helpers.GetLocalizedPluralString("successbuy", new Dictionary<string, int>() {
            {"count", item.count}
          });
      var dialog = new DialogProvider(new DataDialog()
      {
        sprite = item.entity.sprite,
        message = message,
        showCancelButton = true
      });

      await dialog.ShowAndHide();
    }

    DrawBalance();
    _gameManager.InputManager.Enable();
  }

  private void ClickClose()
  {
    AudioManager.Instance.Click();
    _result.isOk = false;
    _processCompletionSource.SetResult(_result);
  }

  public async UniTask<DataDialogResult> ProcessAction()
  {

    _processCompletionSource = new TaskCompletionSource<DataDialogResult>();

    return await _processCompletionSource.Task;
  }
}
