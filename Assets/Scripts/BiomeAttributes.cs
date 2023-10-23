using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    // below this height, the biome is always solid ground
    public int solidGroundHeight;

    // max delta between solid ground height and air
    public int terrainHeight;

    public float terrainScale;

    [Header("Trees")]
    public float treeZoneScale = 1.3f;
    [Range(0.1f, 1.0f)]
    public float treeZoneThreshold = 0.6f;
    public float treePlacementScale = 15.0f;
    [Range(0.1f, 1.0f)]
    public float treePlacementThreshold = 0.8f;

    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string lodeName;
    public byte blockId;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}