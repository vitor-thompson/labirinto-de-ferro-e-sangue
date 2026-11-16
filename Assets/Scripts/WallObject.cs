using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile ObstacleTile;
    public Tile DamagedTile; // Tile exibido quando restar apenas 1 HP
    public int MaxHealth = 3;
    private int m_HealthPoint;
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        m_HealthPoint = MaxHealth;

        // Salva o tile original do piso e aplica o tile da parede
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        m_HealthPoint -= 1;

        // Se a vida cair para 1, troca para o tile danificado
        if (m_HealthPoint == 1 && DamagedTile != null)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_Cell, DamagedTile);
        }

        // Se ainda tiver vida, continua bloqueando a entrada
        if (m_HealthPoint > 0)
        {
            return false;
        }

        // Se a vida zerou: restaura o piso original e libera a célula
        GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);

        BoardManager.CellData data = GameManager.Instance.BoardManager.GetCellData(m_Cell);
        if (data != null)
        {
            data.Passable = true;
            data.ContainedObject = null;
        }

        Destroy(gameObject);
        return true;
    }
}