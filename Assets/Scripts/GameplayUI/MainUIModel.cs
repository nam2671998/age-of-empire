public class MainUIModel
{
    private readonly Faction faction;

    public int Wood { get; private set; }
    public int Stone { get; private set; }
    public int Food { get; private set; }

    public MainUIModel(Faction faction)
    {
        this.faction = faction;
    }

    public bool Refresh()
    {
        int nextWood = PlayerResourceInventory.GetAmount(faction, ResourceType.Wood);
        int nextStone = PlayerResourceInventory.GetAmount(faction, ResourceType.Stone);
        int nextFood = PlayerResourceInventory.GetAmount(faction, ResourceType.Food);

        if (nextWood == Wood && nextStone == Stone && nextFood == Food)
        {
            return false;
        }

        Wood = nextWood;
        Stone = nextStone;
        Food = nextFood;
        return true;
    }
}

