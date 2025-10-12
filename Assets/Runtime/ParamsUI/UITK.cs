// Runtime/ParamsUI/UITK.cs
using System;
using System.Collections.Generic;
using System.Linq;
using ParamsUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParamsUI.UITK
{
    public interface IControlBuilder
    {
        bool Supports(Type t, ParamMeta meta);
        VisualElement Build(IParam param);
        void Bind(VisualElement ve, IParam param);
    }

    public sealed class FloatSliderBuilder : IControlBuilder
    {
        public bool Supports(Type t, ParamMeta meta) =>
            (t == typeof(float) || t == typeof(double)) && meta.Min.HasValue && meta.Max.HasValue;

        public VisualElement Build(IParam param)
        {
            var slider = new Slider(param.Label, (float)param.Meta.Min.Value, (float)param.Meta.Max.Value)
            {
                value = Convert.ToSingle(param.GetBoxed())
            };
            AddTooltip(slider, param);
            return slider;
        }

        public void Bind(VisualElement ve, IParam param)
        {
            var slider = (Slider)ve;
            bool updating = false;

            slider.RegisterValueChangedCallback(evt =>
            {
                if (updating) return;
                param.SetBoxed(Convert.ToSingle(evt.newValue));
            });

            param.Changed += _ =>
            {
                updating = true;
                slider.SetValueWithoutNotify(Convert.ToSingle(param.GetBoxed()));
                updating = false;
            };
        }

        static void AddTooltip(VisualElement ve, IParam p)
        {
            if (!string.IsNullOrEmpty(p.Meta.Tooltip))
                ve.tooltip = p.Meta.Tooltip;
        }
    }

    public sealed class IntFieldBuilder : IControlBuilder
    {
        public bool Supports(Type t, ParamMeta meta) => t == typeof(int);

        public VisualElement Build(IParam param)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            row.Add(new Label(param.Label) { style = { minWidth = 140 } });
            var field = new IntegerField { value = (int)param.GetBoxed(), style = { flexGrow = 1 } };
            if (!string.IsNullOrEmpty(param.Meta.Tooltip)) field.tooltip = param.Meta.Tooltip;
            row.Add(field);
            return row;
        }

        public void Bind(VisualElement ve, IParam param)
        {
            var field = ve.Q<IntegerField>();
            bool updating = false;

            field.RegisterValueChangedCallback(evt =>
            {
                if (updating) return;
                int v = evt.newValue;
                if (param.Meta.Min.HasValue) v = Math.Max(v, (int)param.Meta.Min.Value);
                if (param.Meta.Max.HasValue) v = Math.Min(v, (int)param.Meta.Max.Value);
                param.SetBoxed(v);
            });

            param.Changed += _ =>
            {
                updating = true;
                field.SetValueWithoutNotify((int)param.GetBoxed());
                updating = false;
            };
        }
    }

    public sealed class BoolToggleBuilder : IControlBuilder
    {
        public bool Supports(Type t, ParamMeta meta) => t == typeof(bool);

        public VisualElement Build(IParam param)
        {
            var toggle = new Toggle(param.Label) { value = (bool)param.GetBoxed() };
            if (!string.IsNullOrEmpty(param.Meta.Tooltip)) toggle.tooltip = param.Meta.Tooltip;
            return toggle;
        }

        public void Bind(VisualElement ve, IParam param)
        {
            var toggle = (Toggle)ve;
            bool updating = false;

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (updating) return;
                param.SetBoxed(evt.newValue);
            });

            param.Changed += _ =>
            {
                updating = true;
                toggle.SetValueWithoutNotify((bool)param.GetBoxed());
                updating = false;
            };
        }
    }

    public sealed class EnumDropdownBuilder : IControlBuilder
    {
        public bool Supports(Type t, ParamMeta meta) => t.IsEnum;

        public VisualElement Build(IParam param)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            row.Add(new Label(param.Label) { style = { minWidth = 140 } });

            var names = Enum.GetNames(param.ValueType).ToList();
            var dropdown = new DropdownField(names, names.IndexOf(param.GetBoxed().ToString()))
            { style = { flexGrow = 1 } };
            if (!string.IsNullOrEmpty(param.Meta.Tooltip)) dropdown.tooltip = param.Meta.Tooltip;

            row.Add(dropdown);
            return row;
        }

        public void Bind(VisualElement ve, IParam param)
        {
            var dropdown = ve.Q<DropdownField>();
            bool updating = false;

            dropdown.RegisterValueChangedCallback(evt =>
            {
                if (updating) return;
                var value = Enum.Parse(param.ValueType, evt.newValue);
                param.SetBoxed(value);
            });

            param.Changed += _ =>
            {
                updating = true;
                dropdown.SetValueWithoutNotify(param.GetBoxed().ToString());
                updating = false;
            };
        }
    }

    public sealed class ButtonBuilder : IControlBuilder
    {
        readonly Func<string, CommandDef> _cmdGetter;
        public ButtonBuilder(Func<string, CommandDef> cmdGetter) => _cmdGetter = cmdGetter;

        public bool Supports(Type t, ParamMeta meta) => false; // no aplica a IParam, es CommandDef

        public VisualElement Build(IParam param) => null; // no usado
        public void Bind(VisualElement ve, IParam param) { }

        public VisualElement BuildButton(CommandDef cmd)
        {
            var btn = new Button(() => cmd.Execute()) { text = cmd.Label };
            return btn;
        }
    }

    public sealed class FloatFieldBuilder : IControlBuilder
    {
        public bool Supports(Type t, ParamMeta meta) =>
            (t == typeof(float) || t == typeof(double)) && !(meta.Min.HasValue && meta.Max.HasValue);

        public VisualElement Build(IParam p)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            row.Add(new Label(p.Label) { style = { minWidth = 140 } });
            var f = new FloatField { value = Convert.ToSingle(p.GetBoxed()), style = { flexGrow = 1 } };
            if (!string.IsNullOrEmpty(p.Meta.Tooltip)) f.tooltip = p.Meta.Tooltip;
            row.Add(f);
            return row;
        }

        public void Bind(VisualElement ve, IParam p)
        {
            var f = ve.Q<FloatField>();
            bool updating = false;

            f.RegisterValueChangedCallback(evt =>
            {
                if (updating) return;
                p.SetBoxed(Convert.ToSingle(evt.newValue));
            });

            p.Changed += _ =>
            {
                updating = true;
                f.SetValueWithoutNotify(Convert.ToSingle(p.GetBoxed()));
                updating = false;
            };
        }
    }
    
    public sealed class Vector4FieldBuilder : IControlBuilder
    {
        public bool Supports(Type t, ParamMeta meta) => t == typeof(Vector4);

        public VisualElement Build(IParam p)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            row.Add(new Label(p.Label) { style = { minWidth = 140 } });
            var field = new Vector4Field { value = (Vector4)p.GetBoxed(), style = { flexGrow = 1 } };
            if (!string.IsNullOrEmpty(p.Meta.Tooltip)) field.tooltip = p.Meta.Tooltip;
            row.Add(field);
            return row;
        }

        public void Bind(VisualElement ve, IParam p)
        {
            var field = ve.Q<Vector4Field>();
            bool updating = false;

            field.RegisterValueChangedCallback(evt =>
            {
                if (updating) return;
                p.SetBoxed(evt.newValue);
            });

            p.Changed += _ =>
            {
                updating = true;
                field.SetValueWithoutNotify((Vector4)p.GetBoxed());
                updating = false;
            };
        }
    }
    
}