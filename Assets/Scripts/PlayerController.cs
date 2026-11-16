using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Propriedade para acessar a célula atual do jogador
    public Vector2Int Cell => m_CellPosition;

    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;

    // Variáveis para animação e movimento suave
    private Animator m_Animator;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    public float MoveSpeed = 5f;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Init()
    {
        m_IsGameOver = false;
        m_IsMoving = false;
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell, true);
    }

    public void MoveTo(Vector2Int cell, bool immediate)
    {
        m_CellPosition = cell;
        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);

            // Toca o som de passos aleatório ao iniciar um movimento suave
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.RandomizeSfx(SoundManager.Instance.moveSound1, SoundManager.Instance.moveSound2);
            }
        }

        if (m_Animator != null)
        {
            m_Animator.SetBool("Moving", m_IsMoving);
        }
    }

    public void Attack()
    {
        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Attack");
        }

        // Toca o som de ataque/machado
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySingle(SoundManager.Instance.attackSound);
        }
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }

        // Se estiver em movimento suave entre células, aguarda chegar ao destino
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                if (m_Animator != null)
                {
                    m_Animator.SetBool("Moving", false);
                }

                var cellData = m_Board.GetCellData(m_CellPosition);
                if (cellData != null && cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }
            }
            return;
        }

        if (Keyboard.current == null) return;

        // 1. Verificação de Espera de Turno (Tecla Espaço)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log(">>> TURNO PASSADO PELA TECLA ESPAÇO <<<");
            GameManager.Instance.TurnManager.Tick();
            return;
        }

        // 2. Verificação de Movimento (Setas do Teclado)
        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }

        if (hasMoved)
        {
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            if (cellData != null)
            {
                if (!cellData.Passable)
                    return;

                GameManager.Instance.TurnManager.Tick();

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget, false);
                }
                else
                {
                    // Tenta interagir com o objeto na célula
                    if (cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        MoveTo(newCellTarget, false);
                    }
                    else
                    {
                        // Se não pôde entrar (ex: inimigo ou obstáculo), dispara a animação e o som de ataque
                        Attack();
                    }
                }
            }
        }
    }
}