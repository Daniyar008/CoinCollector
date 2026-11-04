#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Text;

public class Module4Verification : MonoBehaviour
{
    StringBuilder report=new StringBuilder();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        if(!UnityEditor.SessionState.GetBool("Module4Verify",false) || Object.FindFirstObjectByType<Module4Verification>()!=null) return;
        var go=new GameObject("Module4Verification"); DontDestroyOnLoad(go);go.AddComponent<Module4Verification>();
    }
    void Check(bool condition,string label)
    {
        report.AppendLine((condition?"PASS: ":"FAIL: ")+label);
        if(condition) return;
        File.WriteAllText("module4-tests.txt",report.ToString());
        UnityEditor.SessionState.SetBool("Module4Verify",false);
        UnityEditor.EditorApplication.isPlaying=false;
        throw new System.Exception(label);
    }
    void Click(string name) { GameObject.Find(name).GetComponent<Button>().onClick.Invoke(); }
    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(.5f);
        Check(SceneManager.GetActiveScene().name=="MainMenu","MainMenu is starting scene");
        Check(SceneUtility.GetScenePathByBuildIndex(0).EndsWith("MainMenu.unity") && SceneUtility.GetScenePathByBuildIndex(1).EndsWith("Game.unity"),"Build order MainMenu=0, Game=1");
        Check(Camera.main.GetComponent<AudioSource>().isPlaying,"Menu music plays");
        Click("PlayButton");
        yield return new WaitForSecondsRealtime(1.5f);
        Check(SceneManager.GetActiveScene().name=="Game","Play button loads Game");
        var player=Object.FindFirstObjectByType<Player>();
        Check(player.scoreText!=null && player.healthText!=null && player.gameOverPanel!=null && player.coinSound!=null,"All four Player references assigned");
        Check(player.scoreText.text=="Ұпай: 0" && player.healthText.text=="Өмір: 100","Initial TMP text");
        Check(player.scoreText.font.HasCharacters("ҰұҚқӨөІіӘәҒғҢңҮүҺһ"),"Font includes Kazakh letters");
        var music=Camera.main.GetComponent<AudioSource>();
        Check(music.isPlaying && music.loop && Mathf.Abs(music.volume-.3f)<.01f,"Game music loops at volume 0.3");
        Check(GameObject.FindGameObjectsWithTag("Coin").Length==5,"Five coins spawned");
        var rb=player.GetComponent<Rigidbody2D>();rb.position=new Vector2(0,-1);rb.linearVelocity=Vector2.zero;
        yield return new WaitForSecondsRealtime(.1f);
        Check(player.Score==1 && player.scoreText.text=="Ұпай: 1","Coin pickup updates TMP score");
        bool chime=false;
        foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            if(source.clip==player.coinSound && source.isPlaying) chime=true;
        Check(chime,"Coin pickup plays sound");
        var sr=player.GetComponent<SpriteRenderer>();var original=sr.color;
        for(int i=0;i<5;i++)
        {
            rb.position=new Vector2(9.25f,-1);rb.linearVelocity=Vector2.zero;
            yield return new WaitForSecondsRealtime(.06f);
            Check(player.health==80-i*20,"Collision damage "+(i+1));
            Check(sr.color==Color.red,"Damage blink starts red "+(i+1));
            yield return new WaitForSecondsRealtime(.25f);
            Check(sr.color==original,"Blink restores color "+(i+1));
            if(i<4) { rb.position=new Vector2(8.5f,-1);yield return new WaitForSecondsRealtime(.15f); }
        }
        Check(player.healthText.text=="Өмір: 0" && player.gameOverPanel.activeSelf,"Game Over panel and life text");
        Check(Time.timeScale==0,"Game pauses on zero health");
        Click("RestartButton");
        yield return new WaitForSecondsRealtime(1);
        player=Object.FindFirstObjectByType<Player>();
        Check(Time.timeScale==1 && player.health==100 && player.Score==0 && !player.gameOverPanel.activeSelf,"Restart resets time, health, score and panel");
        player.TakeDamage(100);
        Click("ToMenuButton");
        yield return new WaitForSecondsRealtime(.5f);
        Check(SceneManager.GetActiveScene().name=="MainMenu" && Time.timeScale==1,"Game Over Menu button resets time and loads MainMenu");
        Click("PlayButton");yield return new WaitForSecondsRealtime(.8f);
        Click("MenuButton");yield return new WaitForSecondsRealtime(.4f);
        Check(SceneManager.GetActiveScene().name=="MainMenu","In-game Menu button works");
        report.AppendLine("MODULE4_ALL_TESTS_PASSED");
        File.WriteAllText("module4-tests.txt",report.ToString());
        Debug.Log(report.ToString());
        UnityEditor.SessionState.SetBool("Module4Verify",false);
        Destroy(gameObject);
    }
}
#endif
