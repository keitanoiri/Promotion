using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Option : MonoBehaviour
{
    public void ReturnTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitOption()
    {
        Destroy(gameObject);
    }

}
