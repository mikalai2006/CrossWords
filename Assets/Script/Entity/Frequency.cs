using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Frequency : BaseEntity
{
  #region Unity methods
  protected override void Awake()
  {
    configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Frequency);

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

    List<UniTask> tasks = new();
    foreach (var node in nodesForCascade)
    {
      var newEntity = await _levelManager.AddEntity(node.arrKey, configEntity.typeEntity, false);

      tasks.Add(newEntity.RunCascadeEffect(gameObject.transform.position));
    }

    await UniTask.WhenAll(tasks);

    nodesForCascade.Clear();

    _gameManager.ChangeState(GameState.StopEffect);

    Destroy(gameObject);
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
        _stateManager.UseHint(1, TypeEntity.Frequency);

        Destroy(gameObject);

        base.Collect();
      });

  }
}
