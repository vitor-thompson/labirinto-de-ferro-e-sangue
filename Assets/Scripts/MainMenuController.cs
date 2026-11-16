using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Chamado pelo botão "Jogar"
    public void StartGame()
    {
        // Tem que estar exatamente "Main" 
        SceneManager.LoadScene("Main");
    }

    // Chamado pelo botão "Sair"
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}