using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

    public void LoadMainScene()
    {
        // MainScene‚ÖˆÚ“®‚·‚é
        SceneManager.LoadScene("MainScene");
    }

    public void Quit()
    {
        // ƒQ[ƒ€‚ğI—¹‚·‚é
        Application.Quit();
    }

}
