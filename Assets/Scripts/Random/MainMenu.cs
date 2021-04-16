using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject[] toEnable1;
    public GameObject[] toEnable2;
    public GameObject[] toDisable1;
    public GameObject[] toDisable2;

    public GameObject creditsMenu;

    public void startGame()
    {
        foreach (GameObject dis in toDisable1)
        {
            dis.SetActive(false);
        }
        foreach (GameObject en in toEnable1)
        {
            en.SetActive(true);
        }
    }

    public void confirmStartGame()
    {
        foreach (GameObject dis in toDisable2)
        {
            dis.SetActive(false);
        }
        foreach (GameObject en in toEnable2)
        {
            en.SetActive(true);
        }
    }

    public void credits()
    {
        creditsMenu.SetActive(!creditsMenu.activeInHierarchy);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void mainMenu()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
}
