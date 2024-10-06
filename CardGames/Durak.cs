using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

enum Move { }

class Durak 
{
    //private bool gameRunning = true;
    private string Winner;
    private static Random rng = new Random();
    private Deck deck;
    private Deck discard;
    private Player you;
    private Player[] playerList;
    private List<Card?[]> Table;
    public static Suit Trump;
    private int Turn;

    public Durak(int numPlayers) {
        Winner = "";
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
        Turn = FindFirstTurn();

        Deal();
        Console.WriteLine(playerList[0]);
        Console.WriteLine(playerList[1]);
    }

    public async Task Play() {
        while (Winner == "") {
            // Console.Clear();
            Console.WriteLine($"Trump: {Trump}");
            Console.WriteLine($"It's {playerList[Turn].Name}'s turn to attack\n");

            Turn = 0;
            bool cant = await GetAiInput(playerList[0]);
            // Turn = 1;
            await GetAiInput(playerList[1]);
            break;
        }  
    }

    public async Task PlayRound() {
        Task[] tasks = new Task[playerList.Length];

    }


    private async Task GetMove() {
        // Player is the initial attacker
        if (Turn == 0) {
            Console.WriteLine("Which card would you like to play?");
            int cardIndex = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine($"You attack with {you[cardIndex]}");
            Attack(you, cardIndex);
        } else { // Player is the defender
            if (Turn == 1) {
                Console.WriteLine("Which attack are you fending off?");
                View();
                int attackIndex = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Which card would you like to play?");
                Console.WriteLine(you);
                int cardIndex;
                bool successfulDefense = false;
                while (!successfulDefense) {
                    cardIndex = Convert.ToInt32(Console.ReadLine());
                    successfulDefense = Beats(you[cardIndex], Table[attackIndex][0]);
                    if (!successfulDefense) {
                        Console.WriteLine("You must play a card that beats the attack");
                    }
                }
            } else {
                Console.WriteLine("Which card would you like to play?");
                int cardIndex = Convert.ToInt32(Console.ReadLine());
                Attack(playerList[2], cardIndex);
            }
        }
    }
    

    // Handles the AI's moves
    // Returns true if the AI successfully attacks or defends
    private async Task<bool> GetAiInput(Player player) {
        await Task.Delay(1000);

        // Player is the initial attacker
        if (Turn == player.TurnId && !player.StartedTurn) {
            Console.WriteLine($"{player.Name} is attacking");
            player.StartedTurn = true;
            int cardIndex = rng.Next(0, player.Hand.Count());
            Attack(player, cardIndex);
            View();
            return true;
        }

        // Player defends
        else if ((Turn+1) % playerList.Length == player.TurnId) {
            Console.WriteLine($"{player.Name} is defending");
            bool successfulDefense = false;
            for(int i = 0; i < Table.Count; i++) {
                if (Table[i][1].Equals(null)) {
                    for (int j = 0; j < player.Hand.Count(); j++) {
                        if (Beats(player[j], Table[i][0])) {
                            Defend(Table.IndexOf(Table[i]), j);
                            View();
                            break;
                        }
                    }
                } 
            }

            foreach (Card?[] attack in Table) {
                if (attack[1].Equals(null)) {
                    successfulDefense = false;
                }
            }
            successfulDefense = true;

            if (!successfulDefense) {
                Console.WriteLine($"{player.Name} can't respond");
            }
            return successfulDefense;
        }

        // Subsequent attacks
        else {
            bool attacked = false;
            foreach (Card?[] attack in Table) {
                if (FullAttack(attack)) {
                    for (int i = 0; i < player.Hand.Count(); i++) {
                        if (attack[0]?.rank == player[i].rank){ Attack(player, i); attacked = true; }
                        if (attack[1]?.rank == player[i].rank) { Attack(player, i); attacked = true; }
                    }
                } else {
                    for (int i = 0; i < player.Hand.Count(); i++) {
                        if (attack[0]?.rank == player[i].rank) { Attack(player, i); attacked = true; }
                    }
                }
            }
            if(!attacked) { Console.WriteLine($"{player.Name} passes"); }
            return attacked;
        }
    }

    public bool FullAttack(Card?[] attack) {
        if (attack[1].Equals(null)) {
            return false;
        } else {
            return true;
        }
    }

    private void Attack(Player attacker, int cardIndex) {
        Card?[] attack = new Card?[2];
        attack[0] = attacker[cardIndex];
        Console.WriteLine($"{attacker.Name} attacks with {attack[0]}");
        Table.Add(attack);
        attacker.Remove(cardIndex);
    }

    private void Defend(int attackIndex, int cardIndex) {
        Player defender = playerList[(Turn+1) % playerList.Length];
        Card?[] attack = Table[attackIndex];
        Card? responseCard = defender[cardIndex];
        Console.WriteLine($"{defender.Name} defends with {responseCard}");
        attack[1] = responseCard;
        playerList[Turn].Remove(cardIndex);
    }


    // The player to go first has the lowest trump card
    public int FindFirstTurn() {
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
            Console.WriteLine("Dealing cards");
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

    private bool IsTrump(Card? card) {
        return card?.suit == Trump;
    }

    private bool Beats(Card? Card1, Card? Card2) {
        if (IsTrump(Card1) && !IsTrump(Card2)) {
            return true;
        } else if (!IsTrump(Card1) && IsTrump(Card2)) {
            return false;
        } else if (Card1?.suit == Card2?.suit) {
            return Card1?.rank > Card2?.rank;
        } else {
            return false;
        }
    }

}