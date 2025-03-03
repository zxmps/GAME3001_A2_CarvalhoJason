using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void PlayClicked()
    {
        SceneManager.LoadScene("Play Scene");
    }
}
