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
    
    public sealed class VectorLikeFieldBuilder : IControlBuilder
    {
        // ----------------- Public API -----------------
        public bool Supports(Type t, ParamMeta meta) => TryGetInfo(t, out _);

        public VisualElement Build(IParam p)
        {
            var info = GetInfoOrThrow(p.ValueType);
            var row  = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            row.Add(new Label(p.Label) { style = { minWidth = 140 } });

            // Cajas numéricas
            var (xF, yF, zF, wF) = (new FloatField(), new FloatField(), new FloatField(), new FloatField());
            xF.style.flexGrow = yF.style.flexGrow = zF.style.flexGrow = wF.style.flexGrow = 1;

            // Valores iniciales
            var (x, y, z, w) = info.Getter(p.GetBoxed());
            xF.value = x; yF.value = y;
            if (info.Dim >= 3) zF.value = z;
            if (info.Dim == 4) wF.value = w;

            // Layout: añade solo las que correspondan
            row.Add(xF); row.Add(yF);
            if (info.Dim >= 3) row.Add(zF);
            if (info.Dim == 4) row.Add(wF);

            if (!string.IsNullOrEmpty(p.Meta.Tooltip)) row.tooltip = p.Meta.Tooltip;

            // Guarda refs en userData para el Bind
            row.userData = new VecFields(info, xF, yF, zF, wF);
            return row;
        }

        public void Bind(VisualElement ve, IParam p)
        {
            var vf = (VecFields)ve.userData;
            bool updating = false;

            Action push = () =>
            {
                if (updating) return;
                var obj = vf.Info.Setter(
                    vf.X.value,
                    vf.Y.value,
                    vf.Info.Dim >= 3 ? vf.Z.value : 0f,
                    vf.Info.Dim == 4 ? vf.W.value : 0f
                );
                p.SetBoxed(obj);
            };

            vf.X.RegisterValueChangedCallback(_ => push());
            vf.Y.RegisterValueChangedCallback(_ => push());
            if (vf.Info.Dim >= 3) vf.Z.RegisterValueChangedCallback(_ => push());
            if (vf.Info.Dim == 4) vf.W.RegisterValueChangedCallback(_ => push());

            p.Changed += _ =>
            {
                updating = true;
                var (x, y, z, w) = vf.Info.Getter(p.GetBoxed());
                vf.X.SetValueWithoutNotify(x);
                vf.Y.SetValueWithoutNotify(y);
                if (vf.Info.Dim >= 3) vf.Z.SetValueWithoutNotify(z);
                if (vf.Info.Dim == 4) vf.W.SetValueWithoutNotify(w);
                updating = false;
            };
        }

        // ----------------- Internals -----------------
        struct VecInfo
        {
            public int Dim;
            public Func<object, (float x, float y, float z, float w)> Getter;
            public Func<float, float, float, float, object> Setter;
        }

        sealed class VecFields
        {
            public VecInfo Info; public FloatField X, Y, Z, W;
            public VecFields(VecInfo info, FloatField x, FloatField y, FloatField z, FloatField w)
            { Info = info; X = x; Y = y; Z = z; W = w; }
        }

        static bool TryGetInfo(Type t, out VecInfo info)
        {
            // 1) UnityEngine.Vector*
            if (t == typeof(UnityEngine.Vector2))
            {
                info = new VecInfo{
                    Dim = 2,
                    Getter = o => { var v = (UnityEngine.Vector2)o; return (v.x, v.y, 0f, 0f); },
                    Setter = (x,y,z,w) => new UnityEngine.Vector2(x,y)
                };
                return true;
            }
            if (t == typeof(UnityEngine.Vector3))
            {
                info = new VecInfo{
                    Dim = 3,
                    Getter = o => { var v = (UnityEngine.Vector3)o; return (v.x, v.y, v.z, 0f); },
                    Setter = (x,y,z,w) => new UnityEngine.Vector3(x,y,z)
                };
                return true;
            }
            if (t == typeof(UnityEngine.Vector4))
            {
                info = new VecInfo{
                    Dim = 4,
                    Getter = o => { var v = (UnityEngine.Vector4)o; return (v.x, v.y, v.z, v.w); },
                    Setter = (x,y,z,w) => new UnityEngine.Vector4(x,y,z,w)
                };
                return true;
            }

            // 2) System.Numerics.Vector*
            if (t == typeof(System.Numerics.Vector2))
            {
                info = new VecInfo{
                    Dim = 2,
                    Getter = o => { var v = (System.Numerics.Vector2)o; return (v.X, v.Y, 0f, 0f); },
                    Setter = (x,y,z,w) => new System.Numerics.Vector2(x,y)
                };
                return true;
            }
            if (t == typeof(System.Numerics.Vector3))
            {
                info = new VecInfo{
                    Dim = 3,
                    Getter = o => { var v = (System.Numerics.Vector3)o; return (v.X, v.Y, v.Z, 0f); },
                    Setter = (x,y,z,w) => new System.Numerics.Vector3(x,y,z)
                };
                return true;
            }
            if (t == typeof(System.Numerics.Vector4))
            {
                info = new VecInfo{
                    Dim = 4,
                    Getter = o => { var v = (System.Numerics.Vector4)o; return (v.X, v.Y, v.Z, v.W); },
                    Setter = (x,y,z,w) => new System.Numerics.Vector4(x,y,z,w)
                };
                return true;
            }

            // 3) Unity.Mathematics.float*
            if (t.FullName == "Unity.Mathematics.float2")
            {
                info = new VecInfo{
                    Dim = 2,
                    Getter = o => (GetField(o,"x"), GetField(o,"y"), 0f, 0f),
                    Setter = (x,y,z,w) => Activator.CreateInstance(t, x, y)
                };
                return true;
            }
            if (t.FullName == "Unity.Mathematics.float3")
            {
                info = new VecInfo{
                    Dim = 3,
                    Getter = o => (GetField(o,"x"), GetField(o,"y"), GetField(o,"z"), 0f),
                    Setter = (x,y,z,w) => Activator.CreateInstance(t, x, y, z)
                };
                return true;
            }
            if (t.FullName == "Unity.Mathematics.float4")
            {
                info = new VecInfo{
                    Dim = 4,
                    Getter = o => (GetField(o,"x"), GetField(o,"y"), GetField(o,"z"), GetField(o,"w")),
                    Setter = (x,y,z,w) => Activator.CreateInstance(t, x, y, z, w)
                };
                return true;
            }

            info = default;
            return false;

            static float GetField(object o, string name)
            {
                var f = o.GetType().GetField(name);
                return f != null ? (float)Convert.ChangeType(f.GetValue(o), typeof(float)) : 0f;
            }
        }

        static VecInfo GetInfoOrThrow(Type t)
        {
            if (TryGetInfo(t, out var info)) return info;
            throw new NotSupportedException($"VectorLikeFieldBuilder: tipo no soportado: {t.FullName}");
        }
    }


    
}