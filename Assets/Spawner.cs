using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject coinPrefab;

    void Start()
    {
        // Start ойын басталғанда бір рет орындалады.
        // i: 0, 1, 2, 3, 4 — барлығы бес қайталау.
        for (int i = 0; i < 5; i++)
        {
            // Әр тиын арасы 2 бірлік: x = 0, 2, 4, 6, 8.
            Instantiate(coinPrefab, new Vector3(i * 2f, -1f, 0f), Quaternion.identity);
        }
    }
}
