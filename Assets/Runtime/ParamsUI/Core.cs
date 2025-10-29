// Runtime/ParamsUI/Core.cs
using System;
using System.Collections.Generic;

namespace ParamsUI
{
    public interface IParam
    {
        string Key { get; }
        string Label { get; }
        string GroupPath { get; }
        Type ValueType { get; }
        event Action<IParam> Changed;
        object GetBoxed();
        void SetBoxed(object value);
        ParamMeta Meta { get; }
    }

    public sealed class Param<T> : IParam
    {
        public string Key { get; }
        public string Label { get; }
        public string GroupPath { get; }
        public ParamMeta Meta { get; }

        readonly Func<T> _getter;
        readonly Action<T> _setter;

        public event Action<IParam> Changed;

        public Param(string key, string label, string groupPath, Func<T> getter, Action<T> setter, ParamMeta meta)
        {
            Key = key;
            Label = label;
            GroupPath = groupPath ?? "";
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            Meta = meta ?? ParamMeta.Default;
        }

        public Type ValueType => typeof(T);
        public T Value { get => _getter(); set { _setter(value); Changed?.Invoke(this); } }
        public object GetBoxed() => Value;
        public void SetBoxed(object value) => Value = (T)value;
    }

    public sealed class ParamDef<T>
    {
        public string Key { get; private set; }
        public string Label { get; private set; }
        public string GroupPath { get; private set; } = "";
        public Func<T> Getter { get; private set; }
        public Action<T> Setter { get; private set; }
        public ParamMeta Meta { get; private set; } = ParamMeta.Default;

        public ParamDef(string key, string label, Func<T> getter, Action<T> setter)
        {
            Key = key;
            Label = label;
            Getter = getter;
            Setter = setter;
        }

        public ParamDef<T> InGroup(string groupPath) { GroupPath = groupPath; return this; }
        public ParamDef<T> WithMeta(ParamMeta meta) { Meta = meta; return this; }

        public IParam Build() => new Param<T>(Key, Label, GroupPath, Getter, Setter, Meta);
    }

    public sealed class ParamMeta
    {
        public static readonly ParamMeta Default = new ParamMeta();

        // Rango/step para numéricos
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Step { get; set; }

        // Configuración específica para vectores (slider por componente)
        public VectorSliderSettings VectorSlider { get; set; }

        // Opciones (enum o listas)
        public IReadOnlyList<(string label, object value)> Options { get; set; }

        // Otros
        public string Tooltip { get; set; }
        public Func<bool> VisibleIf { get; set; } // puede ser null
        public string Unit { get; set; }

        public static ParamMeta Range(double min, double max, double? step = null, string unit = null) =>
            new ParamMeta { Min = min, Max = max, Step = step, Unit = unit };

        public ParamMeta WithVectorSlider(double min, double max, double? step = null, params string[] labels)
        {
            if (ReferenceEquals(this, Default))
            {
                var meta = new ParamMeta();
                return meta.WithVectorSlider(min, max, step, labels);
            }

            VectorSlider = new VectorSliderSettings
            {
                Min = min,
                Max = max,
                Step = step,
                Labels = labels != null && labels.Length > 0 ? labels : null
            };
            return this;
        }

        public ParamMeta WithColorSlider(double min = 0.0, double max = 1.0, double? step = 0.01)
            => WithVectorSlider(min, max, step, "R", "G", "B", "A");

        public sealed class VectorSliderSettings
        {
            public double Min { get; set; }
            public double Max { get; set; }
            public double? Step { get; set; }
            public string[] Labels { get; set; }
        }
    }

    public sealed class CommandDef
    {
        public string Key { get; }
        public string Label { get; }
        public string GroupPath { get; }
        public Action Execute { get; }

        public CommandDef(string key, string label, Action execute, string groupPath = "")
        {
            Key = key; Label = label; Execute = execute; GroupPath = groupPath ?? "";
        }
    }

    public sealed class ParamCatalog
    {
        readonly List<IParam> _params = new();
        readonly List<CommandDef> _commands = new();

        public IReadOnlyList<IParam> AllParams => _params;
        public IReadOnlyList<CommandDef> AllCommands => _commands;

        public void Add(IParam p) => _params.Add(p);
        public void Add<T>(ParamDef<T> def) => _params.Add(def.Build());
        public void Add(CommandDef cmd) => _commands.Add(cmd);

        public void Clear() { _params.Clear(); _commands.Clear(); }
    }
}
