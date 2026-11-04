using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

// Runs only when explicitly launched with --verify-module5. Uses actual physics triggers and UI callbacks.
public class Module5Verification : MonoBehaviour {
    StringBuilder report=new StringBuilder();string folder;float[] saved=new float[3];bool[] had=new bool[3];
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot(){if(!System.Environment.GetCommandLineArgs().Contains("--verify-module5"))return;var go=new GameObject("Module5Verification");DontDestroyOnLoad(go);go.AddComponent<Module5Verification>();}
    void Check(bool ok,string label){report.AppendLine((ok?"PASS ":"FAIL ")+label);File.WriteAllText(Path.Combine(folder,"tests.txt"),report.ToString());if(!ok){Restore();Application.Quit(2);throw new System.Exception(label);}}
    void Click(string name){GameObject.Find(name).GetComponent<Button>().onClick.Invoke();}
    void Restore(){for(int i=0;i<3;i++){string key="BestTime_"+(i==0?"Game":"Level"+(i+1));if(had[i])PlayerPrefs.SetFloat(key,saved[i]);else PlayerPrefs.DeleteKey(key);}PlayerPrefs.Save();}
    IEnumerator Capture(string name){yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,name+".png"),t.EncodeToPNG());Destroy(t);}
    IEnumerator Collect(Player p){var rb=p.GetComponent<Rigidbody2D>();foreach(var coin in GameObject.FindGameObjectsWithTag("Coin").OrderBy(c=>c.transform.position.x)){rb.position=coin.transform.position;rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSecondsRealtime(.12f);}}
    IEnumerator Start(){
        folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Module5_Verification"));Directory.CreateDirectory(folder);
        for(int i=0;i<3;i++){string key="BestTime_"+(i==0?"Game":"Level"+(i+1));had[i]=PlayerPrefs.HasKey(key);saved[i]=PlayerPrefs.GetFloat(key);PlayerPrefs.SetFloat(key,99999);}
        yield return new WaitForSecondsRealtime(.5f);Check(SceneManager.GetActiveScene().name=="MainMenu","Build starts at MainMenu");Click("PlayButton");
        for(int level=1;level<=3;level++){
            yield return new WaitForSecondsRealtime(.6f);var p=Object.FindFirstObjectByType<Player>();
            Check(SceneManager.GetActiveScene().name==(level==1?"Game":"Level"+level),"Scene sequence level "+level);
            Check(GameObject.FindGameObjectsWithTag("Coin").Length==p.totalCoins && p.totalCoins==(level==1?5:level==2?8:10),"Correct coin target level "+level);
            var pause=Object.FindFirstObjectByType<PauseManager>();pause.TogglePause();float elapsed=p.Elapsed;yield return new WaitForSecondsRealtime(.15f);Check(p.Elapsed==elapsed && Time.timeScale==0,"Pause excludes time from record");pause.ResumeGame();
            if(level==2)yield return Capture("02_Gameplay");
            yield return Collect(p);
            Check(p.HasEnded&&p.winPanel.activeSelf&&Time.timeScale==0,"Actual coin triggers win level "+level);
            Check(p.resultText.text.Contains("Рекорд")&&p.BestTime<99999,"Result UI and saved record level "+level);
            Check(PlayerPrefs.GetFloat("BestTime_"+SceneManager.GetActiveScene().name)==p.BestTime,"Record persisted level "+level);
            if(level<3){Check(GameObject.Find("NextLevelButton")!=null,"Next button exists");Click("NextLevelButton");}
            else {Check(GameObject.Find("NextLevelButton")==null,"Final level has no Next button");yield return Capture("03_Victory");Click("WinMenuButton");}
        }
        yield return new WaitForSecondsRealtime(.5f);Check(SceneManager.GetActiveScene().name=="MainMenu","Victory menu returns");Check(Object.FindFirstObjectByType<MenuManager>().recordText.text.Contains("с"),"Menu shows level one record");yield return Capture("01_MenuRecord");
        PlayerPrefs.SetFloat("BestTime_Game",.001f);Click("PlayButton");yield return new WaitForSecondsRealtime(.5f);var player=Object.FindFirstObjectByType<Player>();yield return Collect(player);Check(PlayerPrefs.GetFloat("BestTime_Game")==.001f,"Slower run preserves faster record");
        Click("WinRestartButton");yield return new WaitForSecondsRealtime(.5f);player=Object.FindFirstObjectByType<Player>();Check(player.Score==0&&player.health==100&&Time.timeScale==1,"Restart resets state");player.TakeDamage(100);Check(player.gameOverPanel.activeSelf&&player.healthSlider.value==0,"Death and health slider retained");
        Restore();report.AppendLine("ALL MODULE 5 CHECKS PASSED");File.WriteAllText(Path.Combine(folder,"tests.txt"),report.ToString());Application.Quit(0);
    }
}
