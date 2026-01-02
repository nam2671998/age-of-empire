public sealed class RequirementUIModel
{
    public ResourceCost[] Current { get; private set; }

    public void Set(ResourceCost[] data)
    {
        Current = data;
    }
}

