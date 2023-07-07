// using UnityEngine;
// using UnityEngine.UIElements;

// public class UIDialogBaseWindow : UILocaleBase
// {
//   [SerializeField] private UIDocument _uiDoc;
//   public UIDocument DialogApp => _uiDoc;
//   [SerializeField] private VisualTreeAsset _templateInsertBlok;
//   private readonly string _nameHeaderLabel = "HeaderDialog";
//   private readonly string _nameGeneralBlok = "GeneralBlok";
//   protected Label Title;
//   private VisualElement _generalBlok;
//   protected VisualElement Panel;
//   protected VisualElement root;
//   protected GameSetting GameSetting;

//   public virtual void Start()
//   {
//     root = DialogApp.rootVisualElement;

//     GameSetting = GameManager.Instance.GameSetting;

//     Title = root.Q<Label>(_nameHeaderLabel);
//     _generalBlok = root.Q<VisualElement>(_nameGeneralBlok);

//     Panel = root.Q<VisualElement>("Panel");
//     Panel.AddToClassList("w-50");

//     var panelBlok = root.Q<VisualElement>("PanelBlok");
//     panelBlok.style.flexGrow = 1;

//     VisualElement docDialogBlok = _templateInsertBlok.Instantiate();
//     docDialogBlok.style.flexGrow = 1;
//     _generalBlok.Clear();
//     _generalBlok.Add(docDialogBlok);
//   }

//   protected void Init()
//   {

//     base.Localize(root);
//   }
// }

