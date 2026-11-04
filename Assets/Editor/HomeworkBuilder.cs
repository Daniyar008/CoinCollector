using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.IO;

[InitializeOnLoad]
public static class HomeworkBuilder
{
    static TMP_FontAsset font;
    static HomeworkBuilder() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if(File.Exists("homework-slider-fix.request"))
        {
            File.Delete("homework-slider-fix.request");
            var fixScene=EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
            var fillRect=Object.FindFirstObjectByType<Player>().healthSlider.fillRect;
            fillRect.sizeDelta=Vector2.zero;fillRect.anchoredPosition=Vector2.zero;
            EditorSceneManager.SaveScene(fixScene);
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }
        if(File.Exists("homework-build.request"))
        {
            File.Delete("homework-build.request");
            try { Build(); File.WriteAllText("homework-build.result","OK"); }
            catch(System.Exception e) { File.WriteAllText("homework-build.result",e.ToString());Debug.LogException(e); }
        }
        if(File.Exists("homework-test.request") && File.Exists("homework-build.result") && File.ReadAllText("homework-build.result")=="OK")
        {
            File.Delete("homework-test.request");
            SessionState.SetBool("HomeworkVerify",true);
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            EditorApplication.isPlaying=true;
        }
    }
    [MenuItem("Tools/Practice/Build Homework")]
    public static void Build()
    {
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Arial Kazakh.asset");
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        var player=Object.FindFirstObjectByType<Player>();
        var canvas=GameObject.Find("Canvas");var manager=GameObject.Find("GameManager").GetComponent<MenuManager>();
        if(canvas.transform.Find("WinPanel")!=null) throw new System.Exception("Homework already built; preserving existing scene.");
        var healthRect=player.healthText.rectTransform;
        healthRect.anchoredPosition=new Vector2(370,13);
        var sliderObj=Element(healthRect.parent.gameObject,"HealthSlider",new Vector2(370,-29),new Vector2(270,14));
        var slider=sliderObj.AddComponent<Slider>();
        Panel(sliderObj,"Background",Vector2.zero,new Vector2(270,14),new Color(.12f,.2f,.28f));
        var area=Element(sliderObj,"Fill Area",Vector2.zero,new Vector2(270,14));
        var fill=Panel(area,"Fill",Vector2.zero,new Vector2(270,14),new Color(.2f,.9f,.55f));
        fill.GetComponent<RectTransform>().sizeDelta=Vector2.zero;
        var handle=Element(sliderObj,"Handle Slide Area",Vector2.zero,Vector2.zero);handle.SetActive(false);
        slider.fillRect=fill.GetComponent<RectTransform>();slider.minValue=0;slider.maxValue=100;slider.wholeNumbers=true;slider.value=100;slider.interactable=false;slider.transition=Selectable.Transition.None;
        player.healthSlider=slider;
        var win=Overlay(canvas,"WinPanel","Жеңіс!","Барлық 5 тиынды жинадың!",new Color(1,.78f,.2f));
        var winCard=win.transform.Find("Card").gameObject;
        Button(winCard,"WinRestartButton","Қайта бастау",new Vector2(0,-55),manager.RestartGame);
        Button(winCard,"WinMenuButton","Мәзір",new Vector2(0,-135),manager.ToMenu);
        win.SetActive(false);player.winPanel=win;player.totalCoins=5;
        var pause=Overlay(canvas,"PausePanel","Пауза","Esc — ойынды жалғастыру",new Color(.2f,.85f,.95f));
        var pauseManager=new GameObject("PauseManager").AddComponent<PauseManager>();pauseManager.player=player;pauseManager.pausePanel=pause;
        var pauseCard=pause.transform.Find("Card").gameObject;
        Button(pauseCard,"ResumeButton","Жалғастыру",new Vector2(0,-55),pauseManager.ResumeGame);
        Button(pauseCard,"PauseMenuButton","Мәзір",new Vector2(0,-135),manager.ToMenu);
        pause.SetActive(false);
        var help=canvas.transform.Find("Controls/Help").GetComponent<TMP_Text>();
        help.text="A / D : жүру    SPACE : секіру    ESC : пауза    R : қайта бастау";
        MakeAnimation();
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("HOMEWORK_BUILD_OK");
    }
    static void MakeAnimation()
    {
        Directory.CreateDirectory("Assets/Animations");
        var clip=new AnimationClip();clip.name="CoinPulse";clip.frameRate=60;
        var curve=AnimationCurve.EaseInOut(0,1,.5f,1.3f);curve.AddKey(new Keyframe(1,1,0,0));
        clip.SetCurve("",typeof(Transform),"m_LocalScale.x",curve);
        clip.SetCurve("",typeof(Transform),"m_LocalScale.y",curve);
        clip.SetCurve("",typeof(Transform),"m_LocalScale.z",AnimationCurve.Constant(0,1,1));
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,"Assets/Animations/CoinPulse.anim");
        var controller=AnimatorController.CreateAnimatorControllerAtPath("Assets/Animations/CoinPulse.controller");
        controller.AddMotion(clip);
        var prefab=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Coin.prefab");
        var rootRenderer=prefab.GetComponent<SpriteRenderer>();
        var visual=new GameObject("CoinVisual");visual.transform.SetParent(prefab.transform,false);
        var renderer=visual.AddComponent<SpriteRenderer>();renderer.sprite=rootRenderer.sprite;renderer.color=rootRenderer.color;
        rootRenderer.enabled=false;
        visual.AddComponent<Animator>().runtimeAnimatorController=controller;
        PrefabUtility.SaveAsPrefabAsset(prefab,"Assets/Prefabs/Coin.prefab");PrefabUtility.UnloadPrefabContents(prefab);
    }
    static GameObject Element(GameObject parent,string name,Vector2 pos,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent.transform,false);
        var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;return go;
    }
    static GameObject Panel(GameObject parent,string name,Vector2 pos,Vector2 size,Color color)
    { var go=Element(parent,name,pos,size);go.AddComponent<Image>().color=color;return go; }
    static void Text(GameObject parent,string name,string value,int fontSize,Vector2 pos,Vector2 size)
    { var go=Element(parent,name,pos,size);var text=go.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=fontSize;text.text=value;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false; }
    static GameObject Overlay(GameObject canvas,string name,string title,string subtitle,Color accent)
    {
        var panel=Panel(canvas,name,Vector2.zero,new Vector2(1280,720),new Color(.015f,.03f,.07f,.9f));
        var rt=panel.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var card=Panel(panel,"Card",Vector2.zero,new Vector2(730,405),new Color(.035f,.065f,.12f));
        Panel(card,"Accent",new Vector2(0,199),new Vector2(730,5),accent);
        Text(card,"Title",title,70,new Vector2(0,110),new Vector2(700,100));
        Text(card,"Subtitle",subtitle,27,new Vector2(0,30),new Vector2(680,65));return panel;
    }
    static void Button(GameObject parent,string name,string label,Vector2 pos,UnityAction action)
    {
        var go=Panel(parent,name,pos,new Vector2(370,62),new Color(.08f,.32f,.4f));var b=go.AddComponent<Button>();UnityEventTools.AddPersistentListener(b.onClick,action);
        var colors=b.colors;colors.highlightedColor=new Color(.5f,1,1);b.colors=colors;
        Text(go,"Label",label,28,Vector2.zero,new Vector2(370,62));
    }
}
