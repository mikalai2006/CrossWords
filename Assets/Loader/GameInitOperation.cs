using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Loader
{
  public class GameInitOperation : ILoadingOperation
  {
    public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
    {
      var _gameManager = GameManager.Instance;

      await LocalizationSettings.InitializationOperation.Task;

      onProgress?.Invoke(0.1f);

      var t = await Helpers.GetLocaledString("loading");
      onSetNotify?.Invoke(t);
      var environment = await _gameManager.AssetProvider.LoadSceneAdditive(Constants.Scenes.SCENE_GAME);
      var rootObjects = environment.Scene.GetRootGameObjects();

      LevelManager LevelManager = GameObject.FindGameObjectWithTag("LevelManager")?.GetComponent<LevelManager>();

      if (LevelManager != null)
      {
        _gameManager.InitGameLevel(LevelManager, environment);
        // GameManager.Instance.LevelManager = LevelManager;
        // GameManager.Instance.environment = environment;
        // // LevelManager.CreateLevel();
      }


      onProgress?.Invoke(1f);
    }
  }
}