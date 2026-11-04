using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public TMPro.TMP_Text recordText;
    void Start() {
        if(recordText == null) return;
        float best=PlayerPrefs.GetFloat("BestTime_Game",0);
        recordText.text=best>0 ? "Рекорд: " + best.ToString("F1") + " с" : "Рекорд жоқ";
    }
    public void NextLevel() {
        int next=SceneManager.GetActiveScene().buildIndex+1;
        if(next>=SceneManager.sceneCountInBuildSettings) return;
        Time.timeScale=1f; SceneManager.LoadScene(next);
    }
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Шығу");
    }
}
