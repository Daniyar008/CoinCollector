using UnityEditor;
using UnityEditor.SceneManagement;

public static class PracticeTests
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CoinCollector.unity");
        EditorApplication.isPlaying = true;
    }
}
