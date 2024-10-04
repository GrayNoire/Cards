class Deck
{
    private List<Card> cards = new List<Card>();
    private Random random = new Random();

    public Card this[int index] => cards[index];

    public override string ToString()
    {
        string deckString = "";
        for(int i = 0; i < cards.Count; i++) {
            deckString += $"[{i+1}] {cards[i]}\n";
        }
        return deckString;
    }

    public void Shuffle() {
        for (int i = 0; i < cards.Count; i++) {
            int j = random.Next(i, cards.Count);
            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }

    public Card draw() {
        Card card = cards[0];
        cards.RemoveAt(0);
        return card;
    }
    
    public int GetIndex(Card card) {
        return cards.IndexOf(card);
    }

    public bool Contains(Card card) {
        return cards.Contains(card);
    }

    public void Add(Card card) {
        cards.Add(card);
    }

    public void Remove(int index) {
        cards.RemoveAt(index);
    }

    public int Count() {
        return cards.Count;
    }

    public Deck(string type = "standard") {
        if (type == "standard") {
            for (int suit = 0; suit < 4; suit++) {
                for (int rank = 0; rank < 13; rank++) {
                    cards.Add(new Card((Suit)suit, (Rank)rank));
                }
            }
        }

        if (type == "durak") {
            for (int suit = 0; suit < 4; suit++) {
                cards.Add(new Card((Suit)suit, Rank.Ace));
                for (int rank = 5; rank < 13; rank++) {
                    cards.Add(new Card((Suit)suit, (Rank)rank));
                }
            }
        }

        if (type == "hand") {
            cards = new List<Card>();
        }
    }


}