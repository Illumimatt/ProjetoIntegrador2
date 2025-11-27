using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    
    /// <summary>
    /// Time in seconds before loading the main menu
    /// </summary>
    public float waitTime = 5.0f;

    void Start()
    {
        StartCoroutine(ToMainMenu());
    }

    IEnumerator ToMainMenu()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("LevelSelection");
    }
}