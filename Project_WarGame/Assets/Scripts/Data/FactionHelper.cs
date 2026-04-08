/// <summary>
/// 진영 간 적대 관계 판단 헬퍼.
/// Player ↔ Ally 는 우호 관계, Enemy 는 Player·Ally 양쪽과 적대.
/// </summary>
public static class FactionHelper
{
    public static bool IsHostileTo(Faction a, Faction b)
    {
        if (a == b) return false;
        if (a == Faction.Neutral || b == Faction.Neutral) return false;
        // 플레이어와 동맹군은 서로 우호
        if ((a == Faction.Player && b == Faction.Ally) ||
            (a == Faction.Ally   && b == Faction.Player)) return false;
        return true;
    }
}
