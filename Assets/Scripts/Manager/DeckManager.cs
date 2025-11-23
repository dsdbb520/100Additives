using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> handPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    public void Shuffle()
    {
        // Ï´ÅÆÂß¼­
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardData temp = drawPile[i];
            int randomIndex = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0)
            {
                // Èç¹û³éÅÆ¶ÑÎª¿Õ£¬½«ÆúÅÆ¶ÑÖØÐÂÏ´Èë³éÅÆ¶Ñ
                discardPile.AddRange(drawPile);
                Shuffle();
            }

            if (drawPile.Count > 0)
            {
                CardData card = drawPile[0];
                drawPile.RemoveAt(0);
                handPile.Add(card);
            }
        }
    }

    public void DiscardHand()
    {
        // Çå¿ÕÊÖÅÆ
        discardPile.AddRange(handPile);
        handPile.Clear();
    }
}
