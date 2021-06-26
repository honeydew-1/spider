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

        static int lis(int[] arr, int n)
        {
            int[] lis = new int[n];
            int i, j, max = 0;
    
            for (i = 0; i < n; i++)
                lis[i] = 1;
    
            
            for (i = 1; i < n; i++)
                for (j = 0; j < i; j++)
                    if (arr[i] > arr[j] && lis[i] < lis[j] + 1)
                        lis[i] = lis[j] + 1;
    
            
            for (i = 0; i < n; i++)
                if (max < lis[i])
                    max = lis[i];
    
            return max;
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
            if (piles[toNextPile].Count > 0) 
            {
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
            else _moveCard(atPile, fromCard, toNextPile);
            
            if (piles[toNextPile].Count >= 13) checkStreak(toNextPile);
        }

        public void checkStreak(int pile)
        {
            
        }
    }  

    public partial class Form1 : Form
    {
        int pileNum = 0;
        int pileFrom = 0;
        int pileTo = 0;
        int fromCard = 0;
        int suitCount = 0;
        bool emptyPileExists = false;
        Deck deck = new Deck();
        Piles piles = new Piles();
        Dictionary<string, Image> images = new Dictionary<string, Image>();
        public Form1(int suitCount)
        {   
            this.suitCount = suitCount;
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
        bool inRange(int lo, int val, int hi) => lo < val && hi > val;    
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
                    for (int i = 0; i < 10; i++) 
                        if (Piles.piles[i].Count == 0)
                        {
                            MessageBox.Show("Can't draw from the deck while there is an empty pile");
                            emptyPileExists = true;
                        }
                    if (!emptyPileExists) 
                    {
                        piles.addToPiles();
                        this.Refresh();
                        emptyPileExists = false;
                    }    
                    
                }
            }

            #region pile locations
            if (inRange(10, args.X, 136)) pileNum = 0;
            else if (inRange(160, args.X, 286)) pileNum = 1;
            else if (inRange(310, args.X, 436)) pileNum = 2;
            else if (inRange(460, args.X, 586)) pileNum = 3;
            else if (inRange(610, args.X, 736)) pileNum = 4;
            else if (inRange(760, args.X, 876)) pileNum = 5;
            else if (inRange(910, args.X, 1036)) pileNum = 6;
            else if (inRange(1060, args.X, 1186)) pileNum = 7;
            else if (inRange(1210, args.X, 1336)) pileNum = 8;
            else if (inRange(1360, args.X, 1486)) pileNum = 9;
            #endregion

            if (args.Button == MouseButtons.Left) 
            {
                pileFrom = pileNum;
                int pileImgSize = ((Piles.piles[pileFrom].Count - 1) * 50) + 10;
                fromCard = (args.Y / 55);    
                if (fromCard >= Piles.piles[pileFrom].Count) fromCard = Piles.piles[pileFrom].Count - 1;
            }
            if (args.Button == MouseButtons.Right) 
            {
                pileTo = pileNum;
                piles.moveCard(pileFrom, fromCard, pileTo, suitCount);
                this.Refresh();
                List<Card> curPile = Piles.piles[pileTo];
                if (curPile.Count >= 13 && curPile[curPile.Count - 13].Hidden == false)
                {
                    for (int i = curPile.Count - 13; i < curPile.Count; i++)
                    if (curPile[i].Value > curPile[i + 1].Value)
                    {
                        curPile.RemoveRange(curPile.Count - 13, 13);
                    }
                }
            }
        }
    }
}