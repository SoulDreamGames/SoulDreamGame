using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer render;

    [SerializeField] private Material skinnedMaterial;
    private float _intensity = 0.75f;

    public void SetMaterialColor(Color color)
    {
        string matName = skinnedMaterial.name + " (Instance)";
        foreach (var material in render.materials)
        {
            if (!material.name.Equals(matName)) continue;
            material.SetColor("_EmissionColor", color * _intensity);
            material.color = material.GetColor("_EmissionColor");
        }
        
    }
}
