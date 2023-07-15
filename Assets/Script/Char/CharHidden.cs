// using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CharHidden : CharBase
{
  public CharHiddenMB CharGameObject;

  public override void Init(string _char, GridNode node, WordHidden wordHidden)
  {
    base.Init(_char, node, wordHidden);

    OccupiedNode = node;
    OccupiedNode.SetOccupiedChar(this);
  }

  public async UniTask Open(bool runEffect)
  {

    int valueBonusSaveHintLetter;
    _stateManager.dataGame.bonus.TryGetValue(TypeBonus.SaveHintLetter, out valueBonusSaveHintLetter);

    OccupiedNode.SetOpen();

    CharGameObject.Open(runEffect);

    CharGameObject.ChangeTheme();

    // Add coin.
    if (runEffect && (!OccupiedNode.StateNode.HasFlag(StateNode.Hint) || valueBonusSaveHintLetter > 0))
    {
      // play sound.
      _gameManager.audioManager.PlayClipEffect(_gameSetting.Audio.openHiddenChar);

      _stateManager.IncrementCoin(1);

      // _levelManager.CreateLetter(transform.position, _levelManager.buttonFlask.transform.position, CharHidden.CharValue).Forget();
    }

    OccupiedNode.SetHint(false);

    if (runEffect)
    {
      // await OpenNeighbours(runEffect);
    }

    // // open crosswords if exists.
    // foreach (var crossWordItem in OccupiedWord.Crosswords)
    // {
    //   WordHidden crossWord = (WordHidden)crossWordItem.Key;
    //   bool isAlreadyOpenCrossWord = crossWordItem.Value;

    //   if (!isAlreadyOpenCrossWord && crossWord.isOpen)
    //   {
    //     crossWord.AutoOpenWord().Forget();

    //   }
    // }

    await UniTask.Yield();
  }

  public override async UniTask Draw()
  {
    if (_asset.IsValid()) return;

    // Debug.Log($"OccupiedNode.position={new Vector3(OccupiedNode.arrKey.x, OccupiedNode.arrKey.y)}");
    _asset = Addressables.InstantiateAsync(
      _gameSetting.prefabHiddenChar,
      Vector3.zero,
      Quaternion.identity,
      _levelManager.ManagerHiddenWords.tilemap.transform
      );
    var newObj = await _asset.Task;

    newObj.transform.localPosition = new Vector3(OccupiedNode.arrKey.x, OccupiedNode.arrKey.y);

    var newChar = newObj.GetComponent<CharHiddenMB>();
    newChar.Init(this);

    CharGameObject = newChar;
    if (OccupiedNode.StateNode.HasFlag(StateNode.Open))
    {
      Open(false).Forget();
    }
    if (_levelManager.ManagerHiddenWords.OpenChars.ContainsKey(OccupiedNode.arrKey))
    {
      CharGameObject.ShowCharAsHint(false).Forget();
    }
  }

  public override void Destroy()
  {
    base.Destroy();

    if (_asset.IsValid()) Addressables.ReleaseInstance(_asset);
  }
}