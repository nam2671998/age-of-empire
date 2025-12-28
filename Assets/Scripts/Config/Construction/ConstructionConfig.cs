using UnityEngine;

public abstract class ConstructionConfig : ScriptableObject
{
    [SerializeField] private int constructionId;
    public int ConstructionId => constructionId;
}

