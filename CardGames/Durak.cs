using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

class Durak 
{
    //private bool gameRunning = true;
    private static Random rng = new Random();
    private Deck deck;
    private Deck discard;
    private Player you;
    private Player[] playerList;
    private List<Card?[]> Table;
    public static Suit Trump;
    private int Turn;

    public Durak(int numPlayers) {
        deck = new Deck("durak");
        deck.Shuffle();
        discard = new Deck("hand");

        you = new Player("You");
        playerList = new Player[numPlayers+1];
        playerList[0] = you;
        for (int i = 1; i < playerList.Length; i++) {
            playerList[i] = new Player();
        }

        Table = new List<Card?[]>();
        Trump = deck[35].suit;
        Turn = findFirstTurn();

        Play();
    }

    private void Play() {
        Deal();
        Console.WriteLine(you);
        Console.WriteLine(playerList[1]);
        Attack(you, 0);
        View();
        Defend(0, 0);
        View();
        Console.WriteLine(you);
    }

    private void Attack(Player attacker, int cardIndex) {
        Card?[] attack = new Card?[2];
        attack[0] = attacker[cardIndex];
        Table.Add(attack);
        attacker.Remove(cardIndex);
    }

    private void Defend(int attackIndex, int cardIndex) {
        Player defender = playerList[Turn];
        Card?[] attack = Table[attackIndex];
        Card? responseCard = defender[cardIndex];
        attack[1] = responseCard;
        playerList[Turn].Remove(cardIndex);
    }


    // The player to go first has the lowest trump card
    public int findFirstTurn() {
        Player firstPlayer = playerList[0];
        for (int i = 5; i < 13; i++) {
            foreach(Player player in playerList) {
                if (player.Hand.Contains(new Card(Trump, i))) {
                    firstPlayer = player;
                    return Array.IndexOf(playerList, firstPlayer);
                }
            }
        }
        return 0;
    }

    // Shows all attacks
    public void View() {
        int attacks = Table.Count;
        string[] attackArr = new string[attacks];

        for (int i = 0; i < attacks; i++) {
            Card? card1 = Table[i][0];
            Card? card2 = Table[i][1];

            if(card2.Equals(null)) {
                attackArr[i] = $"[{card1}, ______]";
            } else {
                attackArr[i] = $"[{card1}, {card2}]";
            }
        }

        string returnStr = "";
        for (int i = 0; i < attacks; i++) {
            if(i % 5 == 0 && i != 0) {
                returnStr += "\n\n";
            }
            returnStr += attackArr[i] + "  ";
        }
        Console.WriteLine(returnStr);
    }

    // Deal(-1) automatically deals 6 cards to every player
    // Use Deal(target, #cards) to deal a certain number of cards to target
    private void Deal(int target = -1, int numCards = 1) {
        if (target == -1) {
            for (int i = 0; i < 6; i++) {
                foreach (Player player in playerList) {
                    player.Hand.Add(deck.draw());
                }
            }
        } 

        else if (target >= 0 && target < playerList.Length) {
            for (int i = 0; i < numCards; i++) {
                playerList[target].Hand.Add(deck.draw());
            }
        }

        else {
            Console.WriteLine("Invalid target");
        }
    }

    private bool IsTrump(Card card) {
        return card.suit == Trump;
    }

    private bool Beats(Card Card1, Card Card2) {
        if (IsTrump(Card1) && !IsTrump(Card2)) {
            return true;
        } else if (!IsTrump(Card1) && IsTrump(Card2)) {
            return false;
        } else if (Card1.suit == Card2.suit) {
            return Card1.rank > Card2.rank;
        } else {
            return false;
        }
    }

}