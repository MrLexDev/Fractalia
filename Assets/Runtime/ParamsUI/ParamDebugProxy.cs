// Runtime/ParamsUI/Debug/ParamDebugProxy.cs
using UnityEngine;

public enum DebugMode { A, B, C }

public sealed class ParamDebugProxy : MonoBehaviour
{
    [Header("Observed (watch these in the Inspector)")]
    [SerializeField] float floatValue;
    [SerializeField] int   intValue;
    [SerializeField] bool  boolValue;
    [SerializeField] string textValue;
    [SerializeField] DebugMode mode;
    [SerializeField] Color colorValue = Color.white;

    public float FloatValue { get => floatValue; set { if (Mathf.Approximately(floatValue, value)) return; floatValue = value; Log(nameof(FloatValue), value); MarkDirty(); } }
    public int   IntValue   { get => intValue;     set { if (intValue == value) return; intValue = value; Log(nameof(IntValue), value); MarkDirty(); } }
    public bool  BoolValue  { get => boolValue;    set { if (boolValue == value) return; boolValue = value; Log(nameof(BoolValue), value); MarkDirty(); } }
    public string TextValue { get => textValue;    set { if (textValue == value) return; textValue = value; Log(nameof(TextValue), value); MarkDirty(); } }
    public DebugMode Mode   { get => mode;         set { if (mode == value) return; mode = value; Log(nameof(Mode), value); MarkDirty(); } }
    public Color ColorValue { get => colorValue;   set { if (colorValue == value) return; colorValue = value; Log(nameof(ColorValue), value); MarkDirty(); } }

    void Log<T>(string name, T v) => Debug.Log($"[Proxy] {name} = {v}", this);

#if UNITY_EDITOR
    void MarkDirty() => UnityEditor.EditorUtility.SetDirty(this);
#else
    void MarkDirty() { }
#endif
}