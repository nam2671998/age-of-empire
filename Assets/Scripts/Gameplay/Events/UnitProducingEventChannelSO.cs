using UnityEngine;

[CreateAssetMenu(menuName = "Events/Unit Training Event Channel")]
public class UnitProducingEventChannelSO : EventChannelSO<UnitProducingData>
{
}

public struct UnitProducingData
{
    public int constructionId;
    public int unitId;
    
    public UnitProducingData(int constructionId, int unitId)
    {
        this.constructionId = constructionId;
        this.unitId = unitId;
    }
}