using UnityEngine;
using UnityEditor;

public class SampleFoamPixels
{
    public static void Execute()
    {
        string path = "Assets/Tiny Swords/Terrain/Tileset/Water Foam.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogError("Importer not found"); return; }

        // Read/Write 임시 활성화
        bool wasReadable = importer.isReadable;
        importer.isReadable = true;
        importer.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogError("Texture not found"); return; }

        // 첫 번째 프레임(0~191 x)의 중심 픽셀 몇 개 샘플
        Debug.Log($"[Pixel] 텍스처 크기: {tex.width}x{tex.height}");
        int[] sampleX = { 96, 48, 144 }; // 첫 프레임 내 x 좌표
        int centerY = tex.height / 2;
        foreach (int x in sampleX)
        {
            var c = tex.GetPixel(x, centerY);
            Debug.Log($"  pixel({x},{centerY}): R={c.r:F2} G={c.g:F2} B={c.b:F2} A={c.a:F2}");
        }

        // 원래 설정 복원
        if (!wasReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }
    }
}
