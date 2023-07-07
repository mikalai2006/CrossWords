using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class UIApp : UILocaleBase
{
  // [SerializeField] private UIDocument _uiMenuApp;
  // [SerializeField] private UIDocument _uiSettings;
  private GameSetting GameSetting;
  private GameObject _enviromnment;

  public virtual void Start()
  {

    // base.Localize(_uiDoc.rootVisualElement);
  }

  private void OnDestroy()
  {
    Addressables.Release(_enviromnment);
  }

  public void Init(GameObject environment)
  {
    _enviromnment = environment;
  }
}
