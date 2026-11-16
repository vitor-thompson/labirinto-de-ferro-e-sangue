using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public UIDocument UIDoc;

    public TurnManager TurnManager { get; private set; }

    [Header("Atributos do Jogador")]
    private int m_FoodAmount = 100;
    private int m_AttackAmount = 10;
    private int m_DefenseAmount = 2;

    // Referências para os Labels da UI Toolkit
    private Label m_FoodLabel;
    private Label m_AttackLabel;
    private Label m_DefenseLabel;

    private int m_CurrentLevel = 1;
    public int CurrentLevel => m_CurrentLevel;

    // Propriedades públicas para acesso de combate se necessário
    public int PlayerAttack => m_AttackAmount;
    public int PlayerDefense => m_DefenseAmount;

    // Painel e Elementos de Texto da Tela de Game Over
    private VisualElement m_GameOverPanel;
    private Label m_GameOverTitle;
    private Label m_RoomsSurvivedLabel;
    private Label m_RestartLabel;

    // Variável para controlar se o jogo está em tela de Game Over
    private bool m_IsGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        if (UIDoc != null && UIDoc.rootVisualElement != null)
        {
            m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
            m_AttackLabel = UIDoc.rootVisualElement.Q<Label>("AttackLabel");
            m_DefenseLabel = UIDoc.rootVisualElement.Q<Label>("DefenseLabel");

            m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");

            if (m_GameOverPanel != null)
            {
                m_GameOverTitle = m_GameOverPanel.Q<Label>("GameOverTitle");
                m_RoomsSurvivedLabel = m_GameOverPanel.Q<Label>("RoomsSurvivedLabel");
                m_RestartLabel = m_GameOverPanel.Q<Label>("RestartLabel");
            }
        }

        StartNewGame();
    }

    private void Update()
    {
        // Se estiver em Game Over e o jogador apertar Enter, reinicia a partida do zero (Permadeath)
        if (m_IsGameOver && UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        m_IsGameOver = false;

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.visibility = Visibility.Hidden;
        }

        // Garante que a música de fundo volta a tocar ao iniciar/recomeçar o jogo
        if (SoundManager.Instance != null && SoundManager.Instance.musicSource != null)
        {
            if (!SoundManager.Instance.musicSource.isPlaying)
            {
                SoundManager.Instance.musicSource.Play();
            }
        }

        m_CurrentLevel = 1;
        m_FoodAmount = 100;
        m_AttackAmount = 10;
        m_DefenseAmount = 2;

        UpdateUI();

        if (BoardManager != null)
        {
            BoardManager.Clean();
            BoardManager.Init();
        }

        if (PlayerController != null)
        {
            PlayerController.Init();
            PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        }

        UpdateCameraFraming();
    }

    public void NewLevel()
    {
        m_CurrentLevel++;

        if (BoardManager != null)
        {
            BoardManager.Clean();
            BoardManager.Init();
        }

        if (PlayerController != null)
        {
            PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        }

        UpdateCameraFraming();
    }

    void UpdateCameraFraming()
    {
        // Método mantido vazio para evitar erros de compilação caso use a câmera focada
    }

    private void OnTurnHappen()
    {
        if (!m_IsGameOver)
        {
            ChangeFood(-1);
        }
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        UpdateUI();

        if (m_FoodAmount <= 0)
        {
            m_IsGameOver = true;

            if (PlayerController != null)
            {
                PlayerController.GameOver();
            }

            if (m_GameOverPanel != null)
            {
                m_GameOverPanel.style.visibility = Visibility.Visible;
            }

            // Atualiza o texto de salas sobrevividas dinamicamente
            if (m_RoomsSurvivedLabel != null)
            {
                m_RoomsSurvivedLabel.text = "Você sobreviveu a: " + m_CurrentLevel + " salas";
            }

            // Para a música e toca o som de Game Over
            if (SoundManager.Instance != null)
            {
                if (SoundManager.Instance.musicSource != null)
                {
                    SoundManager.Instance.musicSource.Stop();
                }
                SoundManager.Instance.PlaySingle(SoundManager.Instance.gameOverSound);
            }
        }
    }

    public void ChangeAttack(int amount)
    {
        m_AttackAmount += amount;
        UpdateUI();
    }

    public void ChangeDefense(int amount)
    {
        m_DefenseAmount += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (m_FoodLabel != null)
        {
            m_FoodLabel.text = $"Room : {m_CurrentLevel} | Food : {m_FoodAmount} | Atk : {m_AttackAmount} | Def : {m_DefenseAmount}";
        }
    }
}