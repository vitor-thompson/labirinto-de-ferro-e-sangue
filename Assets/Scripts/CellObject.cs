using UnityEngine;

public class CellObject : MonoBehaviour
{
    protected Vector2Int m_Cell;

    // Método virtual de inicialização da célula
    public virtual void Init(Vector2Int cell)
    {
        m_Cell = cell;
    }

    // Método virtual que define se o jogador pode entrar na célula (padrão é true)
    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }

    // Chamado quando o jogador entra na célula
    public virtual void PlayerEntered()
    {
    }
}