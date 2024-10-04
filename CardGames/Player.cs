class Player
{
    static int playerCount = 0;
    public string Name { get; set; }
    public Deck Hand { get; set; }
    public Card this[int index] => Hand[index];
    public int TurnId { get; set; }
    public bool StartedTurn { get; set; }

    public override string ToString()
    {
        string returnStr = $"{Name}:\n";
        returnStr += Hand.ToString();
        return returnStr;
    }

    public void Remove(int index) {
        Hand.Remove(index);
    }

    public Player()
    {
        Name = $"Player {playerCount}";
        Hand = new Deck("hand");
        StartedTurn = false;
        TurnId = playerCount;
        playerCount++;
    }

    public Player(string name) : this() {
        Name = name;
    }
}