using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Coin : BaseEntity
{
  #region Unity methods
  protected override void Awake()
  {
    configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Coin);

    base.Awake();
  }
  #endregion

  public override void Init(GridNode node, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> asset, bool asBonus)
  {
    base.Init(node, asset, asBonus);

    // node.SetOccupiedEntity(this);

    SetColor(_gameManager.Theme.entityColor);
  }

  public override void InitStandalone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> asset)
  {
    base.InitStandalone(asset);

    spriteBg.color = _gameManager.Theme.bgColor;
    SetColor(_gameManager.Theme.entityColor);
  }

  public override void SetColor(Color color)
  {
    // base.SetColor(color);
    spriteRenderer.color = color;
  }

  public override void AddCoins(int count = 1)
  {
    _stateManager.IncrementCoin(count);

    // RunActivateEffect();
    // _levelManager.topSide.AddCoin().Forget();

    Destroy(gameObject);
  }
  public override void AddTotalCoins(int count = 1)
  {
    _stateManager.IncrementTotalCoin(count);

    RunActivateEffect();
    // _levelManager.topSide.AddCoin().Forget();

    Destroy(gameObject);
  }
  public async override UniTask Run()
  {
    await base.Run();

    // RunEffect();

    Debug.Log("TODO Run coin");
  }


  public override void Collect()
  {
    var positionFrom = initPosition;
    Vector3 positionTo = _levelManager.ManagerHiddenWords.tilemap.WorldToCell(_levelManager.buttonFrequency.transform.position);
    RunMoveEffect();

    Vector3[] waypoints = {
          positionFrom,
          positionFrom - new Vector3(Random.Range(0.5f,1.5f), Random.Range(0.5f,2.5f)),
          positionTo + new Vector3(Random.Range(0,1), Random.Range(0,1)),
          positionTo,
        };
    transform
      .DOLocalPath(waypoints, 1f, PathType.CatmullRom)
      .From(true)
      .SetEase(Ease.OutCubic)
      .OnComplete(() =>
      {
        _stateManager.IncrementCoin(1);

        Destroy(gameObject);

        base.Collect();
      });

  }
}
