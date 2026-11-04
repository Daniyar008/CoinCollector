using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public static class Module5Builder {
    static TMP_FontAsset font;
    static TMP_Text Text(Transform parent,string name,string value,Vector2 pos,Vector2 size,int fs=30) {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
        var rt=go.GetComponent<RectTransform>();rt.sizeDelta=size;rt.anchoredPosition=pos;
        var t=go.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=fs;t.text=value;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;
    }
    public static void Build() {
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Arial Kazakh.asset");
        var source="Assets/Scenes/Game.unity";
        // Start with the original level and preserve all existing UI and animation.
        for(int level=1;level<=3;level++) {
            var scene=EditorSceneManager.OpenScene(source);
            var player=Object.FindFirstObjectByType<Player>();
            var manager=Object.FindFirstObjectByType<MenuManager>();
            var spawner=Object.FindFirstObjectByType<Spawner>();
            if(spawner!=null) Object.DestroyImmediate(spawner.gameObject);
            foreach(var coin in GameObject.FindGameObjectsWithTag("Coin")) Object.DestroyImmediate(coin);
            foreach(var platform in GameObject.FindGameObjectsWithTag("Ground")) if(platform.name!="Ground")Object.DestroyImmediate(platform);
            foreach(var enemy in GameObject.FindGameObjectsWithTag("Enemy")) if(enemy.name!="Spike")Object.DestroyImmediate(enemy);
            player.transform.position=new Vector3(-5,-.8f,0);
            player.totalCoins=level==1?5:level==2?8:10;
            float[] xs=level==1?new[]{-2f,0,2,4,6}:level==2?new[]{-3f,-1,1,3,5,7,9,11}:new[]{-3f,-1,1,3,5,7,8.2f,9.5f,10.7f,11.9f};
            var square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Square.png");
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coin.prefab");
            for(int i=0;i<xs.Length;i++) {
                float y=-.9f;
                if(level>1 && i%2==1) {
                    float top=level==3 && i>=5?1.05f:.1f;
                    var plat=new GameObject("Platform_"+i);plat.tag="Ground";plat.transform.position=new Vector3(xs[i],top-.18f,0);plat.transform.localScale=new Vector3(1.7f,.36f,1);
                    var sr=plat.AddComponent<SpriteRenderer>();sr.sprite=square;sr.color=new Color(.15f,.6f,.53f);plat.AddComponent<BoxCollider2D>();y=top+.65f;
                }
                var coin=(GameObject)PrefabUtility.InstantiatePrefab(prefab);coin.name="Coin_"+(i+1);coin.transform.position=new Vector3(xs[i],y,0);
            }
            // The higher platforms are reachable via lower steps (jump height ~2.5 units).
            var spike=GameObject.Find("Spike");spike.transform.position=new Vector3(level==1?9:2,-1.1f,0);spike.transform.localScale=new Vector3(.55f,.75f,1);
            for(int i=1;i<level;i++){var hazard=Object.Instantiate(spike);hazard.name="Spike_"+i;hazard.transform.position=new Vector3(2+i*4,-1.1f,0);}
            var card=player.winPanel.transform.Find("Card");
            card.GetComponent<RectTransform>().sizeDelta=new Vector2(730,490);
            card.Find("Accent").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,242);
            card.Find("Title").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,160);
            card.Find("Subtitle").GetComponent<TMP_Text>().text="Барлық "+player.totalCoins+" тиын жиналды!";
            card.Find("Subtitle").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,100);
            var old=card.Find("ResultText");if(old!=null)Object.DestroyImmediate(old.gameObject);
            player.resultText=Text(card,"ResultText","Уақыт: 0.0 с\nРекорд: 0.0 с",new Vector2(0,30),new Vector2(680,80),28);
            var restart=card.Find("WinRestartButton");restart.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-125);
            card.Find("WinMenuButton").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-200);
            var previous=card.Find("NextLevelButton");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
            if(level<3) {
                var next=Object.Instantiate(restart.gameObject,card);next.name="NextLevelButton";next.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-50);
                next.GetComponentInChildren<TMP_Text>().text="Келесі деңгей";var b=next.GetComponent<Button>();b.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(b.onClick,manager.NextLevel);
            }
            GameObject.Find("Canvas").transform.Find("HUD/Title").GetComponent<TMP_Text>().text="ТИЫН ЖИНАУШЫ  /  "+level+"–3";
            player.winPanel.SetActive(false);
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+(level==1?"Game":"Level"+level)+".unity");
        }
        var menuScene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var canvas=GameObject.Find("Canvas").transform;
        canvas.Find("Module").GetComponent<TMP_Text>().text="05 / ҮШ ДЕҢГЕЙ ЖӘНЕ РЕКОРД";
        canvas.Find("Footer").GetComponent<TMP_Text>().text="3 ДЕҢГЕЙ  /  23 ТИЫН  /  ЖАҢА РЕКОРД";
        canvas.Find("Controls").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-235);
        var existing=canvas.Find("RecordText");if(existing!=null)Object.DestroyImmediate(existing.gameObject);
        Object.FindFirstObjectByType<MenuManager>().recordText=Text(canvas,"RecordText","Рекорд жоқ",new Vector2(0,-175),new Vector2(1000,45),25);
        EditorSceneManager.SaveScene(menuScene);
        EditorBuildSettings.scenes=new[]{"MainMenu","Game","Level2","Level3"}.Select(n=>new EditorBuildSettingsScene("Assets/Scenes/"+n+".unity",true)).ToArray();
        PlayerSettings.productName="CoinCollector";PlayerSettings.companyName="Student Games";
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("../CoinCollector_Build");
        var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Select(s=>s.path).ToArray(),"../CoinCollector_Build/CoinCollector.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        File.WriteAllText("module5-build.result",report.summary.result.ToString());
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Build failed");
    }
}
