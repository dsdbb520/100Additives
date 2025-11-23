using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        Resolution,
        EnemyTurn,
        EndTurn,
        Win,
        Lose
    }

    public BattleState currentState;

    void Start()
    {
        currentState = BattleState.Start;
        Debug.Log("Battle Started!");
        ChangeState(BattleState.PlayerTurn);
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;
        Debug.Log($"Battle state changed to {currentState}");

        // 根据状态做出不同反应
        switch (currentState)
        {
            case BattleState.Start:
                // 开始战斗时的逻辑
                break;
            case BattleState.PlayerTurn:
                // 玩家回合的逻辑
                break;
            case BattleState.Resolution:
                break;
            case BattleState.EnemyTurn:
                // 敌人回合的逻辑
                break;
            case BattleState.EndTurn:
                break;
            case BattleState.Win:
                // 战斗胜利的逻辑
                break;
            case BattleState.Lose:
                // 战斗失败的逻辑
                break;
                // 添加更多状态时，继续扩展
        }
    }
}
