// Runtime/ParamsUI/Shader/ShaderParamBinderMaterial.cs
using UnityEngine;

public sealed class ShaderParamBinderMaterial : MonoBehaviour
{
    [Header("Target material (the same instance your PC_Renderer uses)")]
    public Material targetMaterial;

    [Tooltip("Instancia una copia en runtime para no modificar el asset en disco.")]
    public bool instantiateAtAwake = true;

    Material _mat;

    public Material RuntimeMaterial => _mat;

    void Awake()
    {
        if (!targetMaterial){
            Debug.LogError("[Binder] targetMaterial is null", this);
            return;
        }
        _mat = instantiateAtAwake ? new Material(targetMaterial) : targetMaterial;
        if (instantiateAtAwake) targetMaterial = _mat; // visible en el Inspector
    }

    int Id(string name) => Shader.PropertyToID(name);

    public float  GetFloat (string n) => _mat.GetFloat (Id(n));
    public int    GetInt   (string n) => _mat.GetInt   (Id(n));
    public Color  GetColor (string n) => _mat.GetColor (Id(n));
    public Vector4 GetVector(string n)=> _mat.GetVector(Id(n));

    public void SetFloat (string n, float  v) => _mat.SetFloat (Id(n), v);
    public void SetInt   (string n, int    v) => _mat.SetInt   (Id(n), v);
    public void SetColor (string n, Color  v) => _mat.SetColor (Id(n), v);
    public void SetVector(string n, Vector4 v)=> _mat.SetVector(Id(n), v);
}