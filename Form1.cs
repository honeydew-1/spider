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
        public enum Color {RED, BLACK}
        public int value { get; set; }
        public Suit rank { get; set; }
        public Color color { get; set; }
        public bool hidden;  

        public Card(int value, Suit rank, bool hidden = true)
        {
            this.value = value;
            this.rank = rank;
            if (this.rank == Suit.H || this.rank == Suit.D) this.color = Color.RED;
            if (this.rank == Suit.C || this.rank == Suit.S) this.color = Color.BLACK;
            this.hidden = hidden;
        }

        private string namedValue
        {
            get
            {
                switch (value)
                {
                    case 1: return "A";
                    case 13: return "K";
                    case 12: return "Q";
                    case 11: return "J";
                    default: return value.ToString();
                }
            }
        }

        public string name => namedValue + rank.ToString();
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
                    (deck[n], deck[k]) = (deck[k], deck[n]);
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
        public List<List<Card>> piles = new List<List<Card>>();

        public List<Card> this[int i] => piles[i];
        public Deck deck  = new Deck();
        
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
                piles[i][pileSize].hidden = false;
            }
        }

        public void addToPiles()
        {
                for (int i = 0; i < 10; i++)
                    for (int j = 0; j < 1; j++) 
                    {
                        Card c = deck.drawFromDeck();
                        c.hidden = false;
                        piles[i].Add(c);
                    }
        }

        private bool isValid(int fromPile, int fromCard)
        {
            for (int i = fromCard; i < piles[fromPile].Count - 1; i++)
            {
                if (piles[fromPile][i].value != piles[fromPile][i + 1].value + 1) return false;
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
                piles[atPile][piles[atPile].Count - 1].hidden = false;
            }
            else 
            {
                if (isValid(atPile, fromCard))
                {
                    for (int i = fromCard; i < piles[atPile].Count; i++) 
                    {
                        Card c = piles[atPile][i];
                        piles[toNextPile].Add(piles[atPile][i]); 
                        piles[atPile][piles[atPile].Count - 1].hidden = false;
                    }

                    if (fromCard == 0) piles[atPile].Clear();
                    else
                    {
                        piles[atPile].RemoveRange(fromCard, piles[atPile].Count - fromCard);
                        piles[atPile][piles[atPile].Count - 1].hidden = false;
                    }
                }
            }
            List<Card> curPile = piles[toNextPile];
            if (piles[toNextPile].Count >= 13 && isValid(toNextPile, curPile.Count - 13 ))
            {
                
                if (curPile.Count >= 13 && curPile[curPile.Count - 13].hidden == false && isValid(toNextPile, curPile.Count - 13))
                {
                    for (int i = curPile.Count - 13; i < curPile.Count; i++)
                    {
                        if (curPile[i].value > curPile[i + 1].value)
                        {
                            if (curPile.Count > 13)
                            {
                                curPile[curPile.Count - 14].hidden = false;
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
                    if (cardToMove.value + 1 == cardToCheck.value)
                        _moveCard(atPile, fromCard, toNextPile);
                }

                if (suitCount == 2)
                {
                    if (cardToMove.color == cardToCheck.color 
                        && cardToMove.value + 1 == cardToCheck.value)
                            _moveCard(atPile, fromCard, toNextPile);
                }

                if (suitCount == 4)
                {
                    if (cardToMove.rank == cardToCheck.rank 
                        && cardToMove.value + 1 == cardToCheck.value)
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
                    images[c.name] = Image.FromFile(Path.Combine("PNG", c.name+".png"));
                }
            images.Add("BACK",Image.FromFile(Path.Combine("PNG", "BACK"+".png")));
            InitializeComponent();   
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (piles.deck.Count > 0)
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
                for (int j = 0; j < piles[i].Count; j++)
                {
                    Card c = piles[i][j];
                    if (c.hidden) img = images["BACK"];
                    else img = images[c.name]; 
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

            if (piles.deck.Count > 0)
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
                        emptyPileExists = false;
                    }  
                }
            }

            if (args.X < 1486) pileNum = (args.X / 150);
            if (args.Button == MouseButtons.Left) 
            {
                pileFrom = pileNum;
                int pileImgSize = ((piles[pileFrom].Count - 1) * 50) + 10;
                fromCard = (args.Y / 55);    
                if (fromCard >= piles[pileFrom].Count) fromCard = piles[pileFrom].Count - 1;
            }

            if (args.Button == MouseButtons.Right) 
            {
                pileTo = pileNum;
                piles.moveCard(pileFrom, fromCard, pileTo, suitCount);
                this.Refresh();

                int emptyPileCount = 0;

                for(int i = 0; i < 10; i++)
                {
                    if (piles[i].Count == 0) emptyPileCount++;
                } 

                if (emptyPileCount == 10 && piles.deck.Count == 0) 
                {
                    MessageBox.Show("Game Over :)");
                    Application.Exit();
                }
            }
        }
    }
}