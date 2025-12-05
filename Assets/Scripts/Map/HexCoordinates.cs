using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    public int x; //q (列)
    public int y; //r (行)
    public int z; //s

    //构造函数：只需要x和y，z自动计算
    public HexCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.z = -x - y;
    }

    public static HexCoordinates Zero => new HexCoordinates(0, 0);

    //计算两个六边形之间的距离
    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }

    //将六边形坐标转换为世界坐标(用于放置 UI/物体)
    //size: 六边形的半径
    public Vector3 ToWorldPosition(float size)
    {
        float xPos = size * Mathf.Sqrt(3) * (x + y / 2f);
        float yPos = size * 3f / 2f * y;

        return new Vector3(xPos, yPos, 0);
    }

    //获取相邻的坐标
    public static HexCoordinates[] GetNeighbors(HexCoordinates center)
    {
        return new HexCoordinates[]
        {
            new HexCoordinates(center.x + 1, center.y),     //右
            new HexCoordinates(center.x + 1, center.y - 1), //右下
            new HexCoordinates(center.x, center.y - 1),     //左下
            new HexCoordinates(center.x - 1, center.y),     //左
            new HexCoordinates(center.x - 1, center.y + 1), //左上
            new HexCoordinates(center.x, center.y + 1)      //右上
        };
    }

    //重写 Equals 和 ToString 方便调试
    public override string ToString() => $"({x}, {y}, {z})";

    public override bool Equals(object obj) => obj is HexCoordinates other && this == other;
    public override int GetHashCode() => (x, y, z).GetHashCode();
    public static bool operator ==(HexCoordinates a, HexCoordinates b) => a.x == b.x && a.y == b.y && a.z == b.z;
    public static bool operator !=(HexCoordinates a, HexCoordinates b) => !(a == b);
}