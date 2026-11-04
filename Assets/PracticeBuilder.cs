using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class PracticeBuilder
{
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Art");
        Directory.CreateDirectory("Assets/Prefabs");
        AddTag("Ground"); AddTag("Coin"); AddTag("Enemy");
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input = settings.FindProperty("activeInputHandler");
        if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var square = MakeSprite("Square", false);
        var circle = MakeSprite("Circle", true);
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5.5f;
        cam.transform.position = new Vector3(3, 1, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.055f, 0.085f, 0.16f);
        cameraObject.AddComponent<AudioListener>();
        var ground = Shape("Ground", square, new Vector2(3, -2), new Vector2(21, 1), new Color(.2f,.55f,.42f));
        ground.tag = "Ground"; ground.AddComponent<BoxCollider2D>();
        var player = Shape("Player", square, new Vector2(-3, 0), Vector2.one, new Color(.15f,.6f,1));
        player.AddComponent<BoxCollider2D>();
        var rb = player.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<Player>();
        var spike = Shape("Spike", square, new Vector2(10, -1), Vector2.one, new Color(1,.23f,.28f));
        spike.tag = "Enemy"; spike.AddComponent<BoxCollider2D>();
        var coin = Shape("Coin", circle, Vector2.zero, new Vector2(.55f,.55f), new Color(1,.8f,.1f));
        coin.tag = "Coin"; coin.AddComponent<CircleCollider2D>().isTrigger = true;
        var prefab = PrefabUtility.SaveAsPrefabAsset(coin, "Assets/Prefabs/Coin.prefab");
        Object.DestroyImmediate(coin);
        new GameObject("Spawner").AddComponent<Spawner>().coinPrefab = prefab;
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/CoinCollector.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/CoinCollector.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("PRACTICE_BUILD_OK");
    }

    static GameObject Shape(string name, Sprite sprite, Vector2 pos, Vector2 scale, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.position = pos; obj.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color;
        return obj;
    }

    static Sprite MakeSprite(string name, bool circle)
    {
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        for (int y=0;y<64;y++) for(int x=0;x<64;x++)
            texture.SetPixel(x,y, !circle || Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32)) <= 31 ? Color.white : Color.clear);
        texture.Apply();
        string path = "Assets/Art/" + name + ".png";
        File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 64;
        importer.alphaIsTransparency = true; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void AddTag(string name)
    {
        var manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = manager.FindProperty("tags");
        for(int i=0;i<tags.arraySize;i++) if(tags.GetArrayElementAtIndex(i).stringValue==name) return;
        tags.InsertArrayElementAtIndex(tags.arraySize); tags.GetArrayElementAtIndex(tags.arraySize-1).stringValue=name;
        manager.ApplyModifiedPropertiesWithoutUndo();
    }
}
