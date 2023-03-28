using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer render;

    [SerializeField] private Material skinnedMaterial;

    public void SetMaterialColor(Color color)
    {
        string matName = skinnedMaterial.name + " (Instance)";
        foreach (var material in render.materials)
        {
            Debug.Log("Render mat: " + material.name);
            if (!material.name.Equals(matName)) continue;
            Debug.Log("Match");
            material.SetColor("_EmissionColor", color);
        }
        
    }
}
