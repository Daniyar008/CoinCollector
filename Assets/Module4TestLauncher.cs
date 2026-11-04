using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class Module4TestLauncher
{
    [MenuItem("Tools/Practice/Preview Game Over")]
    static void PreviewGameOver()
    {
        var player=UnityEngine.Object.FindFirstObjectByType<Player>();
        if(EditorApplication.isPlaying && player!=null) player.TakeDamage(100);
    }
    static Module4TestLauncher() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if(!File.Exists("module4-test.request") || !File.Exists("Assets/Scenes/MainMenu.unity")) return;
        File.Delete("module4-test.request");
        SessionState.SetBool("Module4Verify",true);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying=true;
    }
}
