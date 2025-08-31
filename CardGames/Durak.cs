using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;

class Durak 
{
    //private bool gameRunning = true;
    private string Winner;
    private readonly static Random rng = new();
    private readonly Deck deck;
    private readonly Player you;
    private readonly Player[] playerList;
    private List<Card?[]> Table;
    public static Suit Trump;
    private int Turn;


    public Durak(int numPlayers)
    {
        Winner = "";
        deck = new Deck("durak");
        deck.Shuffle();

        you = new Player("You");
        playerList = new Player[numPlayers + 1];
        playerList[0] = you;
        for (int i = 1; i < playerList.Length; i++)
        {
            playerList[i] = new Player();
        }

        Table = new List<Card?[]>();
        Trump = deck[35].suit;
        Turn = FindFirstTurn();

        Deal();
        // For debugging - DELETE
        Console.WriteLine(playerList[0]);
        Console.WriteLine(playerList[1]);
        Console.WriteLine("-------------------");
    }

    public async Task Play() {
        Task<int>[] tasks = new Task<int>[playerList.Length];
        int[] taskResults = new int[playerList.Length];
        bool passTurn = false;

        Console.WriteLine("You are about to play Durak");
        Console.WriteLine("Press enter if you acknowledge your mistake");
        Console.ReadLine();

        while (Winner == "")
        {
            Console.Clear();
            Console.WriteLine($"Trump: {Trump}");

            if (Turn == 0)
            {
                Console.WriteLine("It's your turn to attack\n");
            }
            else
            {
                Console.WriteLine($"It's {playerList[Turn].Name}'s turn to attack\n");
            }

            while (!passTurn)
            {
                await PlayTurns(tasks);
                taskResults = tasks.Select(task => ((Task<int>)task).Result).ToArray();
                if (taskResults.All(n => n == -1))
                {
                    passTurn = true;
                }
            }
            
            // Checks if a player won
            foreach (Player player in playerList)
            {
                if (player.Hand.Count() == 0)
                {
                    Console.WriteLine($"{player.Name} has no cards left");
                    Winner = player.Name;
                }
            }
        }  
    }


    private async Task PlayTurns(Task<int>[] tasks) {
        tasks[0] = GetYourMove();
        for (int i = 1; i < playerList.Length; i++)
        {
            tasks[i] = GetAiInput(playerList[i]);
        }

        await Task.WhenAll(tasks);
    }

    private async Task<int> GetYourMove()
    {
        if (Turn == 0) // Player's turn to attack
        {
            int cardIndex;
            // First attack of the round
            if (Table.Count == 0)
            {
                cardIndex = await ChooseCard();
                Attack(you, cardIndex);
                return cardIndex;
            }
            // Subsequent attacks
            else
            {
                bool successfulAttack = false;

                while (!successfulAttack)
                {
                    cardIndex = await ChooseCard();

                    if (cardIndex == -1)
                    {
                        Console.WriteLine("You pass your turn");
                        successfulAttack = true;
                        return -1;
                    }
                    try
                    {
                        Card card = you[cardIndex];
                        foreach (Card?[] attack in Table)
                        {
                            if (card.rank == attack[0]?.rank || card.rank == attack[1]?.rank)
                            {
                                successfulAttack = true;
                                Attack(you, cardIndex);
                                return cardIndex;
                            }
                        }

                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Invalid card index");
                        continue;
                    }
                }
            }
        }

        else if (Turn == playerList.Length - 1) // Player defends
        {
            await Task.Delay(1000);
            bool successfulDefense = false;
            int attackIndex = await ChooseAttack();
            int cardIndex = -1;

            while (!successfulDefense)
            {
                cardIndex = await ChooseCard(true);
                if (cardIndex == -1)
                {
                    cardIndex = await GetYourMove();
                    return cardIndex;
                }
                successfulDefense = Beats(you[cardIndex], Table[attackIndex][0]);
                if (!successfulDefense) { Console.WriteLine("That card doesn't beat the attack"); }
            }
            return cardIndex; 
        }

        else // Someone else is getting attacked
        {
            Console.WriteLine("I'll get to it when I get to it");
            return -1;
        }

        Console.WriteLine("Woah how did you get here?!");
        return -1;
    }

    // // Gets the player's move
    // private async Task<int> GetYourMove()
    // {
    //     int cardIndex = -2;
    //     int temp = Turn;

    //     switch (Turn)
    //     {
    //         case 0: // Player is initial attacker
    //             cardIndex = await ChooseCard();
    //             break;

    //         case 1: // Player is defender
    //             int attackIndex = await ChooseAttack();
    //             if (attackIndex == -1)
    //             {
    //                 Console.WriteLine("You pass your turn");
    //                 cardIndex = -1;
    //                 break;
    //             }

    //             bool successfulDefense = false;
    //             while (!successfulDefense)
    //             {
    //                 cardIndex = await ChooseCard(true);
    //                 successfulDefense = Beats(you[cardIndex], Table[attackIndex][0]);

    //                 if (cardIndex == -1)
    //                 {
    //                     Console.WriteLine("You return");
    //                     cardIndex = await GetYourMove();
    //                 }
    //                 else if (!successfulDefense)
    //                 {
    //                     Console.WriteLine("You must play a card that beats the attack");
    //                 }
    //             }
    //             break;

    //         default: // Player is an attacker
    //             cardIndex = await ChooseCard();
    //             bool successfulAttack = false;

    //             while (!successfulAttack)
    //             {

    //                 if (cardIndex == -1)
    //                 {
    //                     Console.WriteLine("You pass");
    //                     cardIndex = -1;
    //                     break;
    //                 }

    //                 foreach (Card?[] attack in Table)
    //                 {
    //                     if (you[cardIndex].rank == attack[0]?.rank)
    //                     {
    //                         successfulAttack = true;
    //                     }

    //                     if (!attack[1].Equals(null) && you[cardIndex].rank == attack[1]?.rank)
    //                     {
    //                         successfulAttack = true;
    //                     }
    //                 }

    //                 if (!successfulAttack)
    //                 {
    //                     Console.WriteLine("That card can't be played!");
    //                     cardIndex = await ChooseCard();
    //                 }
    //             }
    //             break;
    //     }

    //     Turn = temp;
    //     return cardIndex;
    // }

    // Lets me use Console.ReadLine() asynchronously (very niffty)
    private static async Task<string?> AsyncReadLine() {
        return await Task.Run(Console.ReadLine);
    } 

    private async Task<int> ChooseCard(bool returnable = false) {
        if (returnable) Console.WriteLine("Which card would you like to play? (Press z to return)");
        else Console.WriteLine("Which card would you like to play?");
        Console.WriteLine(you);

        string? rawInput = await AsyncReadLine();
        string trimmedInput = rawInput?.Trim() ?? "";       

        if (trimmedInput.ToLower() == "z" && returnable)
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
        if (rawInput == "") { // Accept attack & pass turn
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
    private async Task<int> GetAiInput(Player player) {
        await Task.Delay(1000);

        // Player is the initial attacker
        if (Turn == player.TurnId) {
            Console.WriteLine($"{player.Name} is attacking");
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

    /// <summary>
    /// Checks if an attack has already been defended against
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    public static bool FullAttack(Card?[] attack)
    {
        if (attack[1].Equals(null))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void Attack(Player attacker, int cardIndex) {
        Card?[] attack = new Card?[2];
        attack[0] = attacker[cardIndex];

        string attack_msg = Turn == 0 ? "You attack with" : $"{attacker.Name} attacks with";
        Console.WriteLine($"{attack_msg} {attack[0]}");

        Table.Add(attack);
        attacker.Remove(cardIndex);
    }

    private void Defend(int attackIndex, int cardIndex) {
        Player defender = playerList[(Turn+1) % playerList.Length];
        Card?[] attack = Table[attackIndex];
        Card? responseCard = defender[cardIndex];

        string defend_msg = Turn == 0 ? "You defend with" : $"{defender.Name} defends with";
        Console.WriteLine($"{defend_msg} {responseCard}");

        attack[1] = responseCard;
        defender.Remove(cardIndex);
    }


    // The player to go first has the lowest trump card
    /// <summary>
    /// Finds the player who has the lowest trump card. They are the first player to attack.
    /// </summary>
    /// <returns>The turn index of the first player to attack. The human player is always 0.</returns>
    public int FindFirstTurn()
    {
        Player firstPlayer;
        for (int i = 5; i < 13; i++)
        {
            foreach (Player player in playerList)
            {
                if (player.Hand.Contains(new Card(Trump, i)))
                {
                    firstPlayer = player;
                    return Array.IndexOf(playerList, firstPlayer);
                }
            }
        }
        return 0;
    }

    // Shows all attacks
    /// <summary>
    /// Displays all attacks on the table.
    /// </summary>
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
    /// <summary>
    /// Deals a given number of cards from the deck to the target player.
    /// If provided no argument or if -1 is the only argument, 6 cards are dealt to every player.
    /// </summary>
    /// <param name="target">The index of the player to deal cards to.</param>
    /// <param name="numCards">The number of cards to deal.</param>
    private void Deal(int target = -1, int numCards = 1)
    {
        if (target == -1)
        {
            Console.WriteLine("Dealing cards");
            for (int i = 0; i < 6; i++)
            {
                foreach (Player player in playerList)
                {
                    player.Hand.Add(deck.draw());
                }
            }
        }

        else if (target >= 0 && target < playerList.Length)
        {
            for (int i = 0; i < numCards; i++)
            {
                playerList[target].Hand.Add(deck.draw());
            }
        }

        else
        {
            Console.WriteLine("Invalid target");
        }
    }

    private bool IsTrump(Card? card) {
        return card?.suit == Trump;
    }

    /// <summary>
    /// Takes two cards and determines if the first card beats the second.
    /// This takes into consideration both the rank of the cards and the trump suit.
    /// </summary>
    /// <param name="Card1"></param>
    /// <param name="Card2"></param>
    /// <returns></returns>
    private bool Beats(Card? Card1, Card? Card2)
    {
        if (IsTrump(Card1) && !IsTrump(Card2))
        {
            return true;
        }
        else if (!IsTrump(Card1) && IsTrump(Card2))
        {
            return false;
        }
        else {
            return Card1?.rank > Card2?.rank;
        }
    }

}