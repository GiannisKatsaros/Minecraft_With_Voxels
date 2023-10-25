using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AtlasPacker : EditorWindow
{
    private int blockSize = 16;
    private int atlasSizeInBlocks = 16;
    private int atlasSize;

    private Object[] rawTextures = new Object[256];
    private List<Texture2D> sortedTextures = new();
    private Texture2D atlas;


    [MenuItem("Minecraft/Atlas Packer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasPacker));
    }

    private void OnGUI()
    {
        atlasSize = blockSize * atlasSizeInBlocks;

        GUILayout.Label("Minecraft Texture Atlas Packer", EditorStyles.boldLabel);

        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size in Blocks", atlasSizeInBlocks);

        GUILayout.Label(atlas);

        if (GUILayout.Button("Load Textures"))
        {
            LoadTextures();
            PackAtlas();

            Debug.Log("Atlas Packer: Atlas packed");
        }

        if (GUILayout.Button("Clear Textures"))
        {
            atlas = new Texture2D(atlasSize, atlasSize);
            Debug.Log("Atlas Packer: Cleared textures");
        }

        if (GUILayout.Button("Save Atlas"))
        {
            byte[] bytes = atlas.EncodeToPNG();
            try
            {
                File.WriteAllBytes(Application.dataPath + "/Textures/Packed_Atlas.png", bytes);
                AssetDatabase.Refresh();
            }
            catch
            {
                Debug.Log("Atlas Packer: Failed to save atlas");
            }
        }
    }

    private void LoadTextures()
    {
        sortedTextures.Clear();
        rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));

        int index = 0;

        foreach (Object tex in rawTextures)
        {
            Texture2D t = (Texture2D)tex;
            if (t.width == blockSize && t.height == blockSize)
                sortedTextures.Add(t);
            else
                Debug.LogWarning("Atlas Packer: Texture " + tex.name + " is not " + blockSize + "x" + blockSize + " pixels. Skipping.");

            index++;
        }

        Debug.Log("Atlas Packer: " + sortedTextures.Count + " textures loaded");

    }

    private void PackAtlas()
    {
        atlas = new Texture2D(atlasSize, atlasSize);
        Color[] pixels = new Color[atlasSize * atlasSize];

        for (int x = 0; x < atlasSize; x++)
        {
            for (int y = 0; y < atlasSize; y++)
            {
                int currentBlockX = x / blockSize;
                int currentBlockY = y / blockSize;

                int index = currentBlockX + (currentBlockY * atlasSizeInBlocks);

                // int currentPixelX = x - (currentBlockX * blockSize);
                // int currentPicelY = y - (currentBlockY * blockSize);

                if (index < sortedTextures.Count)
                {
                    pixels[(atlasSize - y - 1) * atlasSize + x] = sortedTextures[index].GetPixel(x, blockSize - y - 1);
                }
                else
                {
                    pixels[(atlasSize - y - 1) * atlasSize + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        atlas.SetPixels(pixels);
        atlas.Apply();
    }
}
