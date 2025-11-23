using UnityEngine;
using System.Collections.Generic;

public class PotManager : MonoBehaviour
{
    public List<CardData> cookingPot = new List<CardData>();

    public void AddCardToPot(CardData card)
    {
        cookingPot.Add(card);
        Debug.Log($"Card {card.cardName} added to the pot.");
    }

    public void ServeDish()
    {
        float totalPhyDamage = 0;
        float totalMenDamage = 0;

        foreach (var card in cookingPot)
        {
            totalPhyDamage += card.phyDamage;
            totalMenDamage += card.menDamage;
        }

        // ¼ÆËãÉËº¦²¢Çå¿Õ¹ø
        Debug.Log($"Dealing {totalPhyDamage} physical damage and {totalMenDamage} mental damage.");
        cookingPot.Clear();
    }
}
