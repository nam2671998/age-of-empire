using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Constructions/Unit Produce Construction Config")]
public class UnitProduceConstructionConfig : ConstructionConfig
{
    [SerializeField] private int[] unitProducible;

    public int[] UnitProducible => unitProducible;
}