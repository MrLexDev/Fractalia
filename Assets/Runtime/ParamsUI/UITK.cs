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
            bool useSliders = p.Meta.VectorSlider != null;

            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            row.Add(new Label(p.Label) { style = { minWidth = 140 } });

            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    alignItems = Align.Center
                }
            };

            var (x, y, z, w) = info.Getter(p.GetBoxed());
            var labels = p.Meta.VectorSlider?.Labels;
            if (labels == null && p.ValueType == typeof(Color))
                labels = new[] { "R", "G", "B", "A" };

            VisualElement xControl = CreateComponent(0, x, useSliders, p.Meta.VectorSlider, labels);
            VisualElement yControl = CreateComponent(1, y, useSliders, p.Meta.VectorSlider, labels);
            VisualElement zControl = null;
            VisualElement wControl = null;

            container.Add(xControl);
            container.Add(yControl);

            if (info.Dim >= 3)
            {
                zControl = CreateComponent(2, z, useSliders, p.Meta.VectorSlider, labels);
                container.Add(zControl);
            }

            if (info.Dim == 4)
            {
                wControl = CreateComponent(3, w, useSliders, p.Meta.VectorSlider, labels);
                container.Add(wControl);
            }

            if (!string.IsNullOrEmpty(p.Meta.Tooltip))
                row.tooltip = p.Meta.Tooltip;

            row.Add(container);
            row.userData = new VecFields(info, xControl, yControl, zControl, wControl, p.Meta.VectorSlider);
            return row;
        }

        public void Bind(VisualElement ve, IParam p)
        {
            var vf = (VecFields)ve.userData;
            bool updating = false;

            void Push()
            {
                if (updating) return;

                var obj = vf.Info.Setter(
                    ReadValue(vf.X),
                    ReadValue(vf.Y),
                    vf.Info.Dim >= 3 ? ReadValue(vf.Z) : 0f,
                    vf.Info.Dim == 4 ? ReadValue(vf.W) : 0f
                );
                p.SetBoxed(obj);
            }

            RegisterComponent(vf.X, vf.Slider, () =>
            {
                if (!updating) Push();
            });
            RegisterComponent(vf.Y, vf.Slider, () =>
            {
                if (!updating) Push();
            });
            if (vf.Info.Dim >= 3)
                RegisterComponent(vf.Z, vf.Slider, () => { if (!updating) Push(); });
            if (vf.Info.Dim == 4)
                RegisterComponent(vf.W, vf.Slider, () => { if (!updating) Push(); });

            p.Changed += _ =>
            {
                updating = true;
                var (x, y, z, w) = vf.Info.Getter(p.GetBoxed());
                WriteValue(vf.X, vf.Slider, x);
                WriteValue(vf.Y, vf.Slider, y);
                if (vf.Info.Dim >= 3) WriteValue(vf.Z, vf.Slider, z);
                if (vf.Info.Dim == 4) WriteValue(vf.W, vf.Slider, w);
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
            public VecInfo Info;
            public VisualElement X, Y, Z, W;
            public ParamMeta.VectorSliderSettings Slider;

            public VecFields(VecInfo info, VisualElement x, VisualElement y, VisualElement z, VisualElement w, ParamMeta.VectorSliderSettings slider)
            {
                Info = info;
                X = x;
                Y = y;
                Z = z;
                W = w;
                Slider = slider;
            }
        }

        static VisualElement CreateComponent(int index, float value, bool useSlider, ParamMeta.VectorSliderSettings settings, string[] labels)
        {
            if (useSlider && settings != null)
            {
                return CreateSlider(GetAxisLabel(labels, index), value, settings, index == 0);
            }

            return CreateField(value, index == 0);
        }

        static FloatField CreateField(float value, bool isFirst)
        {
            var field = new FloatField { value = value };
            field.style.flexGrow = 1;
            if (!isFirst)
                field.style.marginLeft = 4;
            return field;
        }

        static Slider CreateSlider(string label, float value, ParamMeta.VectorSliderSettings settings, bool isFirst)
        {
            float min = (float)settings.Min;
            float max = (float)settings.Max;
            var slider = new Slider(label, min, max)
            {
                showInputField = false
            };

            slider.value = SnapToSlider(value, settings, min, max);
            slider.style.flexGrow = 1;
            if (!isFirst)
                slider.style.marginLeft = 6;
            slider.labelElement.style.minWidth = 18;
            slider.labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            slider.labelElement.style.alignSelf = Align.Center;
            slider.labelElement.style.marginRight = 4;
            return slider;
        }

        static string GetAxisLabel(string[] labels, int index)
        {
            if (labels != null && index < labels.Length && !string.IsNullOrEmpty(labels[index]))
                return labels[index];

            return index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "W",
                _ => string.Empty
            };
        }

        static void RegisterComponent(VisualElement element, ParamMeta.VectorSliderSettings settings, Action push)
        {
            if (element == null) return;

            switch (element)
            {
                case Slider slider:
                    slider.RegisterValueChangedCallback(evt =>
                    {
                        float min = slider.lowValue;
                        float max = slider.highValue;
                        float snapped = settings != null ? SnapToSlider(evt.newValue, settings, min, max) : Mathf.Clamp(evt.newValue, min, max);
                        if (!Mathf.Approximately(snapped, evt.newValue))
                            slider.SetValueWithoutNotify(snapped);
                        push();
                    });
                    break;
                case FloatField field:
                    field.RegisterValueChangedCallback(_ => push());
                    break;
            }
        }

        static float ReadValue(VisualElement element)
        {
            return element switch
            {
                Slider slider => slider.value,
                FloatField field => field.value,
                _ => 0f
            };
        }

        static void WriteValue(VisualElement element, ParamMeta.VectorSliderSettings settings, float value)
        {
            switch (element)
            {
                case Slider slider:
                    float min = slider.lowValue;
                    float max = slider.highValue;
                    slider.SetValueWithoutNotify(settings != null ? SnapToSlider(value, settings, min, max) : Mathf.Clamp(value, min, max));
                    break;
                case FloatField field:
                    field.SetValueWithoutNotify(value);
                    break;
            }
        }

        static float SnapToSlider(float value, ParamMeta.VectorSliderSettings settings, float min, float max)
        {
            float clamped = Mathf.Clamp(value, min, max);
            if (settings?.Step is double stepD && stepD > 0)
            {
                float step = (float)stepD;
                clamped = min + Mathf.Round((clamped - min) / step) * step;
                clamped = Mathf.Clamp(clamped, min, max);
            }
            return clamped;
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

            if (t == typeof(UnityEngine.Color))
            {
                info = new VecInfo
                {
                    Dim = 4,
                    Getter = o =>
                    {
                        var v = (UnityEngine.Color)o;
                        return (v.r, v.g, v.b, v.a);
                    },
                    Setter = (x, y, z, w) => new UnityEngine.Color(x, y, z, w)
                };
                return true;
            }

            if (t == typeof(UnityEngine.Color32))
            {
                info = new VecInfo
                {
                    Dim = 4,
                    Getter = o =>
                    {
                        var v = (UnityEngine.Color32)o;
                        return (v.r / 255f, v.g / 255f, v.b / 255f, v.a / 255f);
                    },
                    Setter = (x, y, z, w) => new UnityEngine.Color32(
                        (byte)Mathf.Clamp(Mathf.RoundToInt(x * 255f), 0, 255),
                        (byte)Mathf.Clamp(Mathf.RoundToInt(y * 255f), 0, 255),
                        (byte)Mathf.Clamp(Mathf.RoundToInt(z * 255f), 0, 255),
                        (byte)Mathf.Clamp(Mathf.RoundToInt(w * 255f), 0, 255))
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