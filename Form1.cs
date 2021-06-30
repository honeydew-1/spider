using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace spider
{
    public enum Suit {C, D, H, S}

    public class Card
    {
        
        public enum Color{RED, BLACK}
        public int Value { get; set; }
        public Suit Rank { get; set; }
        public Color RB { get; set; }
        public bool Hidden;  

        public Card(int value, Suit rank, bool hidden = true)
        {
            this.Value = value;
            this.Rank = rank;
            if (this.Rank == Suit.H || this.Rank == Suit.D) this.RB = Color.RED;
            if (this.Rank == Suit.C || this.Rank == Suit.S) this.RB = Color.BLACK;
            this.Hidden = hidden;
        }

        private string NamedValue
        {
            get
            {
                switch (Value)
                {
                    case 1: return "A";
                    case 13: return "K";
                    case 12: return "Q";
                    case 11: return "J";
                    default: return Value.ToString();
                }
            }
        }

        public string Name => NamedValue + Rank.ToString();
    }

    public class Deck
    {
        public List<Card> deck { get; set; }

        public int Count => deck.Count;

        public Card this[int i]
        {
            get => deck[i];
            set => deck[i] = value;
        }
        
        public Deck()
        {
            deck = new List<Card>();
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 52; i++)
                {
                    Suit rank = (Suit)(Math.Floor((decimal)i/13));
                    int val = (i % 13) + 1;
                    deck.Add(new Card(val, rank));
                }
                Random r = new Random();
                for (int n = deck.Count - 1; n > 0; --n)
                {
                    int k = r.Next(n+1);
                    Card temp = deck[n];
                    deck[n] = deck[k];
                    deck[k] = temp;
                }
            }
        }
        
        public Card drawFromDeck()
        {
            Card c = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            return c;
        }
    }

    public class Piles
    {
        public static List<List<Card>> piles = new List<List<Card>>();

        Deck deck = new Deck();

        public List<Card> this[int i] => piles[i];

        public Piles()
        {   
            for (int i = 0; i < 10; i++)
            {
                int fiveOrSix = 5;
                if (i < 4) fiveOrSix = 6;
                List<Card> subPiles = new List<Card>();
                for (int j = 0; j < fiveOrSix; j++) subPiles.Add(deck.drawFromDeck());
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
                        Card c = deck.drawFromDeck();
                        c.Hidden = false;
                        piles[i].Add(c);
                    }
        }

        private bool isValid(int fromPile, int fromCard)
        {
            for (int i = fromCard; i < piles[fromPile].Count - 1; i++)
            {
                if (piles[fromPile][i].Value != piles[fromPile][i + 1].Value + 1) return false;
            }
            return true;
        }
        
        public void _moveCard(int atPile, int fromCard, int toNextPile)
        {
            if (fromCard == piles[atPile].Count - 1 && fromCard != 0) 
            {
                Card c = piles[atPile][fromCard];
                piles[toNextPile].Add(c);
                piles[atPile].Remove(c);
                piles[atPile][piles[atPile].Count - 1].Hidden = false;
            }
            else 
            {
                if (isValid(atPile, fromCard))
                {
                    for (int i = fromCard; i < piles[atPile].Count; i++) 
                    {
                        Card c = piles[atPile][i];
                        piles[toNextPile].Add(piles[atPile][i]); 
                        piles[atPile][piles[atPile].Count - 1].Hidden = false;
                    }

                    if (fromCard == 0) piles[atPile].Clear();
                    else
                    {
                        piles[atPile].RemoveRange(fromCard, piles[atPile].Count - fromCard);
                        piles[atPile][piles[atPile].Count - 1].Hidden = false;
                    }
                }
            }
            List<Card> curPile = piles[toNextPile];
            if (piles[toNextPile].Count >= 13 && isValid(toNextPile, curPile.Count - 13 ))
            {
                
                if (curPile.Count >= 13 && curPile[curPile.Count - 13].Hidden == false && isValid(toNextPile, curPile.Count - 13))
                {
                    for (int i = curPile.Count - 13; i < curPile.Count; i++)
                    {
                        if (curPile[i].Value > curPile[i + 1].Value)
                        {
                            if (curPile.Count > 13)
                            {
                                curPile[curPile.Count - 14].Hidden = false;
                            }
                            curPile.RemoveRange(curPile.Count - 13, 13);
                        }
                    }
                }
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
                    if (cardToMove.RB == cardToCheck.RB 
                        && cardToMove.Value + 1 == cardToCheck.Value)
                            _moveCard(atPile, fromCard, toNextPile);
                }

                if (suitCount == 4)
                {
                    if (cardToMove.Rank == cardToCheck.Rank 
                        && cardToMove.Value + 1 == cardToCheck.Value)
                            _moveCard(atPile, fromCard, toNextPile);
                }
            }
            else _moveCard(atPile, fromCard, toNextPile);
        }
    }  

    public partial class Form1 : Form
    {
        int pileNum = 0;
        int pileFrom = 0;
        int pileTo = 0;
        int fromCard = 0;
        int suitCount = 0;
        
        public Deck deck = new Deck();
        public Piles piles = new Piles();
        Dictionary<string, Image> images = new Dictionary<string, Image>();

        public Form1(int suitCount)
        {   
            this.suitCount = suitCount;
            Text = "Spider Solitaire";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.DarkGreen;
            WindowState=FormWindowState.Maximized;
            DoubleBuffered = true;
            for (Suit suit = Suit.C ; suit <= Suit.S ; suit++)
                for (int val = 1 ; val <= 13 ; ++val) 
                {
                    Card c = new Card(val, suit);
                    images[c.Name] = Image.FromFile(Path.Combine("PNG", c.Name+".png"));
                }
            images.Add("BACK",Image.FromFile(Path.Combine("PNG", "BACK"+".png")));
            InitializeComponent();   
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (deck.Count > 0)
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

            bool emptyPileExists = false;
            
            for (int i = 0; i < 10; i++)
            {
                if (piles[i].Count == 0) emptyPileExists = true;
            }

            if (deck.Count > 0)
            {
                int w = this.Width - (img.Width / 2);
                int h = this.Height - (img.Height / 2);
                Rectangle deckSize = new Rectangle(w,h,img.Width, img.Width);

                if (deckSize.Contains(args.Location)) 
                {
                    
                    if (emptyPileExists) 
                    {
                        MessageBox.Show("Can't draw from the deck while there is an empty pile");
                    }
                    else
                    {
                        piles.addToPiles();
                        this.Refresh();
                        MessageBox.Show(deck.deck.Count.ToString());
                        emptyPileExists = false;
                    }  
                }
            }

            if (args.X < 1486) pileNum = (args.X / 150);
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

                int emptyPileCount = 0;

                for(int i = 0; i < 10; i++)
                {
                    if (Piles.piles[i].Count == 0) emptyPileCount++;
                } 

                if (emptyPileCount == 10 && deck.Count == 0) 
                {
                    MessageBox.Show("Game Over :)");
                    Application.Exit();
                }
            }
        }
    }
}