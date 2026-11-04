using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Player player;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void TogglePause()
    {
        // Жеңіс пен Game Over экрандарын Esc арқылы жабуға болмайды.
        if (player == null || player.HasEnded || pausePanel == null) return;
        bool paused = !pausePanel.activeSelf;
        pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        if (player == null || player.HasEnded) return;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
