using UnityEngine;

public class TurnManager
{
    public event System.Action OnTick; // Evento que será chamado a cada turno avançado
    private int m_TurnCount;

    // Construtor: inicia a contagem de turnos em 1
    public TurnManager()
    {
        m_TurnCount = 1;
    }

    // Avança o turno em 1 e exibe no Console
    public void Tick()
    {
        m_TurnCount += 1;
        Debug.Log("Current turn count : " + m_TurnCount);
        
        // Dispara o evento chamando todos os métodos inscritos nele
        OnTick?.Invoke();
    }
}