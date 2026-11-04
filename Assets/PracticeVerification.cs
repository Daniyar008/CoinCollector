#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Reflection;

public class PracticeVerification : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-practiceTest") >= 0)
            new GameObject("Verification").AddComponent<PracticeVerification>();
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        var player = Object.FindFirstObjectByType<Player>();
        var rb = player.GetComponent<Rigidbody2D>();
        Check(player.IsGrounded, "Player lands on Ground");
        Check(GameObject.FindGameObjectsWithTag("Coin").Length == 5, "Spawner creates five coins");
        typeof(Player).GetField("horizontal", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player, 1f);
        typeof(Player).GetMethod("FixedUpdate", BindingFlags.NonPublic|BindingFlags.Instance).Invoke(player, null);
        Check(Mathf.Abs(rb.linearVelocity.x-player.speed)<.01f, "Horizontal movement speed");
        Check(player.TryJump(), "Ground jump accepted");
        Check(!player.TryJump(), "Air jump rejected");
        yield return new WaitForSeconds(.2f);
        Check(rb.position.y > -.8f, "Jump moves player upward");
        yield return new WaitForSeconds(1.8f);
        rb.position = new Vector2(0, -1); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(.3f);
        Check(player.Score == 1 && GameObject.FindGameObjectsWithTag("Coin").Length == 4, "Coin collected and removed");
        rb.position = new Vector2(9.25f, -1); rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(.3f);
        Check(player.health == 80, "Enemy collision removes 20 health");
        rb.position = new Vector2(8, -1);
        yield return new WaitForSeconds(.3f);
        rb.position = new Vector2(9.25f, -1);
        yield return new WaitForSeconds(.3f);
        Check(player.health == 60, "Second enemy collision removes 20 health");
        Debug.Log("PRACTICE_TESTS_PASSED");
        UnityEditor.EditorApplication.Exit(0);
    }

    void Check(bool condition, string description)
    {
        if (condition) { Debug.Log("PASS: " + description); return; }
        Debug.LogError("FAIL: " + description);
        UnityEditor.EditorApplication.Exit(1);
        throw new System.Exception(description);
    }
}
#endif
