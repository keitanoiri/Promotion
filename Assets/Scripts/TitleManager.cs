using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

    public void LoadMainScene()
    {
        // MainScene�ֈړ�����
        SceneManager.LoadScene("MainScene");
    }

    public void Quit()
    {
        // �Q�[�����I������
        Application.Quit();
    }

}
