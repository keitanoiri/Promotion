using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

    public void LoadMainScene()
    {
        // MainSceneへ移動する
        SceneManager.LoadScene("MainScene");
    }

    public void Quit()
    {
        // ゲームを終了する
        Application.Quit();
    }

}
