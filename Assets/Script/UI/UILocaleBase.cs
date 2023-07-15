using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;


public abstract class UILocaleBase : MonoBehaviour
{
  [SerializeField] public LocalizedStringTable _localization;
  private VisualElement _box;

  private List<LocalizeObj> _elementList;
  protected GameManager _gameManager => GameManager.Instance;
  protected GameSetting _gameSetting => GameManager.Instance.GameSettings;
  protected AudioManager _audioManager => GameManager.Instance.audioManager;

  public async void Initialize(VisualElement root)
  {
    _box = root;
    await Localize(root);
    Theming(_box);
  }

  public void Theming(VisualElement _root)
  {
    _box = _root;

    UQueryBuilder<VisualElement> builder = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> list = builder.Class("text-primary").ToList();
    foreach (var item in list)
    {
      item.style.color = _gameManager.Theme.colorPrimary;
    }

    UQueryBuilder<VisualElement> builderSecondary = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listSecondary = builderSecondary.Where(t => t.ClassListContains("text-secondary")).ToList();
    foreach (var item in listSecondary)
    {
      item.style.color = _gameManager.Theme.colorSecondary;
    }

    UQueryBuilder<VisualElement> builderDrag = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listDragEl = builderDrag.Class("unity-base-slider__dragger").ToList();
    foreach (var item in listDragEl)
    {
      item.style.backgroundColor = _gameManager.Theme.colorSecondary;
    }

    UQueryBuilder<VisualElement> builderInput = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listInputEl = builderInput.Class("unity-base-text-field__input").ToList();
    foreach (var item in listInputEl)
    {
      item.style.backgroundColor = _gameManager.Theme.colorBgInput;
      item.style.color = _gameManager.Theme.colorTextInput;
    }

    UQueryBuilder<VisualElement> builderPopup = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listPopupEl = builderPopup.Class("unity-base-popup-field__input").ToList();
    foreach (var item in listPopupEl)
    {
      item.style.backgroundColor = _gameManager.Theme.colorBgInput;
      item.style.color = _gameManager.Theme.colorTextInput;
    }
    UQueryBuilder<VisualElement> builderArrow = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listArrowEl = builderArrow.Class("unity-base-popup-field__arrow").ToList();
    foreach (var item in listArrowEl)
    {
      item.style.unityBackgroundImageTintColor = _gameManager.Theme.colorTextInput;
    }

    UQueryBuilder<VisualElement> builderCheck = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listCheckEl = builderCheck.Class("unity-toggle__checkmark").ToList();
    foreach (var item in listCheckEl)
    {
      item.style.backgroundColor = _gameManager.Theme.colorBgInput;
      item.style.unityBackgroundImageTintColor = _gameManager.Theme.colorTextInput;
      item.style.color = _gameManager.Theme.colorTextInput;
    }

    UQueryBuilder<VisualElement> builderBtn = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listBtnEl = builderBtn.Class("btn").ToList();
    foreach (var item in listBtnEl)
    {
      item.style.backgroundColor = _gameManager.Theme.colorBgButton;
    }

    UQueryBuilder<VisualElement> builderLowBtns = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listLowBtns = builderLowBtns.Class("unity-scroller__low-button").ToList();
    foreach (var item in listLowBtns)
    {
      item.style.unityBackgroundImageTintColor = _gameManager.Theme.colorTextInput;
    }

    UQueryBuilder<VisualElement> builderHighBtns = new UQueryBuilder<VisualElement>(_box);
    List<VisualElement> listHighBtns = builderHighBtns.Class("unity-scroller__high-button").ToList();
    foreach (var item in listHighBtns)
    {
      item.style.unityBackgroundImageTintColor = _gameManager.Theme.colorTextInput;
    }
  }

  public async UniTask Localize(VisualElement root)
  {
    await LocalizationSettings.InitializationOperation.Task;

    var op = _localization.GetTableAsync();
    await op.Task;
    _elementList = HelperUI.FindAllTextElements(root);
    OnTableLoaded(op);
  }

  private void OnTableLoaded(AsyncOperationHandle<StringTable> op)
  {
    StringTable table = op.Result;

    foreach (var item in _elementList)
    {
      var entry = op.Result[item.Key];
      if (entry != null)
        item.Element.text = entry.LocalizedValue;
      else
        Debug.LogWarning($"No {op.Result.LocaleIdentifier.Code} translation for key: '{item.Key}'");
    }
  }
}

