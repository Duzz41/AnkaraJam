using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public GameObject pauseMenu; // Pause menüsünü tutan GameObject
    public Animator pauseMenuAnimator; // Pause menüsünün Animator bileşeni
    private bool isPaused = false; // Oyun duraklatma durumu

    void Update()
    {
        // ESC tuşuna basıldığında duraklatma işlemi
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGameMenu();
            }
        }
    }

    void PauseGameMenu()
    {
        // Zamanı durdur
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Locked;
        // Pause menüsünü aç
        //pauseMenu.SetActive(true);
        pauseMenuAnimator.SetTrigger("Open"); // Açılma animasyonunu başlat
    }

    public void ResumeGame()
    {
        // Zamanı devam ettir
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // Pause menüsünü kapat
        pauseMenuAnimator.SetTrigger("Close"); // Kapatma animasyonunu başlat
        StartCoroutine(DisableMenuAfterAnimation());
    }

    private System.Collections.IEnumerator DisableMenuAfterAnimation()
    {
        // Animasyon süresince bekle
        yield return new WaitForSeconds(pauseMenuAnimator.GetCurrentAnimatorStateInfo(0).length);
        //pauseMenu.SetActive(false); // Menü kapat
    }
}