using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParamsUI.UITK
{
    public sealed class ParamWindow : MonoBehaviour
    {
        [SerializeField] UIDocument _uiDocument;
        public ParamCatalog Catalog { get; set; }

        readonly List<IControlBuilder> _builders = new();
        ButtonBuilder _buttonBuilder;

        void Awake()
        {
            if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

            // Registrar builders disponibles
            _buttonBuilder = new ButtonBuilder(FindCommandByKey);
            _builders.Add(new FloatSliderBuilder());
            _builders.Add(new IntFieldBuilder());
            _builders.Add(new BoolToggleBuilder());
            _builders.Add(new EnumDropdownBuilder());
            _builders.Add(new FloatFieldBuilder());
            _builders.Add(new Vector4FieldBuilder());
            // Aquí puedes registrar ColorFieldBuilder, Vector3FieldBuilder, etc.
        }

        CommandDef FindCommandByKey(string key) => Catalog?.AllCommands.FirstOrDefault(c => c.Key == key);

        void OnEnable() => Rebuild();

        public void Rebuild()
        {
            var root = _uiDocument.rootVisualElement;
            root.Clear();

            if (Catalog == null) return;

            // Layout básico
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
            var left = new ScrollView { style = { width = 220, borderRightWidth = 1, borderRightColor = new StyleColor(new Color(0,0,0,0.2f)) } };
            var right = new ScrollView { style = { flexGrow = 1, paddingLeft = 8, paddingRight = 8 } };
            container.Add(left);
            container.Add(right);

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 6, paddingTop = 6, paddingRight = 6 } };
            var search = new TextField { label = "Search", style = { flexGrow = 1 } };
            var reset = new Button(() => ResetVisible(right)) { text = "Reset Section" };
            header.Add(search);
            header.Add(reset);

            root.Add(header);
            root.Add(container);

            // Agrupar por GroupPath (foldouts)
            var groups = Catalog.AllParams
                .GroupBy(p => p.GroupPath ?? "")
                .OrderBy(g => g.Key);

            var groupToPanel = new Dictionary<string, VisualElement>();

            foreach (var g in groups)
            {
                var fo = new Foldout { text = string.IsNullOrWhiteSpace(g.Key) ? "(Root)" : g.Key, value = false };
                left.Add(fo);

                var panel = new VisualElement { style = { flexDirection = FlexDirection.Column, paddingTop = 4, paddingBottom = 12 } };
                groupToPanel[g.Key] = panel;

                fo.RegisterValueChangedCallback(_ =>
                {
                    right.Clear();
                    right.Add(panel);
                });

                foreach (var param in g.OrderBy(p => p.Label))
                {
                    if (param.Meta.VisibleIf != null && !param.Meta.VisibleIf()) continue;

                    var builder = _builders.FirstOrDefault(b => b.Supports(param.ValueType, param.Meta));
                    if (builder == null)
                    {
                        // Fallback simple: label + text field
                        var fallback = new TextField(param.Label) { value = param.GetBoxed().ToString() };
                        fallback.RegisterValueChangedCallback(evt =>
                        {
                            try
                            {
                                var cast = Convert.ChangeType(evt.newValue, param.ValueType);
                                param.SetBoxed(cast);
                            }
                            catch { /* ignore parse */ }
                        });
                        panel.Add(fallback);
                        continue;
                    }

                    var ve = builder.Build(param);
                    builder.Bind(ve, param);
                    panel.Add(ve);
                }

                // Comandos (botones) del mismo grupo
                foreach (var cmd in Catalog.AllCommands.Where(c => c.GroupPath == g.Key).OrderBy(c => c.Label))
                {
                    panel.Add(_buttonBuilder.BuildButton(cmd));
                }
            }

            // Búsqueda rápida (simple)
            search.RegisterValueChangedCallback(evt =>
            {
                var q = evt.newValue?.Trim() ?? "";
                foreach (var kv in groupToPanel)
                {
                    foreach (var child in kv.Value.Children())
                    {
                        bool match = true;
                        if (child is BaseField<float> bf) match = bf.label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                        else if (child is Toggle t) match = t.label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                        else if (child is TextField tf) match = tf.label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                        else if (child is IntegerField inf) match = inf.label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                        child.style.display = match ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
            });
        }

        void ResetVisible(VisualElement rightPanel)
        {
            // Si quieres soportar “valor por defecto”, añade DefaultValue en ParamMeta y restáuralo aquí.
            // Ejemplo placeholder (no-op).
        }
    }
}