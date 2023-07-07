using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class TopSide : MonoBehaviour
{
  // private LevelManager _levelManager => GameManager.Instance.LevelManager;
  private GameManager _gameManager => GameManager.Instance;
  // private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  // private Vector3 _initialScaleSpriteCoin;
  // private Vector3 _initialPositionSpriteCoin;
  // private Vector3 _initialScaleSpriteRate;
  // private Vector3 _initialPositionSpriteRate;
  // [SerializeField] private TMPro.TextMeshProUGUI _statusText;
  // [SerializeField] private Image _spriteRate;
  // [SerializeField] private TMPro.TextMeshProUGUI _rate;
  // [SerializeField] private Image _spriteCoin;
  // [SerializeField] public GameObject coinObject;
  // [SerializeField] public GameObject targetTotalCoinObject;
  [SerializeField] public GameObject bonusObject;
  // [SerializeField] private TMPro.TextMeshProUGUI _coins;
  [SerializeField] private Image _imageGridBg;
  [SerializeField] private Image _imageBonusesBg;
  [SerializeField] private List<BaseBonus> bonusObjects;
  public Dictionary<TypeBonus, BaseBonus> Bonuses = new();
  // [SerializeField] private GridLayoutGroup layoutGroupBonus;
  // public Vector3 spriteCoinPosition => _spriteCoin.gameObject.transform.position;

  private void Awake()
  {
    // _initialScaleSpriteRate = _spriteRate.transform.localScale;
    // _initialPositionSpriteRate = _spriteRate.transform.position;
    // _initialScaleSpriteCoin = _spriteCoin.transform.localScale;
    // _initialPositionSpriteCoin = _spriteCoin.transform.position;

    ChangeTheme();

    // var configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Letter);
    // _spriteCoin.sprite = configEntity.sprite;
    // _spriteRate.sprite = _gameManager.GameSettings.spriteRate;


    var configsAllEntities = _gameManager.ResourceSystem.GetAllBonus();
    List<UniTask> tasks = new();
    for (int i = 0; i < configsAllEntities.Count; i++)
    {
      var item = configsAllEntities[i];
      tasks.Add(AddBonus(item.typeBonus));
    }
    UniTask.WhenAll(tasks);
    // foreach (var item in bonusObjects)
    // {
    //   Bonuses.Add(item.Config.typeBonus, item);
    // }

    // StateManager.OnChangeState += SetValue;
    GameManager.OnChangeTheme += ChangeTheme;
  }
  private void OnDestroy()
  {
    // StateManager.OnChangeState -= SetValue;
    GameManager.OnChangeTheme -= ChangeTheme;
  }

  private void ChangeTheme()
  {
    _imageGridBg.color = _gameManager.Theme.colorBgGrid;
    _imageBonusesBg.color = _gameManager.Theme.colorBgGrid;
    // _statusText.color = _gameManager.Theme.colorPrimary;
    // _rate.color = _gameManager.Theme.colorPrimary;
    // _coins.color = _gameManager.Theme.colorPrimary;
    // _spriteRate.color = _gameManager.Theme.colorPrimary;
    // _spriteCoin.color = _gameManager.Theme.colorPrimary;
  }

  public async UniTask AddBonus(TypeBonus typeBonus)
  {
    if (Bonuses.ContainsKey(typeBonus)) return;

    var configsAllEntities = _gameManager.ResourceSystem.GetAllBonus();
    GameBonus bonusConfig = configsAllEntities.Find(t => t.typeBonus == typeBonus);

    var asset = Addressables.InstantiateAsync(
      bonusConfig.prefab,
      Vector3.zero,
      Quaternion.identity,
      bonusObject.transform
      );
    var newObj = await asset.Task;
    // newObj.transform.localPosition = new Vector3(-Bonuses.Count / 2f + .5f, 0, 0);

    var newBonus = newObj.GetComponent<BaseBonus>();

    newBonus.Init(asset);

    Bonuses.Add(typeBonus, newBonus);
  }

  // public async UniTask AddCoin()
  // {
  //   Vector3 upScale = new Vector3(1.5f, 1.5f, 0);

  //   float elapsedTime = 0f;
  //   float duration = .2f;
  //   float startTime = Time.time;

  //   while (elapsedTime < duration)
  //   {
  //     float progress = (Time.time - startTime) / duration;
  //     _spriteCoin.transform.localScale = Vector3.Lerp(_initialScaleSpriteCoin, upScale, progress);
  //     await UniTask.Yield();
  //     elapsedTime += Time.deltaTime;
  //   }

  //   SetDefault();
  // }

  // public async void SetValue(StateGame state)
  // {
  //   _rate.text = state.activeDataGame.rate.ToString();

  //   _coins.text = state.activeDataGame.activeLevel.coins.ToString();

  //   string status = await Helpers.GetLocaledString(_gameManager.PlayerSetting.text.title);

  //   string name = string.IsNullOrEmpty(_gameManager.AppInfo.UserInfo.name)
  //     ? await Helpers.GetLocaledString(_gameManager.GameSettings.noName.title)
  //     : _gameManager.AppInfo.UserInfo.name;

  //   _statusText.text = string.Format("{0} <size=.2>{1}</size>", name, status);
  // }

  // private void SetDefault()
  // {
  //   _spriteRate.transform.localScale = _initialScaleSpriteRate;
  //   _spriteRate.transform.position = _initialPositionSpriteRate;
  //   _spriteCoin.transform.localScale = _initialScaleSpriteCoin;
  //   _spriteCoin.transform.position = _initialPositionSpriteCoin;
  // }
}
