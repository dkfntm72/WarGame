using UnityEngine;
using UnityEditor;

public class CheckTextureImport
{
    public static void Execute()
    {
        string path = "Assets/Tiny Swords/Terrain/Tileset/Water Foam.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogError("Importer not found"); return; }

        Debug.Log($"[Texture] textureType: {importer.textureType}");
        Debug.Log($"[Texture] spriteImportMode: {importer.spriteImportMode}");
        Debug.Log($"[Texture] alphaIsTransparency: {importer.alphaIsTransparency}");
        Debug.Log($"[Texture] sRGBTexture: {importer.sRGBTexture}");
        Debug.Log($"[Texture] filterMode: {importer.filterMode}");

        // 실제 텍스처 로드
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
        {
            Debug.Log($"[Texture] size: {tex.width}x{tex.height}, format: {tex.format}");
            // 첫 번째 픽셀 색상 샘플 (readable 여부 확인)
            try
            {
                var color = tex.GetPixel(0, 0);
                Debug.Log($"[Texture] pixel(0,0): {color}");
                var centerColor = tex.GetPixel(96, tex.height / 2);
                Debug.Log($"[Texture] pixel(96, center): {centerColor}");
            }
            catch { Debug.Log("[Texture] 텍스처가 Read/Write 불가 - 픽셀 샘플 불가"); }
        }
    }
}
