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


    private async Task GetMove() {
        int cardIndex = -2;
        switch (Turn) {
            case 0: // Player is initial attacker
                Console.WriteLine("Which card would you like to play?");
                cardIndex = await ChooseCard();
                Console.WriteLine($"You attack with {you[cardIndex]}");
                Attack(you, cardIndex);
                break;

            case 1: // Player is defender
                int attackIndex = await ChooseAttack();
                if (attackIndex == -1) {
                    Console.WriteLine("You pass");
                    return;
                }

                bool successfulDefense = false;
                while (!successfulDefense) {
                    cardIndex = await ChooseCard(true);
                    successfulDefense = Beats(you[cardIndex], Table[attackIndex][0]);
                    if (!successfulDefense) {
                        Console.WriteLine("You must play a card that beats the attack");
                    } 
                }
                Defend(attackIndex, cardIndex); 
                break;

                default: // Player is an attacker
                    Console.WriteLine("Which card would you like to play?");
                    cardIndex = await ChooseCard();
                    bool successfulAttack = false;

                    foreach (Card?[] attack in Table) {
                        if (you[cardIndex].rank == attack[0]?.rank) {
                            Console.WriteLine($"You attack with {you[cardIndex]}");
                            Attack(you, cardIndex);
                            successfulAttack = true;
                            break;
                        }

                        if (!attack[1].Equals(null) && you[cardIndex].rank == attack[1]?.rank) {
                            Console.WriteLine($"You attack with {you[cardIndex]}");
                            Attack(you, cardIndex);
                            successfulAttack = true;
                            break;
                        }
                    }
                    if (!successfulAttack) {
                        Console.WriteLine("That card can't be played!");
                    }
                break;
        }
    }

    // Lets me use Console.ReadLine() asynchronously (very niffty)
    private static async Task<string?> AsyncReadLine() {
        return await Task.Run(() => Console.ReadLine());
    } 

    private async Task<int> ChooseCard(bool returnable = false) {
        if (returnable) Console.WriteLine("Which card would you like to play? (Press z to return)");
        else Console.WriteLine("Which card would you like to play?");
        Console.WriteLine(you);
        string? rawInput = await AsyncReadLine();

        if(rawInput == "z" || rawInput == "Z") {
            return -1;
        } else {
            bool validInput = Int32.TryParse(rawInput, out int cardIndex);
            if (!validInput || cardIndex > you.Hand.Count() || cardIndex <= 0) {
                Console.WriteLine("Invalid card index");
                cardIndex = await ChooseCard();
            }
            cardIndex--;
            return cardIndex;
        }
    }



    private async Task<int> ChooseAttack() {
        Console.WriteLine("Which attack are you fending off? (Press enter to pass)");
        await Task.Delay(1000);
        View();
        string? rawInput = await AsyncReadLine();
        if (rawInput == "") {
            return -1;
        } else {
            int attackIndex = Convert.ToInt32(rawInput);
            while (attackIndex > Table.Count && attackIndex <= 0) {
                Console.WriteLine("Invalid attack index");
                attackIndex = Convert.ToInt32(Console.ReadLine());
            }
            return attackIndex;
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