using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace spider
{
    public class Card
    {
        #region Card properties
        public enum Suits {C = 0, D, H, S}
        public int Value { get; set; }
        public Suits Suit { get; set; }
        public string Color;
        public bool Hidden;
        
        #endregion
        public Card(int value, Suits suit, bool hidden = true)
        {
            this.Value = value;
            this.Suit = suit;
            this.Hidden = hidden;
            if (this.Suit == Suits.H || this.Suit == Suits.D) this.Color = "R";
            if (this.Suit == Suits.C || this.Suit == Suits.S) this.Color = "B";
        }
        private string NamedValue
        {
            get
            {
                string name = string.Empty;
                switch (Value)
                {
                    case (1):
                        name = "A";
                        break;
                    case (13):
                        name = "K";
                        break;
                    case (12):
                        name = "Q";
                        break;
                    case (11):
                        name = "J";
                        break;
                    default:
                        name = Value.ToString();
                        break;
                }
                return name;
            }
        }
        public string Name
        {
            get => NamedValue + Suit.ToString();
        }
    }
    public class Deck
    {
        public static List<Card> deck = new List<Card>();
        public static List<Card> imageDeck = new List<Card>();
        
        public Deck()
        {
            for (int i = 0; i < 2; i++) fillDeck();
        }

        private void fillDeck()
        {
            Random r = new Random();
            for (int i = 0; i < 52; i++)
            {
                Card.Suits suit = (Card.Suits)(Math.Floor((decimal)i/13));
                int val = (i % 13) + 1;
                deck.Add(new Card(val, suit));
                imageDeck.Add(new Card(val, suit));
            }
            for (int n = deck.Count - 1; n > 0; --n)
            {
                int k = r.Next(n+1);
                Card temp = deck[n];
                deck[n] = deck[k];
                deck[k] = temp;
            }
               
        }
        public static Card drawFromDeck()
        {
            Card c = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            return c;
        }
    }
    public class Piles
    {
        public static List<List<Card>> piles = new List<List<Card>>();
        public Piles()
        {
            for (int i = 0; i < 10; i++)
            {
                List<Card> subPiles = new List<Card>();
                for (int j = 0; j < 5; j++) subPiles.Add(Deck.drawFromDeck());
                piles.Add(subPiles);
                int pileSize = piles[i].Count - 1;
                piles[i][pileSize].Hidden = false;
            }
        }
        public void addToPiles()
        {
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 1; j++) 
                {
                    Card c = Deck.drawFromDeck();
                    c.Hidden = false;
                    piles[i].Add(c);
                }
        }
        public void _moveCard(int atPile, int fromCard, int toNextPile)
        {
            for (int i = fromCard; i < piles[atPile].Count; i++) piles[toNextPile].Add(piles[atPile][i]); 
            piles[atPile].RemoveRange(fromCard, piles[atPile].Count - fromCard);
            piles[atPile][piles[atPile].Count - 1].Hidden = false;
        }
        public void moveCard(int atPile, int fromCard, int toNextPile, int suitCount)
        {
            int pileLength = piles[atPile].Count;
            Card firstCard = piles[atPile][fromCard];
            Card cardToCheck = piles[toNextPile][piles[toNextPile].Count - 1];

            if (suitCount == 1)
            {
                for (int i = fromCard; i < pileLength; i++)
                {
                    Card curCard = piles[atPile][i]; 
                    if (curCard.Value + 1 == cardToCheck.Value) _moveCard(atPile, i, toNextPile);
                }   
            }

            if (suitCount == 2)
            {
                for (int i = fromCard; i < pileLength; i++)
                {
                    Card curCard = piles[atPile][i];
                    if (curCard.Value + 1 == cardToCheck.Value
                        && curCard.Color == cardToCheck.Color) 
                            _moveCard(atPile, i, toNextPile);
                }   
            }

            if (suitCount == 4)
            {
                for (int i = fromCard; i < pileLength; i++)
                {
                    Card curCard = piles[atPile][i];
                    if (curCard.Value + 1 == cardToCheck.Value
                        && curCard.Suit == cardToCheck.Suit) 
                            _moveCard(atPile, i, toNextPile);
                }      
            }
        }
    }  
    public partial class Form1 : Form
    {
        Deck d = new Deck();
        Piles p = new Piles();
        Dictionary<string, Image> images = new Dictionary<string, Image>();
        public Form1()
        {   
            Text = "Spider Solitaire";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.DarkGreen;
            FormBorderStyle =FormBorderStyle.None;
            WindowState=FormWindowState.Maximized;
            foreach (Card c in Deck.imageDeck) 
                images[c.Name] = Image.FromFile(Path.Combine("PNG", c.Name+".png"));
            images.Add("BACK",Image.FromFile(Path.Combine("PNG", "Back"+".png")));
            InitializeComponent();   
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (Deck.deck.Count > 0)
            {
                Image deckImg = images["BACK"];
                int w = this.Width - (deckImg.Width / 2);
                int h = this.Height - (deckImg.Height / 2);
                g.DrawImage(deckImg, w, h);

            } 
            int x = 10;
            for (int i = 0; i < 10; i++)
            {
                int y = 10;
                Image img = null;
                for (int j = 0; j < 5; j++)
                {
                    Card c = Piles.piles[i][j];
                    if (c.Hidden) img = images["BACK"];
                    else img = images[c.Name]; 
                    g.DrawImage(img, x, y);
                    y += 50;
                }
                x += 197;
            }
        }
    }
}
