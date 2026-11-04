using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Тек безендіру. Қозғалыс пен соқтығысу Player.cs ішінде қалады.
public class GamePresentation : MonoBehaviour
{
    Player player;
    Sprite square, circle, triangle;
    Color navy = new Color(.035f,.06f,.12f);
    Color cyan = new Color(.3f,.88f,1);
    Color gold = new Color(1,.79f,.25f);
    GUIStyle title, small, number, center;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= Create;
        SceneManager.sceneLoaded += Create;
    }

    static void Create(Scene scene, LoadSceneMode mode)
    {
        if (Object.FindFirstObjectByType<Player>() != null)
            new GameObject("Visual Design").AddComponent<GamePresentation>();
    }

    IEnumerator Start()
    {
        player = Object.FindFirstObjectByType<Player>();
        square = MakeSprite(0); circle = MakeSprite(1); triangle = MakeSprite(2);
        Camera.main.backgroundColor = navy;
        var random = new System.Random(24);
        for (int i=0;i<60;i++)
        {
            float x=(float)random.NextDouble()*26-10;
            float y=(float)random.NextDouble()*8-1;
            float size=i%6==0 ? .055f : .025f;
            Draw("Star", circle, new Vector2(x,y), new Vector2(size,size), new Color(.5f,.75f,.9f,.55f), -20);
        }
        Draw("Moon glow",circle,new Vector2(8,4),Vector2.one*2.3f,new Color(.3f,.65f,.85f,.05f),-19);
        Draw("Moon",circle,new Vector2(8,4),Vector2.one*1.25f,new Color(.68f,.84f,.86f),-18);
        Draw("Moon shade",circle,new Vector2(8.3f,4.2f),Vector2.one*1.2f,navy,-17);
        for(int i=0;i<7;i++)
        {
            float height=3.3f+(i%3)*.7f;
            Draw("Distant mountain",triangle,new Vector2(-9+i*4,-1.5f+height/2),new Vector2(7,height),new Color(.08f,.16f,.23f),-15);
            Draw("Near mountain",triangle,new Vector2(-10+i*4,-.3f),new Vector2(6,2.5f),new Color(.08f,.23f,.28f),-12);
        }
        var ground=GameObject.Find("Ground");
        ground.GetComponent<SpriteRenderer>().color=new Color(.08f,.18f,.22f);
        Draw("Grass edge",square,new Vector2(3,-1.52f),new Vector2(21,.12f),new Color(.25f,.85f,.68f),1);
        Draw("Ground foundation",square,new Vector2(3,-4),new Vector2(21,3),new Color(.055f,.12f,.18f),-1);
        for(int i=0;i<30;i++)
            Draw("Ground fleck",square,new Vector2(-7+i*.7f,-1.85f-(i%3)*.35f),new Vector2(.18f,.05f),new Color(.13f,.3f,.33f),1);
        Face(player.transform,false);
        Face(GameObject.Find("Spike").transform,true);
        yield return null; // Spawner.Start() бес тиынды жасағанша күту.
        foreach(var coin in GameObject.FindGameObjectsWithTag("Coin"))
        {
            var visual=coin.transform.Find("CoinVisual");
            if(visual==null) visual=coin.transform;
            visual.GetComponent<SpriteRenderer>().color=gold;
            Child(visual,"Coin rim",circle,Vector2.zero,Vector2.one*.78f,new Color(.75f,.43f,.08f),2);
            Child(visual,"Coin face",circle,Vector2.zero,Vector2.one*.65f,gold,3);
            Child(visual,"Coin shine",square,Vector2.zero,new Vector2(.1f,.36f),new Color(1,.95f,.65f),4);
            visual.gameObject.AddComponent<CoinShimmer>();
        }
    }

    void Face(Transform parent, bool enemy)
    {
        Child(parent,"Body highlight",square,new Vector2(0,.38f),new Vector2(.78f,.07f),enemy?new Color(1,.6f,.5f):cyan,2);
        foreach(float x in new[]{-.2f,.2f})
        {
            Child(parent,"Eye",square,new Vector2(x,.1f),new Vector2(.18f,.23f),Color.white,3);
            Child(parent,"Pupil",square,new Vector2(x+.035f,.08f),new Vector2(.08f,.12f),navy,4);
        }
        Child(parent,"Mouth",square,new Vector2(0,-.2f),new Vector2(.24f,.06f),navy,3);
    }

    GameObject Draw(string name,Sprite sprite,Vector2 pos,Vector2 scale,Color color,int order)
    {
        var obj=new GameObject(name); obj.transform.SetParent(transform);
        obj.transform.position=pos; obj.transform.localScale=new Vector3(scale.x,scale.y,1);
        var sr=obj.AddComponent<SpriteRenderer>(); sr.sprite=sprite; sr.color=color; sr.sortingOrder=order;
        return obj;
    }

    void Child(Transform parent,string name,Sprite sprite,Vector2 pos,Vector2 scale,Color color,int order)
    {
        var obj=Draw(name,sprite,Vector2.zero,scale,color,order);
        obj.transform.SetParent(parent,false); obj.transform.localPosition=pos;
    }

    Sprite MakeSprite(int shape)
    {
        var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);
        tex.filterMode=FilterMode.Bilinear;
        for(int y=0;y<64;y++) for(int x=0;x<64;x++)
        {
            bool inside=shape==0 || (shape==1 ? Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32))<31.5f : Mathf.Abs(x-31.5f)<(64-y)*.5f);
            tex.SetPixel(x,y,inside?Color.white:Color.clear);
        }
        tex.Apply(); return Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);
    }

    void OnGUI()
    {
        if(player==null) return;
        if(player.scoreText!=null) return; // 4-модульде Canvas + TextMeshPro қолданылады.
        if(title==null)
        {
            title=new GUIStyle(GUI.skin.label){fontSize=27,fontStyle=FontStyle.Bold}; title.normal.textColor=Color.white;
            small=new GUIStyle(title){fontSize=13,fontStyle=FontStyle.Normal}; small.normal.textColor=new Color(.65f,.76f,.85f);
            number=new GUIStyle(title){fontSize=22};
            center=new GUIStyle(title){alignment=TextAnchor.MiddleCenter,fontSize=22};
        }
        float scale=Screen.width/1000f;
        GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
        float height=Screen.height/scale;
        Panel(new Rect(25,23,950,100),new Color(.035f,.07f,.13f,.94f));
        Panel(new Rect(25,23,4,100),cyan);
        GUI.Label(new Rect(46,34,430,38),"ТИЫН ЖИНАУШЫ",title);
        GUI.Label(new Rect(48,78,470,25),"03 / ФИЗИКА ЖӘНЕ ӨЗАРА ӘРЕКЕТТЕСУ",small);
        GUI.Label(new Rect(590,40,170,28),"ТИЫН  " + player.Score + " / 5",number);
        for(int i=0;i<5;i++) Panel(new Rect(592+i*27,82,20,5),i<player.Score?gold:new Color(.17f,.24f,.3f));
        GUI.Label(new Rect(785,40,170,28),"ӨМІР  " + player.health,number);
        Panel(new Rect(787,82,150,6),new Color(.17f,.24f,.3f));
        Panel(new Rect(787,82,150*player.health/100f,6),player.health>40?cyan:new Color(1,.35f,.4f));
        Panel(new Rect(25,height-56,950,34),new Color(.035f,.07f,.13f,.94f));
        GUI.Label(new Rect(44,height-51,900,26),"A / D  немесе  ← / → : жүру       SPACE : секіру       R : қайта бастау",small);
        if(player.Score==5 || player.health==0)
        {
            Panel(new Rect(260,145,480,65),new Color(.035f,.07f,.13f,.96f));
            GUI.Label(new Rect(260,145,480,65),player.health==0?"Өмір аяқталды!  R — қайта бастау":"Керемет! Барлық 5 тиын жиналды!",center);
        }
    }

    void Panel(Rect rect,Color color)
    {
        GUI.color=color; GUI.DrawTexture(rect,Texture2D.whiteTexture); GUI.color=Color.white;
    }
}

public class CoinShimmer : MonoBehaviour
{
    SpriteRenderer body;
    void Start() { body=GetComponent<SpriteRenderer>(); }
    void Update()
    {
        float brightness=.9f+.1f*Mathf.Sin(Time.time*3+transform.position.x);
        body.color=new Color(1,.75f+brightness*.1f,.18f);
    }
}
