using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform m_Target;
    private Camera m_Camera;
    private BoardManager m_BoardManager;

    public float SmoothSpeed = 10f;
    public Vector3 Offset = new Vector3(0f, 0f, -10f);

    [Header("Configuração de Visão Restrita")]
    public float ViewSize = 4f;

    void Awake()
    {
        m_Camera = GetComponent<Camera>();
    }

    void Start()
    {
        if (m_Camera != null)
        {
            m_Camera.orthographic = true;
            m_Camera.orthographicSize = ViewSize;
        }

        transform.position = new Vector3(5f, 5f, Offset.z);

        FindPlayer();
        m_BoardManager = FindFirstObjectByType<BoardManager>();
    }

    void LateUpdate()
    {
        if (m_Target == null)
        {
            FindPlayer();
            return;
        }

        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
            return;
        }

        Vector3 targetPosition = m_Target.position + Offset;

        // Pega a altura e largura reais do tabuleiro gerado
        float boardWidth = m_BoardManager.Width;
        float boardHeight = m_BoardManager.Height;

        // Calcula os limites corretos para a câmera não passar das bordas do tilemap
        float minX = m_Camera.orthographicSize;
        float maxX = boardWidth - m_Camera.orthographicSize;
        float minY = m_Camera.orthographicSize;
        float maxY = boardHeight - m_Camera.orthographicSize;

        // Aplica o travamento (clamp) correto considerando o tamanho total do mapa
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Move suavemente até a posição restrita dentro do mapa
        transform.position = Vector3.Lerp(transform.position, targetPosition, SmoothSpeed * Time.deltaTime);
    }

    void FindPlayer()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            m_Target = player.transform;
        }
    }
}