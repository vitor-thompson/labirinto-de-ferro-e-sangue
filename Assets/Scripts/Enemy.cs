using UnityEngine;

public class Enemy : CellObject
{
    public int Health = 3;
    private int m_CurrentHealth;

    [Header("Configuração de Combate")]
    public int AttackDamage = 10; // Dano bruto que o inimigo causa ao atacar o player

    // Componente de animação e controle de movimento suave
    private Animator m_Animator;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    public float MoveSpeed = 5f; // Mesma velocidade do Player

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Inscreve o inimigo no sistema de turnos de forma segura
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
            GameManager.Instance.TurnManager.OnTick += TurnHappened;
        }
    }

    private void OnDestroy()
    {
        // Remove a inscrição do turno quando o inimigo for destruído
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);

        // Aumenta a vida do inimigo em +3 para cada sala avançada (Nível 1 = vida base, Nível 2 = +3, etc.)
        int levelBonus = 0;
        if (GameManager.Instance != null)
        {
            levelBonus = (GameManager.Instance.CurrentLevel - 1) * 3;
        }

        m_CurrentHealth = Health + levelBonus;
        Debug.Log($"{name} nasceu com {m_CurrentHealth} de vida na sala {GameManager.Instance.CurrentLevel}.");

        m_IsMoving = false;
        transform.position = GameManager.Instance.BoardManager.CellToWorld(coord);

        // Garante que a célula inicial saiba que este inimigo está nela
        var cellData = GameManager.Instance.BoardManager.GetCellData(coord);
        if (cellData != null)
        {
            cellData.ContainedObject = this;
        }
    }

    // O jogador ataca o inimigo ao tentar entrar na célula dele, usando o seu Atributo de Ataque
    public override bool PlayerWantsToEnter()
    {
        int playerDamage = 1;
        if (GameManager.Instance != null)
        {
            playerDamage = GameManager.Instance.PlayerAttack;
        }

        m_CurrentHealth -= playerDamage;
        Debug.Log(this.name + " sofreu " + playerDamage + " de dano! Vida restante: " + m_CurrentHealth);

        if (m_CurrentHealth <= 0)
        {
            // Limpa a referência da célula antes de destruir o inimigo
            var board = GameManager.Instance.BoardManager;
            var currentCell = board.GetCellData(m_Cell);
            if (currentCell != null && currentCell.ContainedObject == this)
            {
                currentCell.ContainedObject = null;
            }

            Destroy(gameObject);
        }
        return false; // Impede o jogador de andar para cima do inimigo instantaneamente
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        // Verifica se a célula é válida, passável e se não está ocupada por outro objeto (exceto ele mesmo)
        if (targetCell == null || !targetCell.Passable || (targetCell.ContainedObject != null && targetCell.ContainedObject != this))
        {
            return false;
        }

        // Remove o inimigo da célula atual com segurança
        var currentCell = board.GetCellData(m_Cell);
        if (currentCell != null && currentCell.ContainedObject == this)
        {
            currentCell.ContainedObject = null;
        }

        // Adiciona o inimigo na nova célula
        targetCell.ContainedObject = this;
        m_Cell = coord;

        // Configura o movimento suave
        m_MoveTarget = board.CellToWorld(coord);
        m_IsMoving = true;

        // Ativa a animação de movimento (Bool "Moving")
        if (m_Animator != null)
        {
            m_Animator.SetBool("Moving", true);
        }

        return true;
    }

    private void Update()
    {
        // Faz o inimigo deslizar suavemente até o destino
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;

                // Desliga a animação de movimento ao chegar
                if (m_Animator != null)
                {
                    m_Animator.SetBool("Moving", false);
                }
            }
        }
    }

    void TurnHappened()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerController == null) return;

        var playerCell = GameManager.Instance.PlayerController.Cell;
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;
        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        // Se estiver adjacente ao jogador, ataca!
        if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
        {
            if (m_Animator != null)
            {
                m_Animator.SetTrigger("Attack");
            }

            // Toca o som de impacto/dano no player quando o inimigo ataca
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySingle(SoundManager.Instance.playerHitSound);
            }

            int playerDefense = GameManager.Instance.PlayerDefense;
            int damageReceived = Mathf.Max(1, AttackDamage - playerDefense);

            GameManager.Instance.ChangeFood(-damageReceived);

            Debug.Log($"Inimigo atacou! Dano bruto: {AttackDamage} | Defesa do Player: {playerDefense} | Dano sofrido: {damageReceived}");
        }
        else
        {
            // Move-se em direção ao jogador priorizando o eixo com maior distância
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        return MoveTo(m_Cell + Vector2Int.down);
    }
}