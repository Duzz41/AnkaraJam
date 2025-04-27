using UnityEngine;
using UnityEngine.SceneManagement;

public class Tuttu : MonoBehaviour
{
    public GameObject tut1;
    public GameObject tut2;

    int count = 0;
    public void Update()
    {
        if (Input.GetKeyDown("space"))
        {

            if (count == 0)
            {
                tut1.SetActive(false);
                tut2.SetActive(true);
                count = 1;
            }
            else
                SceneManager.LoadScene("MainScene");
        }
    }
}
