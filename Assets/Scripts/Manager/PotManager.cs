using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
public class PotManager : MonoBehaviour
{
    public List<CardData> cookingPot = new List<CardData>();

    public Image potPressureFill;
    public Image extraPotPressureFill;
    public Image extraExtraPotPressureFill;
    public Sprite Green, Yellow, Red;
    public TextMeshProUGUI pressureNum;
    public Transform potPanel;

    private void Start()
    {
        potPressureFill = GameObject.Find("PotPressureFill").GetComponent<Image>();  //找到压力表
        pressureNum = GameObject.Find("Pressure").GetComponent<TextMeshProUGUI>();
        UpdateTotalPressure();
    }

    // 添加卡牌到锅
    public void AddCardToPot(CardData card, GameObject cardObject)
    {
        if (card.isFrozen)
        {
            FindObjectOfType<FloatingHint>().ShowHint("卡牌被冻结，请先解冻！");
            return; //如果卡牌被冻结，不进入锅
        }
        cookingPot.Add(card);
        cardObject.transform.SetParent(potPanel);  //移动卡牌到锅面板
        cardObject.transform.DOScale(new Vector3(0.7f, 0.7f, 1), 0.3f); //将卡牌缩小
        Debug.Log($"Card {card.cardName} added to the pot.");
        UpdateTotalPressure();
    }

    // 移除卡牌并添加回手牌
    public void RemoveCardFromPot(CardData card, GameObject cardObject, HandManager handManager)
    {
        cookingPot.Remove(card);
        handManager.handCards.Add(card);
        cardObject.transform.SetParent(handManager.handPanel);  // 移动卡牌回手牌面板
        cardObject.transform.DOScale(new Vector3(1, 1, 1), 0.3f); // 将卡牌放大
        Debug.Log($"Card {card.cardName} returned to the hand.");
        UpdateTotalPressure();
    }


    // 清空锅中的所有卡牌
    public void ClearPot()
    {
        //遍历锅中的每张卡牌，销毁GameObject
        foreach (var card in cookingPot)
        {
            GameObject cardObject = GetCardObject(card);
            if (cardObject != null)
            {
                Destroy(cardObject);  //销毁GameObject
            }
        }
        // 清空锅的卡牌列表
        cookingPot.Clear();
        UpdateTotalPressure();
        Debug.Log("All cards cleared from the pot.");
    }

    // 获取卡牌的GameObject
    private GameObject GetCardObject(CardData card)
    {
        foreach (Transform child in potPanel)
        {
            CardUIHandler cardUIHandler = child.GetComponent<CardUIHandler>();
            if (cardUIHandler != null && cardUIHandler.cardData == card)
            {
                return child.gameObject;  //返回找到的卡牌GameObject(当锅中有多张重复牌的时候不一定会销毁指定的那一张，以后写多卡联动要注意)
            }
        }
        return null;
    }

    public float UpdateTotalPressure()
    {
        float totalPressure = 0f;
        //遍历锅中的每张卡牌，累加压力值
        foreach (var card in cookingPot)
        {
            totalPressure += card.pressure;
        }
        potPressureFill.fillAmount = totalPressure / 200f;
        pressureNum.text = totalPressure.ToString() + "%";
        //设置常规进度条的颜色
        if (totalPressure <= 60)
            potPressureFill.sprite = Green;
        else if (totalPressure <= 100)
            potPressureFill.sprite = Yellow;
        else
            potPressureFill.sprite = Red;
        //如果压力超过 200，更新超出部分的进度条
        if (totalPressure > 200)
        {
            extraPotPressureFill.fillAmount = (totalPressure - 200f) / 100f;
            extraPotPressureFill.gameObject.SetActive(true);  //激活额外进度条
        }
        else if (totalPressure <= 200)
        {
            extraPotPressureFill.gameObject.SetActive(false);  //压力不超过200时隐藏额外进度条
        }
        if(totalPressure > 300)
        {
            extraExtraPotPressureFill.fillAmount = (totalPressure - 300f) / 100f;
            extraExtraPotPressureFill.gameObject.SetActive(true);
        }else
            extraExtraPotPressureFill.gameObject.SetActive(false);
        return totalPressure;
    }
}
