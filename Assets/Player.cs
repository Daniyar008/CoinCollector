using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public int health = 100;
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public GameObject gameOverPanel;
    public AudioClip coinSound;
    public Slider healthSlider;
    public GameObject winPanel;
    public int totalCoins = 5;
    public TMP_Text resultText;
    public float Elapsed { get; private set; }
    public float BestTime { get; private set; }
    public bool HasEnded { get; private set; }
    SpriteRenderer sr;
    Color startColor;
    Coroutine blink;
    public int Score { get; private set; }
    public bool IsGrounded { get; private set; }
    Rigidbody2D rb;
    float horizontal;
    bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    void Start() { RefreshUI(); }

    void RefreshUI()
    {
        if (scoreText != null) scoreText.text = "Тиын: " + Score + " / " + totalCoins;
        if (healthText != null) healthText.text = "Өмір: " + health;
        if (healthSlider != null) healthSlider.value = health;
    }

    void Update()
    {
        if (HasEnded || Time.timeScale == 0f) { horizontal=0; jumpRequested=false; return; }
        Elapsed += Time.deltaTime;
        if(transform.position.y < -6f) TakeDamage(100);
        horizontal = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        if (jumpRequested) TryJump();
        jumpRequested = false;
    }

    public bool TryJump()
    {
        if (!IsGrounded || HasEnded || Time.timeScale == 0f) return false;
        IsGrounded = false;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        return true;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        CheckGround(c);
        if (c.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(20);
        }
    }

    void OnCollisionStay2D(Collision2D c) { CheckGround(c); }

    void CheckGround(Collision2D c)
    {
        if (!c.gameObject.CompareTag("Ground")) return;
        foreach (ContactPoint2D contact in c.contacts)
            if (contact.normal.y > 0.5f && rb.linearVelocity.y <= 0.1f)
                IsGrounded = true;
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground")) IsGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (HasEnded || !other.CompareTag("Coin") || !other.enabled) return;
        other.enabled = false;
        Score++;
        Debug.Log("Ұпай: " + Score);
        RefreshUI();
        if (coinSound != null) AudioSource.PlayClipAtPoint(coinSound, Camera.main.transform.position, .65f);
        Destroy(other.gameObject);
        if (Score >= totalCoins && winPanel != null)
        {
            HasEnded = true;
            string key = "BestTime_" + SceneManager.GetActiveScene().name;
            BestTime = PlayerPrefs.GetFloat(key, float.MaxValue);
            if (Elapsed < BestTime) { BestTime = Elapsed; PlayerPrefs.SetFloat(key, BestTime); PlayerPrefs.Save(); }
            if(resultText != null) resultText.text = "Уақыт: " + Elapsed.ToString("F1") + " с\nРекорд: " + BestTime.ToString("F1") + " с";
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        if (HasEnded) return;
        health = Mathf.Max(0, health - damage);
        Debug.Log("Өмір: " + health);
        RefreshUI();
        if (blink != null) StopCoroutine(blink);
        blink = StartCoroutine(Blink());
        if (health == 0)
        {
            HasEnded = true;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("Ойын аяқталды!");
        }
    }

    IEnumerator Blink()
    {
        sr.color = Color.red;
        // Ойын тоқтағанда да бастапқы түсіне оралады.
        yield return new WaitForSecondsRealtime(.2f);
        sr.color = startColor;
        blink = null;
    }

}

