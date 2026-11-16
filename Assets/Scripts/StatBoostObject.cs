using UnityEngine;

public class StatBoostObject : CellObject
{
    public enum PotionType { Health, Attack, Defense, Mystery }

    [Header("Configuração da Poção")]
    public PotionType Type = PotionType.Health;

    [Header("Valores dos Bônus")]
    public int FoodBonus = 15;
    public int AttackBonus = 2;
    public int DefenseBonus = 1;

    // Permite que o jogador pise/entre na célula onde a poção está
    public override bool PlayerWantsToEnter()
    {
        return true;
    }

    // Executado assim que o jogador chega na célula da poção
    public override void PlayerEntered()
    {
        // Toca o som de bebida/poção antes de aplicar os bônus e destruir o item
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RandomizeSfx(SoundManager.Instance.drinkSound1, SoundManager.Instance.drinkSound2);
        }

        if (GameManager.Instance != null)
        {
            // Aplica o efeito correspondente ao tipo de poção escolhido no Inspetor
            switch (Type)
            {
                case PotionType.Health:
                    GameManager.Instance.ChangeFood(FoodBonus);
                    break;

                case PotionType.Attack:
                    GameManager.Instance.ChangeAttack(AttackBonus);
                    GameManager.Instance.ChangeFood(5); // Pequeno bônus de comida ao beber
                    break;

                case PotionType.Defense:
                    GameManager.Instance.ChangeDefense(DefenseBonus);
                    GameManager.Instance.ChangeFood(5); // Pequeno bônus de comida ao beber
                    break;

                case PotionType.Mystery:
                    GameManager.Instance.ChangeFood(FoodBonus);
                    GameManager.Instance.ChangeAttack(1);
                    GameManager.Instance.ChangeDefense(1);
                    break;
            }
        }

        Debug.Log($"Poção de {Type} Coletada! Comida: +{FoodBonus} | Ataque: +{AttackBonus} | Defesa: +{DefenseBonus}");

        // Remove o item do mapa após a coleta
        Destroy(gameObject);
    }
}