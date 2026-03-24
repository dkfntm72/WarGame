using UnityEditor;
using TMPro;

public static class WarGameTMPImport
{
    [MenuItem("Window/WarGame/Import TMP Resources")]
    public static void ImportTMP()
    {
        TMP_PackageUtilities.ImportProjectResourcesMenu();
    }
}
