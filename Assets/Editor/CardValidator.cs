using System.Collections.Generic;

public static class CardValidator
{
    public class ValidationResult
    {
        public List<string> Errors = new List<string>();
        public bool HasErrors => Errors.Count > 0;
    }

    public static ValidationResult Validate(CardData card)
    {
        var result = new ValidationResult();

        // TODO: 补充你认为重要的校验规则，例如：
        // if (string.IsNullOrEmpty(card.cardName))
        //     result.Errors.Add("卡牌名称不能为空");

        // if (card.icon == null)
        //     result.Errors.Add("缺少图标");

        // if (card.cost < 0)
        //     result.Errors.Add("费用不能为负数");

        return result;
    }
}
