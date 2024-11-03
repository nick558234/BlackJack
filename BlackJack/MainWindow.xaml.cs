using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BlackJack
{
    public partial class MainWindow : Window
    {
        private List<(string Suit, string Rank, int Value)> deck;
        private List<(string Suit, string Rank, int Value)> playerCards;
        private List<(string Suit, string Rank, int Value)> dealerCards;
        private int playerScore;
        private int dealerScore;
        private bool isGameOver;
        private int playerBalance;
        private int currentBet;
        private static readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            deck = new List<(string Suit, string Rank, int Value)>();
            playerCards = new List<(string Suit, string Rank, int Value)>();
            dealerCards = new List<(string Suit, string Rank, int Value)>();
            playerBalance = 1000; // Initial balance
            UpdateUI();
        }

        private void NewGame()
        {
            deck = CreateDeck();
            playerCards = new List<(string Suit, string Rank, int Value)>();
            dealerCards = new List<(string Suit, string Rank, int Value)>();
            playerScore = 0;
            dealerScore = 0;
            isGameOver = false;

            DealInitialCards();
            UpdateUI();
        }

        private List<(string Suit, string Rank, int Value)> CreateDeck()
        {
            var suits = new[] { "Harten", "Ruiten", "Klaver", "Schoppen" };
            var ranks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Boer", "Vrouw", "Koning", "Aas" };
            var values = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 11 };

            var deck = new List<(string Suit, string Rank, int Value)>();

            foreach (var suit in suits)
            {
                for (int i = 0; i < ranks.Length; i++)
                {
                    deck.Add((suit, ranks[i], values[i]));
                }
            }

            return deck.OrderBy(_ => random.Next()).ToList();
        }

        private (string Suit, string Rank, int Value) DrawCard()
        {
            if (deck.Count == 0)
            {
                throw new InvalidOperationException("Het deck is leeg.");
            }

            var card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        private void DealInitialCards()
        {
            dealerCards.Add(DrawCard());
            playerCards.Add(DrawCard());
            playerCards.Add(DrawCard());

            CalculateScores();
        }

        private void CalculateScores()
        {
            playerScore = CalculateScore(playerCards);
            dealerScore = CalculateScore(dealerCards);
        }

        private int CalculateScore(List<(string Suit, string Rank, int Value)> cards)
        {
            int score = 0;
            int aceCount = 0;

            foreach (var card in cards)
            {
                score += card.Value;
                if (card.Rank == "Aas")
                {
                    aceCount++;
                }
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        private void UpdateUI()
        {
            PlayerCards.ItemsSource = null;
            PlayerCards.ItemsSource = playerCards;
            DealerCards.ItemsSource = null;
            DealerCards.ItemsSource = isGameOver ? dealerCards : new List<(string Suit, string Rank, int Value)> { dealerCards.FirstOrDefault() };
            PlayerScore.Text = $"Score: {playerScore}";
            DealerScore.Text = isGameOver ? $"Score: {dealerScore}" : "Score: ?";
            PlayerBalance.Text = $"Balance: ${playerBalance}";
        }

        private void DealButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(BetAmount.Text, out int bet) && bet > 0 && bet <= playerBalance)
            {
                currentBet = bet;
                playerBalance -= currentBet;
                NewGame();
            }
            else
            {
                MessageBox.Show("Ongeldige inzet. Voer een geldig bedrag in.");
            }
        }

        private void HitButton_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver) return;

            playerCards.Add(DrawCard());
            CalculateScores();
            UpdateUI();

            if (playerScore > 21)
            {
                MessageBox.Show("Speler gaat kapot! Dealer wint.");
                isGameOver = true;
                UpdateUI();
            }
        }

        private void StandButton_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver) return;

            while (dealerScore < 17)
            {
                dealerCards.Add(DrawCard());
                CalculateScores();
                UpdateUI();
            }

            isGameOver = true;
            UpdateUI();

            if (dealerScore > 21 || playerScore > dealerScore)
            {
                MessageBox.Show("Speler wint!");
                playerBalance += currentBet * 2; // Player wins, double the bet amount
            }
            else if (playerScore < dealerScore)
            {
                MessageBox.Show("Dealer wint!");
            }
            else
            {
                MessageBox.Show("Het is een gelijkspel!");
                playerBalance += currentBet; // Return the bet amount
            }

            UpdateUI(); // Ensure the balance is updated in the UI
        }
    }
}
