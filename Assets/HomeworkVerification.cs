#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Text;

public class HomeworkVerification : MonoBehaviour
{
    StringBuilder report=new StringBuilder();
    string screenshots;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        if(!UnityEditor.SessionState.GetBool("HomeworkVerify",false) || Object.FindFirstObjectByType<HomeworkVerification>()!=null) return;
        var go=new GameObject("HomeworkVerification");DontDestroyOnLoad(go);go.AddComponent<HomeworkVerification>();
    }
    void Check(bool value,string label)
    {
        report.AppendLine((value?"PASS: ":"FAIL: ")+label);
        File.WriteAllText("homework-tests.txt",report.ToString());
        if(value)return;
        UnityEditor.SessionState.SetBool("HomeworkVerify",false);UnityEditor.EditorApplication.isPlaying=false;throw new System.Exception(label);
    }
    void Click(string name) { GameObject.Find(name).GetComponent<Button>().onClick.Invoke(); }
    IEnumerator Capture(string name)
    {
        yield return new WaitForEndOfFrame();
        var texture=ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Path.Combine(screenshots,name+".png"),texture.EncodeToPNG());Destroy(texture);
    }
    IEnumerator Start()
    {
        screenshots=Path.GetFullPath("../Homework_Screenshots");Directory.CreateDirectory(screenshots);
        yield return new WaitForSecondsRealtime(.5f);
        Check(SceneManager.GetActiveScene().name=="MainMenu","Main menu loads");
        yield return Capture("03_MainMenu");
        Click("PlayButton");yield return new WaitForSecondsRealtime(1.5f);
        var player=Object.FindFirstObjectByType<Player>();var rb=player.GetComponent<Rigidbody2D>();
        var pause=Object.FindFirstObjectByType<PauseManager>();
        Check(player.healthSlider!=null && player.healthSlider.maxValue==100 && player.healthSlider.minValue==0 && player.healthSlider.wholeNumbers && !player.healthSlider.interactable,"Slider configured 0..100, whole numbers, non-interactable");
        Check(player.healthSlider.value==100 && !player.winPanel.activeSelf && player.totalCoins==5,"Initial state and target five coins");
        var coins=GameObject.FindGameObjectsWithTag("Coin");
        Check(coins.Length==5,"Five coins spawned");
        var visual=coins[0].transform.Find("CoinVisual");var anim=visual.GetComponent<Animator>();
        Check(anim.runtimeAnimatorController!=null && anim.GetCurrentAnimatorStateInfo(0).loop,"CoinPulse Animator loops");
        float min=10,max=0;
        for(int i=0;i<24;i++) { min=Mathf.Min(min,visual.localScale.x);max=Mathf.Max(max,visual.localScale.x);yield return new WaitForSecondsRealtime(.05f); }
        Check(min<1.05f && max>1.25f && max<=1.31f,"Animation scales 1 to 1.3 and back");
        pause.TogglePause();Check(Time.timeScale==0 && pause.pausePanel.activeSelf,"Pause shows panel and freezes time");
        yield return new WaitForSecondsRealtime(.1f); // Завершить отложенную оценку Animator текущего кадра.
        var frozen=visual.localScale;yield return new WaitForSecondsRealtime(.2f);
        Check(Vector3.Distance(visual.localScale,frozen)<.0001f && Time.timeScale==0,"Coin animation freezes while paused ("+frozen+" -> "+visual.localScale+", time="+Time.timeScale+")");
        pause.TogglePause();Check(Time.timeScale==1 && !pause.pausePanel.activeSelf,"Second pause toggle resumes");
        pause.TogglePause();Click("ResumeButton");Check(Time.timeScale==1 && !pause.pausePanel.activeSelf,"Resume button works");
        rb.position=new Vector2(9.25f,-1);rb.linearVelocity=Vector2.zero;yield return new WaitForSecondsRealtime(.35f);
        Check(player.health==80 && player.healthSlider.value==80 && player.healthText.text=="Өмір: 80","Collision updates health text and slider to 80");
        Canvas.ForceUpdateCanvases();
        Check(Mathf.Abs(player.healthSlider.fillRect.rect.width-216)<.1f,"Visible slider fill is 80 percent of 270px");
        rb.position=new Vector2(0,-1);rb.linearVelocity=Vector2.zero;yield return new WaitForSecondsRealtime(.25f);
        Check(player.Score==1 && !player.winPanel.activeSelf,"One coin does not trigger victory");
        yield return Capture("01_Gameplay");
        for(int i=1;i<5;i++)
        {
            rb.position=new Vector2(i*2,-1);rb.linearVelocity=Vector2.zero;yield return new WaitForSecondsRealtime(.2f);
            Check(player.Score==i+1,"Coin collected "+(i+1));
        }
        Check(player.winPanel.activeSelf && player.HasEnded && Time.timeScale==0 && !player.gameOverPanel.activeSelf,"Five coins trigger only WinPanel and pause game");
        pause.TogglePause();Check(Time.timeScale==0 && !pause.pausePanel.activeSelf,"Esc cannot unpause victory");
        yield return Capture("02_Victory");
        Click("WinRestartButton");yield return new WaitForSecondsRealtime(1);
        player=Object.FindFirstObjectByType<Player>();pause=Object.FindFirstObjectByType<PauseManager>();
        Check(player.Score==0 && player.healthSlider.value==100 && Time.timeScale==1 && !player.winPanel.activeSelf,"Win restart resets everything");
        player.TakeDamage(100);yield return new WaitForSecondsRealtime(.25f);
        Check(player.healthSlider.value==0 && player.gameOverPanel.activeSelf && !player.winPanel.activeSelf,"Zero health empties slider and shows Game Over");
        Canvas.ForceUpdateCanvases();Check(player.healthSlider.fillRect.rect.width<.1f,"Zero health leaves zero visible fill width");
        pause.TogglePause();Check(Time.timeScale==0 && !pause.pausePanel.activeSelf,"Esc cannot unpause Game Over");
        Click("RestartButton");yield return new WaitForSecondsRealtime(.8f);
        player=Object.FindFirstObjectByType<Player>();rb=player.GetComponent<Rigidbody2D>();
        for(int i=0;i<5;i++){rb.position=new Vector2(i*2,-1);yield return new WaitForSecondsRealtime(.15f);}
        Click("WinMenuButton");yield return new WaitForSecondsRealtime(.5f);
        Check(SceneManager.GetActiveScene().name=="MainMenu" && Time.timeScale==1,"Win menu returns to MainMenu and restores time");
        Check(File.Exists(Path.Combine(screenshots,"01_Gameplay.png")) && File.Exists(Path.Combine(screenshots,"02_Victory.png")) && File.Exists(Path.Combine(screenshots,"03_MainMenu.png")),"Three submission screenshots saved");
        report.AppendLine("HOMEWORK_ALL_TESTS_PASSED");File.WriteAllText("homework-tests.txt",report.ToString());Debug.Log(report.ToString());
        UnityEditor.SessionState.SetBool("HomeworkVerify",false);Destroy(gameObject);
    }
}
#endif
