using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class TextureSettings : UpdatableData
{
    public Material defaultmaterial;
    [SerializeField]
    public Region[] regions;
    public List<string> regionNames;
    public Dictionary<string, Material> materials;
    public Dictionary<string, Color> colors;

    private void Awake()
    {
        ReloadDicts();
    }

    public void ReloadDicts()
    {
        regionNames = new List<string>();
        materials = new Dictionary<string, Material>();
        colors = new Dictionary<string, Color>();
        for (int i = 0; i < regions.Length; i++)
        {
            regionNames.Add(regions[i].regionName);
            materials.Add(regions[i].regionName, regions[i].material);
            colors.Add(regions[i].regionName, regions[i].color);
        }
    }
    [Serializable]
    public struct Region
    {
        [SerializeField]
        public string regionName;
        [SerializeField]
        public Material material;
        [SerializeField]
        public Color color;
    }
}
