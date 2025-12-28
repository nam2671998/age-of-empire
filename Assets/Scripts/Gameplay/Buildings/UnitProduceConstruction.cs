using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UnitProduceConstruction : MonoBehaviour
{
    [SerializeField] private UnitProducingEventChannelSO onProduceUnit;
    [SerializeField] private ConstructionConfig constructionConfig;
    [SerializeField] private BoxCollider ownCollider;

    public ConstructionConfig ConstructionConfig => constructionConfig;

    private void OnEnable()
    {
        onProduceUnit.Register(ProduceUnit);
    }
    
    private void OnDisable()
    {
        onProduceUnit.Register(ProduceUnit);
    }

    private async void ProduceUnit(UnitProducingData data)
    {
        if (data.constructionId == constructionConfig.ConstructionId)
        {
            GameObject unit = await Addressables.InstantiateAsync($"Units/{data.unitId}.prefab");
            Bounds bounds = ownCollider.bounds;
            bounds.Expand(2);
            Vector3 spawnPos = bounds.min;
            spawnPos.y = 0;
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    spawnPos.z = bounds.min.z;
                    spawnPos.x = Random.Range(bounds.min.x, bounds.max.x);
                }
                else
                {
                    spawnPos.z = bounds.max.z;
                    spawnPos.x = Random.Range(bounds.min.x, bounds.max.x);
                }
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    spawnPos.x = bounds.min.x;
                    spawnPos.z = Random.Range(bounds.min.z, bounds.max.z);
                }
                else
                {
                    spawnPos.x = bounds.max.x;
                    spawnPos.z = Random.Range(bounds.min.z, bounds.max.z);
                }
            }
            unit.transform.position = spawnPos;
        }
    }
}
