using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
public class PotManager : MonoBehaviour
{
    public List<CardData> cookingPot = new List<CardData>();

    [Header("大锅参数")]
    public float heatMultiplier = 1.0f; //伤害倍率
    public bool stopPressureGrowth = false; //是否暂停压力自然增长
    public float explosionChanceModifier = 0f; //炸锅概率修正
    public bool ignoreExplosionDamage = false; //免疫炸锅伤害

    [Header("状态控制")]
    public bool canServe = true;  //是否允许上菜

    [Header("组件引用")]
    public Image potPressureFill;
    public Image extraPotPressureFill;
    public Image extraExtraPotPressureFill;
    public Image previewPressureFill;
    public Sprite Green, Yellow, Red;
    public TextMeshProUGUI pressureNum;
    public Transform potPanel;
    private SpecialEffectManager fxManager;


    private float temporaryPressure = 0f;

    

    private void Start()
    {
        potPressureFill = GameObject.Find("PotPressureFill").GetComponent<Image>();  //找到压力表
        pressureNum = GameObject.Find("Pressure").GetComponent<TextMeshProUGUI>();
        fxManager = SpecialEffectManager.Instance;
        UpdateTotalPressure();
    }

    // 添加卡牌到锅
    public void AddCardToPot(CardData card, GameObject cardObject)
    {
        if (card.isFrozen)
        {
            FloatingHint.Instance.ShowHint("卡牌被冻结，请先解冻！");
            return; //如果卡牌被冻结，不进入锅
        }

        if (card.targetType != CardTargetType.BigPot && card.targetType != CardTargetType.Dual)
        {
            Debug.LogError($"错误：试图将非大锅牌 {card.cardName} 加入大锅！");
            return;
        }

        float oldPressure = GetTotalPressure();
        cookingPot.Add(card);
        cardObject.transform.SetParent(potPanel);  //移动卡牌到锅面板
        cardObject.transform.DOScale(new Vector3(0.7f, 0.7f, 1), 0.3f); //将卡牌缩小
        Debug.Log($"Card {card.cardName} added to the pot.");
        UpdateTotalPressure();

        //执行特殊效果：压力增大时
        if (GetTotalPressure() > oldPressure)
        {
            if (fxManager != null) fxManager.OnPressureIncreased();
        }
        //执行特殊效果：加入锅中时
        if (!string.IsNullOrEmpty(card.specialEffectID))
        {
            SpecialEffectManager.Instance.ApplyEffect(card.specialEffectID, card, false, EffectTriggerPhase.OnAdd);
        }
        RelicManager.Instance.TriggerAllRelics(RelicTriggerType.OnPutIntoPot, card);
    }

    //移除卡牌并添加回手牌
    public void RemoveCardFromPot(CardData card, GameObject cardObject, HandManager handManager)
    {
        if (cookingPot.Contains(card))
        {
            cookingPot.Remove(card);
            if (handManager != null)
            {
                handManager.handCards.Add(card);
                cardObject.transform.SetParent(handManager.handPanel);
                cardObject.transform.DOScale(Vector3.one, 0.3f);
            }
            UpdateTotalPressure();
        }
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
        temporaryPressure = 0;
        ResetRoundStatus();
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


    //直接增加压力
    public void AddDirectPressure(float amount)
    {
        if (amount > 0 && fxManager != null) fxManager.OnPressureIncreased();
        temporaryPressure += amount;
        UpdateTotalPressure();
    }

    public float GetTotalPressure()
    {
        float total = temporaryPressure;
        foreach (var card in cookingPot) total += card.pressure;
        return total;
    }

    public float UpdateTotalPressure()
    {
        float totalPressure = temporaryPressure;
        //遍历锅中的每张卡牌，累加压力值
        foreach (var card in cookingPot)
        {
            totalPressure += card.pressure;
            if (card.specialEffectID == "LiquidNitrogen")  //清空液氮之前的所有卡的压力
            {
                totalPressure = 0;
            }
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

        //计算预览压力
        if (previewPressureFill != null)
        {
            float projectedAdd = 0f;

            foreach (var c in cookingPot)
            {
                if (fxManager != null)
                {
                    projectedAdd += fxManager.GetOnServePressurePrediction(c);
                }
            }

            float finalProjectedPressure = totalPressure + projectedAdd;

            //更新预览条
            previewPressureFill.fillAmount = finalProjectedPressure / 200f;

            if (Mathf.Abs(projectedAdd) > 0.1f)
            {
                previewPressureFill.gameObject.SetActive(true);

                if (finalProjectedPressure > 100f && totalPressure <= 100f)
                {
                    //TODO：加一些视觉警告
                }
            }
        }
        return totalPressure;
    }

    public IEnumerator PlayCookingAnimation(float duration = 2.0f)
    {
        List<GameObject> cardObjs = new List<GameObject>();
        List<CanvasGroup> cardCGs = new List<CanvasGroup>();

        foreach (var card in cookingPot)
        {
            GameObject obj = GetCardObject(card);
            if (obj != null)
            {
                cardObjs.Add(obj);

                CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = obj.AddComponent<CanvasGroup>();
                cardCGs.Add(canvasGroup);

                obj.transform.DOKill();

                //移动到锅中心
                obj.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
                //随机旋转
                obj.transform.DORotate(new Vector3(0, 0, Random.Range(-30f, 30f)), 0.5f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        float shakeDuration = duration - 1.0f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;

            //震动强度曲线：前期弱，后期呈指数级增强
            float currentStrength = Mathf.Lerp(5f, 50f, progress * progress);
            foreach (var obj in cardObjs)
            {
                if (obj != null)
                {
                    //随机偏移
                    Vector3 randomOffset = (Vector3)UnityEngine.Random.insideUnitCircle * currentStrength;
                    obj.transform.localPosition = randomOffset;
                }
            }
            yield return null;
        }
        foreach (var canvasGroup in cardCGs)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad);
            }
        }
        yield return new WaitForSeconds(0.5f); //等待完全消失
    }

    //统计锅里有多少张特定的标签
    public int CountTagsInPot(TagType targetTag)
    {
        int count = 0;
        foreach (var card in cookingPot)
        {
            if (card.tags.Contains(targetTag))
            {
                count++;
            }
        }
        return count;
    }

    //修改伤害倍率
    public void ModifyHeatMultiplier(float amount)
    {
        heatMultiplier += amount;
        Debug.Log($"大锅热度倍率变更: {heatMultiplier}");
    }

    //获取炸锅概率
    public float GetExplosionChance(float excessPressure)
    {
        //基础概率+修正值
        float chance = Mathf.Clamp(excessPressure / 20f, 0f, 1f);
        return Mathf.Clamp01(chance + explosionChanceModifier);
    }

    //移除最后一张加入的牌
    public void RemoveLastCard(HandManager handManager)
    {
        if (cookingPot.Count > 0)
        {
            CardData cardToRemove = cookingPot[cookingPot.Count - 1];
            GameObject cardObj = GetCardObject(cardToRemove);
            RemoveCardFromPot(cardToRemove, cardObj, handManager);
            FloatingHint.Instance.ShowHint($"移除了{cardToRemove.cardName}");
        }
        else
        {
            FloatingHint.Instance.ShowHint("锅是空的！");
        }
    }

    public void DisableServing()
    {
        canServe = false;
        Debug.Log("本回合禁止上菜！");
    }

    //重置状态
    public void ResetRoundStatus()
    {
        heatMultiplier = 1.0f;
        canServe = true;
        stopPressureGrowth = false;
        explosionChanceModifier = 0f;
    }
}
