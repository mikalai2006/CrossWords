using Cysharp.Threading.Tasks;
using UnityEngine;

public class Letter : BaseEntity
{
  #region Unity methods
  protected override void Awake()
  {
    configEntity = _gameManager.ResourceSystem.GetAllEntity().Find(t => t.typeEntity == TypeEntity.Letter);

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

  public override void AddLetter(char _char)
  {
    // _stateManager.IncrementCoin(1);

    _stateManager.OpenCharHiddenWord(_char);

    _levelManager.buttonFlask.AddChar().Forget();
    // _stateManager.OpenCharHiddenWord(_char);
    Destroy(gameObject);
  }

  public async override UniTask Run()
  {
    await base.Run();

    // RunEffect();

    Debug.Log("TODO Run coin");
  }
}
