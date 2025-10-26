using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParamsUI.UITK
{
    public sealed class ParamWindow : MonoBehaviour
    {
        [SerializeField] UIDocument _uiDocument;
        bool _isVisible = true;
        public ParamCatalog Catalog { get; set; }

        readonly List<IControlBuilder> _builders = new();
        ButtonBuilder _buttonBuilder;

        void Awake()
        {
            if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

            _buttonBuilder = new ButtonBuilder(FindCommandByKey);
            _builders.Add(new FloatSliderBuilder());
            _builders.Add(new IntFieldBuilder());
            _builders.Add(new BoolToggleBuilder());
            _builders.Add(new EnumDropdownBuilder());
            _builders.Add(new FloatFieldBuilder());
            _builders.Add(new VectorLikeFieldBuilder());
        }

        CommandDef FindCommandByKey(string key) => Catalog?.AllCommands.FirstOrDefault(c => c.Key == key);

        void OnEnable()
        {
            Rebuild();
            ApplyVisibility();
        }

        public void Rebuild()
        {
            var root = _uiDocument.rootVisualElement;
            root.Clear();
            if (Catalog == null)
            {
                ApplyVisibility();
                return;
            }

            ApplyStyles(root); // <- ver abajo
            ApplyVisibility();

            // Header
            var header = new VisualElement();
            header.AddToClassList("params-header");
            var search = new TextField { label = "Search" };
            search.AddToClassList("params-search");
            var reset = new Button(() => ResetVisible(null)) { text = "Reset Section" };
            reset.AddToClassList("params-reset");
            header.Add(search);
            header.Add(reset);
            root.Add(header);

            // Tabs (Toolbar) + Pages
            var tabsBar = new Toolbar();
            tabsBar.AddToClassList("params-tabs");
            var pages = new VisualElement();
            pages.AddToClassList("params-pages");

            root.Add(tabsBar);
            root.Add(pages);

            // Agrupar por grupo → una página por pestaña
            var groups = Catalog.AllParams
                .GroupBy(p => p.GroupPath ?? "")
                .OrderBy(g => g.Key);

            var pageByKey = new Dictionary<string, VisualElement>();
            var toggles = new List<ToolbarToggle>();

            foreach (var g in groups)
            {
                string key = string.IsNullOrWhiteSpace(g.Key) ? "(Root)" : g.Key;

                // Pestaña
                var tab = new ToolbarToggle { text = key };
                tab.AddToClassList("params-tab");
                toggles.Add(tab);
                tabsBar.Add(tab);

                // Página
                var page = new ScrollView();
                page.AddToClassList("params-page");
                pageByKey[key] = page;
                pages.Add(page);

                // Contenido de la página
                foreach (var param in g.OrderBy(p => p.Label))
                {
                    if (param.Meta.VisibleIf != null && !param.Meta.VisibleIf()) continue;

                    var builder = _builders.FirstOrDefault(b => b.Supports(param.ValueType, param.Meta));
                    VisualElement ve;
                    if (builder == null)
                    {
                        ve = new TextField(param.Label) { value = param.GetBoxed()?.ToString() ?? "" };
                        ve.RegisterCallback<ChangeEvent<string>>(evt =>
                        {
                            try
                            {
                                var cast = Convert.ChangeType(evt.newValue, param.ValueType);
                                param.SetBoxed(cast);
                            } catch { }
                        });
                    }
                    else
                    {
                        ve = builder.Build(param);
                        builder.Bind(ve, param);
                    }

                    ve.AddToClassList("param-row"); // estilo común por fila
                    page.Add(ve);
                }

                // Comandos del mismo grupo
                foreach (var cmd in Catalog.AllCommands.Where(c => c.GroupPath == g.Key).OrderBy(c => c.Label))
                {
                    var btn = _buttonBuilder.BuildButton(cmd);
                    btn.AddToClassList("param-button");
                    page.Add(btn);
                }

                // Click de pestaña → mostrar página
                tab.RegisterValueChangedCallback(evt =>
                {
                    if (!evt.newValue) return;
                    SelectTab(key, toggles, pageByKey);
                });
            }

            // Selecciona la primera pestaña
            var first = toggles.FirstOrDefault();
            if (first != null)
            {
                first.SetValueWithoutNotify(true);
                SelectTab(first.text, toggles, pageByKey);
            }

            // Búsqueda: filtra solo en la página activa
            search.RegisterValueChangedCallback(evt =>
            {
                var q = (evt.newValue ?? "").Trim();
                var active = pageByKey.Values.FirstOrDefault(p => p.style.display == DisplayStyle.Flex);
                if (active == null) return;

                foreach (var child in active.Children())
                {
                    string label = ExtractLabel(child);
                    bool match = string.IsNullOrEmpty(q) || (label?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
                    child.style.display = match ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });
        }

        // Helpers ------------------------------------------------------------
        static void SelectTab(string key, List<ToolbarToggle> toggles, Dictionary<string, VisualElement> pageByKey)
        {
            foreach (var t in toggles)
                t.SetValueWithoutNotify(t.text == key);

            foreach (var kv in pageByKey)
                kv.Value.style.display = kv.Key == key ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static string ExtractLabel(VisualElement ve)
        {
            // intenta coger la etiqueta de los campos más comunes
            if (ve is BaseField<float> bf) return bf.label;
            if (ve is IntegerField inf) return inf.label;
            if (ve is Toggle t) return t.label;
            if (ve is TextField tf) return tf.label;
            if (ve is BaseField<Vector2> v2f) return v2f.label;
            if (ve is BaseField<Vector3> v3f) return v3f.label;
            if (ve is BaseField<Vector4> v4f) return v4f.label;
            // fallback: busca un Label hijo
            var lbl = ve.Q<Label>();
            return lbl?.text;
        }
        void ApplyStyles(VisualElement root)
        {
            root.style.flexGrow = 1;
            root.AddToClassList("params-root");

            var ss = Resources.Load<StyleSheet>("ParamsUI/params"); // Assets/Resources/ParamsUI/params.uss
            if (ss != null && !root.styleSheets.Contains(ss))
                root.styleSheets.Add(ss);

            // Opacidad de fondo (viene de USS, aquí por si quieres retocarla en vivo)
            root.userData = 0f;
        }

        void ResetVisible(VisualElement _)
        {
            // TODO: restaurar valores por defecto si guardas DefaultValue en ParamMeta
        }

        public bool Visible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                ApplyVisibility();
            }
        }

        void ApplyVisibility()
        {
            if (_uiDocument == null) return;
            var root = _uiDocument.rootVisualElement;
            if (root == null) return;
            root.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
