using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Necessário para o novo Input System

public class PauseMenuController : MonoBehaviour
{
    [Header("Painel de Pausa")]
    public GameObject PausePanel;
    private bool m_IsPaused = false;

    void Update()
    {
        // Usa o novo Input System para detectar a tecla Esc de forma compatível
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (m_IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (PausePanel != null)
            PausePanel.SetActive(false);

        Time.timeScale = 1f; // Restaura o tempo normal do jogo
        m_IsPaused = false;
    }

    void Pause()
    {
        if (PausePanel != null)
            PausePanel.SetActive(true);

        Time.timeScale = 0f; // Congela o tempo
        m_IsPaused = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // IMPORTANTE: Destrava o tempo antes de trocar de cena
        SceneManager.LoadScene("mainmenuscene"); // Nome da sua cena do menu principal
    }
}