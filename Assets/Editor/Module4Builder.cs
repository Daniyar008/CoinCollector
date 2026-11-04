using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

[InitializeOnLoad]
public static class Module4Builder
{
    static TMP_FontAsset font;
    static Color navy = new Color(.035f,.065f,.12f,.96f);
    static Color cyan = new Color(.25f,.85f,.95f);
    static Module4Builder() { EditorApplication.delayCall += AutoBuild; }
    static void AutoBuild()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists("module4-build.request")) return;
        File.Delete("module4-build.request");
        try { Build(); File.WriteAllText("module4-build.result","OK"); }
        catch(System.Exception e) { Debug.LogException(e); File.WriteAllText("module4-build.result",e.ToString()); }
    }

    [MenuItem("Tools/Practice/Build Module 4")]
    public static void Build()
    {
        if(!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            foreach(var path in Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories))
            { AssetDatabase.ImportPackage(path,false); break; }
        }
        AssetDatabase.Refresh();
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Arial Kazakh.asset");
        if(font==null)
        {
            font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Arial.ttf"),48,5,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            font.name="Arial Kazakh";
            AssetDatabase.CreateAsset(font,"Assets/Fonts/Arial Kazakh.asset");
            foreach(var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture,font);
            AssetDatabase.AddObjectToAsset(font.material,font);
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ҰұҚқӨөІіӘәҒғҢңҮүҺһАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюя:!— /→←.");
            EditorUtility.SetDirty(font);
        }
        var settings=Resources.Load<TMP_Settings>("TMP Settings");
        if(settings!=null)
        {
            var serialized=new SerializedObject(settings);
            serialized.FindProperty("m_defaultFontAsset").objectReferenceValue=font;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        BuildGame(); BuildMenu();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Game.unity",true)};
        AssetDatabase.SaveAssets();
        Debug.Log("MODULE4_BUILD_OK");
    }

    static void BuildGame()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/CoinCollector.unity");
        var player=Object.FindFirstObjectByType<Player>();
        Music(Camera.main.gameObject);
        var manager=new GameObject("GameManager").AddComponent<MenuManager>();
        var canvas=CanvasRoot();
        var header=Panel(canvas,"HUD",new Vector2(0,285),new Vector2(1180,100),navy);
        player.scoreText=Text(header,"ScoreText","Ұпай: 0",40,new Vector2(-370,0),new Vector2(330,70));
        Text(header,"Title","ТИЫН ЖИНАУШЫ",23,Vector2.zero,new Vector2(400,60));
        player.healthText=Text(header,"HealthText","Өмір: 100",40,new Vector2(370,0),new Vector2(330,70));
        var footer=Panel(canvas,"Controls",new Vector2(0,-307),new Vector2(1180,44),navy);
        Text(footer,"Help","A / D : жүру    SPACE : секіру    R : қайта бастау",20,new Vector2(-60,0),new Vector2(900,40));
        Button(footer,"MenuButton","Мәзір",new Vector2(490,0),new Vector2(170,38),manager.ToMenu);
        var overlay=Panel(canvas,"GameOverPanel",Vector2.zero,new Vector2(1280,720),new Color(.015f,.03f,.07f,.88f));
        Stretch(overlay.GetComponent<RectTransform>());
        var card=Panel(overlay,"Card",Vector2.zero,new Vector2(730,405),navy);
        Panel(card,"Accent",new Vector2(0,199),new Vector2(730,5),cyan);
        Text(card,"GameOverTitle","Ойын аяқталды!",62,new Vector2(0,110),new Vector2(710,100));
        Text(card,"Hint","Қайта байқап көр — келесі жолы сәтті болады!",23,new Vector2(0,30),new Vector2(680,60));
        Button(card,"RestartButton","Қайта бастау",new Vector2(0,-55),new Vector2(370,64),manager.RestartGame);
        Button(card,"ToMenuButton","Мәзір",new Vector2(0,-135),new Vector2(370,56),manager.ToMenu);
        overlay.SetActive(false);
        player.gameOverPanel=overlay;
        player.coinSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CoinChime.wav");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Game.unity");
    }

    static void BuildMenu()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera"); camera.tag="MainCamera";
        var cam=camera.AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=navy;
        camera.AddComponent<AudioListener>(); Music(camera);
        var manager=new GameObject("MenuManager").AddComponent<MenuManager>();
        var canvas=CanvasRoot();
        var bg=Panel(canvas,"Background",Vector2.zero,new Vector2(1280,720),navy); Stretch(bg.GetComponent<RectTransform>());
        var random=new System.Random(42);
        for(int i=0;i<50;i++) Panel(canvas,"Star",new Vector2(random.Next(-620,620),random.Next(-340,340)),Vector2.one*(i%4==0?3:1.5f),new Color(.3f,.6f,.7f,.5f));
        Panel(canvas,"GroundLine",new Vector2(0,-260),new Vector2(1280,4),cyan);
        Text(canvas,"Module","04 / ИНТЕРФЕЙС ЖӘНЕ ОЙЫН ҚҰРЫЛЫМЫ",21,new Vector2(0,270),new Vector2(1100,50));
        Text(canvas,"Title","Тиын жинаушы",80,new Vector2(0,175),new Vector2(1100,120));
        Text(canvas,"Subtitle","Секір. Тиын жина. Кедергіден сақтан.",27,new Vector2(0,90),new Vector2(1000,60));
        Button(canvas,"PlayButton","Ойнау",new Vector2(0,-15),new Vector2(380,76),manager.PlayGame);
        Button(canvas,"QuitButton","Шығу",new Vector2(0,-110),new Vector2(380,64),manager.QuitGame);
        Text(canvas,"Controls","A / D — жүру     SPACE — секіру",23,new Vector2(0,-210),new Vector2(1000,50));
        Text(canvas,"Footer","5 ТИЫН  /  100 ӨМІР  /  БІР МАҚСАТ",20,new Vector2(0,-305),new Vector2(1000,50));
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/MainMenu.unity");
    }

    static void Music(GameObject go)
    {
        var source=go.AddComponent<AudioSource>(); source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MoonlightLoop.wav");
        source.playOnAwake=true; source.loop=true; source.volume=.3f; source.spatialBlend=0;
    }
    static GameObject CanvasRoot()
    {
        var root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        return root;
    }
    static void Stretch(RectTransform rt) { rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero; }
    static GameObject Element(GameObject parent,string name,Vector2 pos,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent.transform,false);
        var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;return go;
    }
    static GameObject Panel(GameObject parent,string name,Vector2 pos,Vector2 size,Color color)
    {
        var go=Element(parent,name,pos,size);go.AddComponent<Image>().color=color;return go;
    }
    static TMP_Text Text(GameObject parent,string name,string value,int size,Vector2 pos,Vector2 rect)
    {
        var go=Element(parent,name,pos,rect);var text=go.AddComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=size;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return text;
    }
    static void Button(GameObject parent,string name,string value,Vector2 pos,Vector2 size,UnityAction action)
    {
        var go=Panel(parent,name,pos,size,new Color(.08f,.32f,.4f));var button=go.AddComponent<UnityEngine.UI.Button>();
        var colors=button.colors;colors.highlightedColor=new Color(.5f,1,1);colors.pressedColor=new Color(.2f,.65f,.7f);button.colors=colors;
        UnityEventTools.AddPersistentListener(button.onClick,action);
        Text(go,"Label",value,28,Vector2.zero,size);
    }
}
