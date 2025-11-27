using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    public float waitTime = 5.0f; // Time in seconds before loading menu

    void Start()
    {
        // Start the countdown as soon as the scene loads
        StartCoroutine(ToMainMenu());
    }

    IEnumerator ToMainMenu()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("StartScene");
    }
}