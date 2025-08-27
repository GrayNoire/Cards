using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;

class Durak 
{
    //private bool gameRunning = true;
    private string Winner;
    private readonly static Random rng = new();
    private readonly Deck deck;
    private readonly Deck discard;
    private readonly Player you;
    private readonly Player[] playerList;
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
        // For debugging - DELETE
        Console.WriteLine(playerList[0]);
        Console.WriteLine(playerList[1]);
    }

    public async Task Play() {
        Task<int>[] tasks = new Task<int>[playerList.Length];
        int[] taskResults = new int[playerList.Length];
        bool passTurn = false;
        
        while (Winner == "") {
            // Console.Clear();
            Console.WriteLine($"Trump: {Trump}");
            if (Turn == 0) {
                Console.WriteLine("It's your turn to attack\n");
            }
            else {
                Console.WriteLine($"It's {playerList[Turn].Name}'s turn to attack\n");
            }
                

            foreach (Player player in playerList) {
                if (player.Hand.Count() == 0) {
                    Console.WriteLine($"{player.Name} has no cards left");
                    Winner = player.Name;
                } else {
                    player.StartedTurn = false;
                }
            }

            while (!passTurn) {
                await PlayTurns(tasks);
                taskResults = tasks.Select(task => ((Task<int>)task).Result).ToArray();
                if (taskResults.All(n => n == -1)) {
                    passTurn = true;
                }
            }
            await PlayTurns(tasks);
        }  
    }


    private async Task PlayTurns(Task<int>[] tasks) {
        tasks[0] = GetYourMove();
        for (int i = 1; i < playerList.Length; i++) {
            tasks[i] = GetAiInput(playerList[i]);
        }

        await Task.WhenAll(tasks);
    }

    // Gets the player's move
    private async Task<int> GetYourMove() {
        int cardIndex = -2;
        int temp = Turn; 

        if (you.StartedTurn) {
            Turn = -1; // Temporary, so the switch goes to default
        }

        switch(Turn) {
            case 0: // Player is initial attacker
                you.StartedTurn = true;
                cardIndex = await ChooseCard();
                break;

            case 1: // Player is defender
                int attackIndex = await ChooseAttack();
                if (attackIndex == -1) {
                    Console.WriteLine("You pass your turn");
                    cardIndex = -1;
                    break;
                } 

                bool successfulDefense = false;
                while (!successfulDefense) {
                    cardIndex = await ChooseCard(true);
                    successfulDefense = Beats(you[cardIndex], Table[attackIndex][0]);

                    if (cardIndex == -1) {
                        Console.WriteLine("You return");
                        cardIndex = await GetYourMove();
                    } else if (!successfulDefense) {
                        Console.WriteLine("You must play a card that beats the attack");
                    }
                }
                break;

                default: // Player is an attacker
                    cardIndex = await ChooseCard();
                    bool successfulAttack = false;

                    while(!successfulAttack) {

                        if (cardIndex == -1) {
                            Console.WriteLine("You pass");
                            cardIndex = -1;
                            break;
                        }

                        foreach (Card?[] attack in Table) {
                            if (you[cardIndex].rank == attack[0]?.rank) { 
                                successfulAttack = true;
                            }

                            if (!attack[1].Equals(null) && you[cardIndex].rank == attack[1]?.rank) {
                                successfulAttack = true;
                            }
                        }

                        if (!successfulAttack) {
                            Console.WriteLine("That card can't be played!");
                            cardIndex = await ChooseCard();
                        }
                    }
                    break;
        }

        Turn = temp;
        return cardIndex;
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

        if (rawInput == "z" || rawInput == "Z")
        {
            return -1;
        }
        else
        {
            bool validInput = Int32.TryParse(rawInput, out int cardIndex);
            if (!validInput || cardIndex > you.Hand.Count() || cardIndex <= 0)
            {
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
                string? input = await AsyncReadLine();
                attackIndex = Convert.ToInt32(input);
            }
            return attackIndex;
        }
    }
    

    // Handles the AI's moves
    // Returns true if the AI successfully attacks or defends
    private async Task<int> GetAiInput(Player player) {
        await Task.Delay(1000);

        // Player is the initial attacker
        if (Turn == player.TurnId && !player.StartedTurn) {
            Console.WriteLine($"{player.Name} is attacking");
            player.StartedTurn = true;
            int cardIndex = rng.Next(0, player.Hand.Count());
            return cardIndex;
        }

        // Player defends
        else if ((Turn+1) % playerList.Length == player.TurnId) {
            Console.WriteLine($"{player.Name} is defending");
            foreach (Card?[] attack in Table) {
                if (attack[1].Equals(null)) {
                    for (int i = 0; i < player.Hand.Count(); i++) {
                        if (Beats(player[i], attack[0])) {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        // Subsequent attacks 
        else {
            foreach (Card?[] attack in Table) {
                if (FullAttack(attack)) {
                    for (int i = 0; i < player.Hand.Count(); i++) {
                        if (attack[0]?.rank == player[i].rank) { return i; }
                        if (attack[1]?.rank == player[i].rank) { return i; }
                    }
                }
                else {
                    for (int i = 0; i < player.Hand.Count(); i++) {
                        if (attack[0]?.rank == player[i].rank) { return i; }
                    }
                }
            }
            return -1;
        }
    }

    public static bool FullAttack(Card?[] attack) {
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