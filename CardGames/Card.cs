enum Suit { Spades, Hearts, Diamonds, Clubs };
enum Rank { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King };

struct Card {
    public Suit suit;
    public Rank rank;

    public Card(Suit suit, Rank rank) {
        this.suit = suit;
        this.rank = rank;
    }

    public Card(string suit, int rank) {
        this.suit = (Suit)Enum.Parse(typeof(Suit), suit);
        this.rank = (Rank)rank;
    }

    public Card(Suit suit, int rank) {
        this.suit = suit;
        this.rank = (Rank)rank;
    }

    public override string ToString() {
        return rank + " of " + suit;
    }


}
