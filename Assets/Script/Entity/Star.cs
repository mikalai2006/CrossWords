using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Star : BaseEntity
{
  #region Unity methods
  protected override void Awake()
  {
    configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Star);

    base.Awake();
  }
  #endregion


  // public override void Init(GridNode node, UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> asset, bool asBonus)
  // {
  //   base.Init(node, asset, asBonus);

  //   node.SetOccupiedEntity(this);
  // }

  public async override UniTask Run()
  {
    await base.Run();

    RunOpenEffect();

    // var nodes = _levelManager.ManagerHiddenWords.GridHelper.GetEqualsHiddenNeighbours();
    // foreach (var node in nodes)
    // {
    //   if (node != null)
    //   {
    //     node.OccupiedChar.ShowCharAsHint(true).Forget();
    //     _levelManager.ManagerHiddenWords.AddOpenChar(node.OccupiedChar);
    //     node.SetHint();
    //   }
    // }

    _gameManager.ChangeState(GameState.StopEffect);

    Destroy(gameObject);
  }

  public override void Collect()
  {
    var positionFrom = initPosition;
    Vector3 positionTo = _levelManager.ManagerHiddenWords.tilemap.WorldToCell(_levelManager.buttonStar.transform.position);
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
        _stateManager.UseHint(1, TypeEntity.Star);

        Destroy(gameObject);

        base.Collect();
      });

  }
}
