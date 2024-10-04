class Player
{
    static int playerCount = 0;
    public string Name { get; set; }
    public Deck Hand { get; set; }
    public Card this[int index] => Hand[index];

    public override string ToString()
    {
        return Hand.ToString();
    }

    public void Remove(int index) {
        Hand.Remove(index);
    }

    public Player()
    {
        Name = $"Player {playerCount}";
        Hand = new Deck("hand");
        playerCount++;
    }

    public Player(string name) : this() {
        Name = name;
    }
}