using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace spider
{
    public class Card
    {
        public enum Suits {C = 0, D, H, S}
        public int Value { get; set; }
        public Suits Suit { get; set; }
        public string Color;
        public bool Hidden;        
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
        public static List<Card> deck;
        public static List<Card> imageDeck;
        
        public Deck()
        {
            deck = new List<Card>();
            imageDeck = new List<Card>();
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
        public static List<List<Card>> piles;
        public bool emptyPileExists;
        public Piles()
        {
            piles = new List<List<Card>>();
            for (int i = 0; i < 10; i++)
            {
                int fiveOrSix = 5;
                if (i < 4) fiveOrSix = 6;
                List<Card> subPiles = new List<Card>();
                for (int j = 0; j < fiveOrSix; j++) subPiles.Add(Deck.drawFromDeck());
                piles.Add(subPiles);
                int pileSize = piles[i].Count - 1;
                piles[i][pileSize].Hidden = false;
            }
        }
        public void addToPiles()
        {
            foreach (List<Card> subPiles in piles) 
            {
                if (subPiles.Count < 0) emptyPileExists = true;
            }
            {
                for (int i = 0; i < 10; i++)
                for (int j = 0; j < 1; j++) 
                {
                    Card c = Deck.drawFromDeck();
                    c.Hidden = false;
                    piles[i].Add(c);
                }
            }
        }
        public void _moveCard(int atPile, int fromCard, int toNextPile)
        {
            for (int i = fromCard; i < piles[atPile].Count; i++) 
            {
                Card c = piles[atPile][i];
                piles[toNextPile].Add(piles[atPile][i]); 
            }
            if (fromCard == 0) piles[atPile].Clear();
            else 
            {
                piles[atPile].RemoveRange(fromCard, piles[atPile].Count - fromCard);
                piles[atPile][piles[atPile].Count - 1].Hidden = false;
            }
        }

        public void moveCard(int atPile, int fromCard, int toNextPile, int suitCount)
        {
            int pileLength = piles[atPile].Count;
            Card cardToMove = piles[atPile][fromCard];
            Card cardToCheck = piles[toNextPile][piles[toNextPile].Count - 1];

            if (suitCount == 1)
            {
                if (cardToMove.Value + 1 == cardToCheck.Value)
                     _moveCard(atPile, fromCard, toNextPile);
            }

            if (suitCount == 2)
            {
                if (cardToMove.Color == cardToCheck.Color 
                    && cardToMove.Value + 1 == cardToCheck.Value)
                        _moveCard(atPile, fromCard, toNextPile);
            }

            if (suitCount == 4)
            {
                if (cardToMove.Suit == cardToCheck.Suit 
                    && cardToMove.Value + 1 == cardToCheck.Value)
                        _moveCard(atPile, fromCard, toNextPile);
            }
        }
    }  

    public partial class Form1 : Form
    {
        int pileNum = 0;
        int pileFrom = 0;
        int pileTo = 0;
        Deck deck = new Deck();
        Piles piles = new Piles();
        Dictionary<string, Image> images = new Dictionary<string, Image>();
        public Form1()
        {   
            Text = "Spider Solitaire";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.DarkGreen;
            FormBorderStyle =FormBorderStyle.None;
            WindowState=FormWindowState.Maximized;
            DoubleBuffered = true;
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
                for (int j = 0; j < Piles.piles[i].Count; j++)
                {
                    Card c = Piles.piles[i][j];
                    if (c.Hidden) img = images["BACK"];
                    else img = images[c.Name]; 
                    g.DrawImage(img, x, y);
                    y += 50;
                }
                x += 150;
            }
        }        
        protected override void OnMouseDown(MouseEventArgs args) 
        {
            Image img = images["BACK"];
            if (Deck.deck.Count > 0)
            {
                int w = this.Width - (img.Width / 2);
                int h = this.Height - (img.Height / 2);
                Rectangle deckSize = new Rectangle(w,h,img.Width, img.Width);
                if (deckSize.Contains(args.Location)) 
                {
                    if (piles.emptyPileExists) 
                        MessageBox.Show("Can't draw from the deck while there is an empty pile");
                    else 
                    {
                        piles.addToPiles();
                        this.Refresh();
                    }
                }
            }

            #region pile locations
            if (Enumerable.Range(10, 136).Contains(args.X)) pileNum = 0;
            if (Enumerable.Range(160, 286).Contains(args.X)) pileNum = 1;
            if (Enumerable.Range(310, 436).Contains(args.X)) pileNum = 2;
            if (Enumerable.Range(460, 586).Contains(args.X)) pileNum = 3;
            if (Enumerable.Range(610, 736).Contains(args.X)) pileNum = 4;
            if (Enumerable.Range(760, 876).Contains(args.X)) pileNum = 5;
            if (Enumerable.Range(910, 1036).Contains(args.X)) pileNum = 6;
            if (Enumerable.Range(1060, 1186).Contains(args.X)) pileNum = 7;
            if (Enumerable.Range(1210, 1336).Contains(args.X)) pileNum = 8;
            if (Enumerable.Range(1360, 1486).Contains(args.X)) pileNum = 9;
            #endregion

            if (args.Button == MouseButtons.Left) pileFrom = pileNum;
            if (args.Button == MouseButtons.Right) pileTo = pileNum;

            MessageBox.Show($"{args.Location}");
        }   
    }
}
