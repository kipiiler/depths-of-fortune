using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Going to scene 1");
        SceneManager.LoadScene(1);
    }

    public void MainMenu()
    {
        Debug.Log("Going to main menu");
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
