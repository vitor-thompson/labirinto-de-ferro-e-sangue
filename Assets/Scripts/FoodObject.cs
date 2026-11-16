using UnityEngine;

public class FoodObject : CellObject
{
    public int AmountGranted = 10; // Valor customizável no Inspector

    [Header("Tipo de Item")]
    public bool isDrink = false; // Marque esta caixa no Inspector se o item for uma bebida (Soda)

    public override void PlayerEntered()
    {
        // Toca o som correspondente antes de destruir o objeto
        if (SoundManager.Instance != null)
        {
            if (isDrink)
            {
                SoundManager.Instance.RandomizeSfx(SoundManager.Instance.drinkSound1, SoundManager.Instance.drinkSound2);
            }
            else
            {
                SoundManager.Instance.RandomizeSfx(SoundManager.Instance.eatSound1, SoundManager.Instance.eatSound2);
            }
        }

        Destroy(gameObject);

        // Adiciona a quantidade de comida ao GameManager
        GameManager.Instance.ChangeFood(AmountGranted);
    }
}