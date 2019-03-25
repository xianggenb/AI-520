using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//Maze Path Finding Program with User Friendly UI
//Programmed by Xiang Xing For Rutgers 198:520 Intro to AI as Assignment 1
//illegal to Copy until 10/10/2018 except Group Members
//Later will be released at https://github.com/xianggenb publically
namespace Mazerunner
{
    public partial class Form1 : Form
    {
        #region factors and container
        private static Dictionary<Tuple<int, int>, Boolean> Maze;
        private static Dictionary<Tuple<int, int>, Boolean> BackUpMaze ;
        //record the solution path, the first one is current node and the second one is the corresponding parent node
        private static Dictionary<Tuple<int, int>, Tuple<int, int>> Solution;
        private static double OccupationRate = 0;
        private static int Dim = 5;
        private static int blockSize = 40;
        private static int x_single_point;
        private static int y_single_point;
        private static List<Tuple<int, int>> bitGroup;
        private static int x_sel_start;
        private static int y_sel_start;
        private static int x_sel_end;
        private static int y_sel_end;
        private static int group_indicator=0;

        #endregion
        public Form1()
        {
            InitializeComponent();
          
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            disable_pathFinding();
           
            this.pictureBox1.MouseDown += new MouseEventHandler(Cell_state);
            this.pictureBox1.MouseMove += new MouseEventHandler(group_select);
            this.pictureBox1.MouseDown += new MouseEventHandler(select_start);
            this.pictureBox1.MouseUp += new MouseEventHandler(select_end);
        }
        
        #region Bitmap initialization
        private void button4_Click(object sender, EventArgs e)
        {
            //Generate new maze and discard the old one
            //create new dictionary as reference
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (int.TryParse(textBox1.Text, out Dim))
                {
                    if (Dim > 400) {
                        //larget dim
                        Maze = new Dictionary<Tuple<int, int>, bool>();
                        Random rdm = new Random();
                        for (int i = 0; i < Dim; i++) {
                            for (int j = 0; j < Dim; j++) {
                                if ((i == 0 && j == 0) || (i == Dim - 1 && j == Dim - 1))
                                {

                                    Maze.Add(new Tuple<int, int>(i, j), true);
                                }
                                else if (rdm.Next() % 10 < OccupationRate * 10)
                                {
                                    //occupied
                                    Maze.Add(new Tuple<int, int>(i, j), false);
                                }
                                else {
                                    Maze.Add(new Tuple<int, int>(i, j), true);
                                }
                            }
                        }
                        enable_pathFinding();
                        return;
                    }
                    else if(Dim==0) {
                        MessageBox.Show("Dim cannot be zero","error");
                        return;
                    }
                  
                }
            }
            Maze = new Dictionary<Tuple<int, int>, bool>();
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (int.TryParse(textBox1.Text, out Dim))
                {
                    blockSize = 800/ Dim;
                }
            }
            else {
                blockSize = 40;
            }
            #region create bitmap
            int bitMapSize = Dim * blockSize;
            Bitmap flag = new Bitmap(bitMapSize, bitMapSize);
            Graphics flagGraphics = Graphics.FromImage(flag);
            Random rd = new Random();
            for (int i = 0; i < bitMapSize; i+=blockSize) {
                //draw grid view of the maze
                for (int j = 0; j < bitMapSize; j+=blockSize) {
                    //draw cells of the grid, white is valid , balck is occupied
                    if ((i == 0 && j == 0) || (i == bitMapSize-blockSize && j == bitMapSize-blockSize)) {
                        //mark the entry and exit as valid
                        flagGraphics.FillRectangle(Brushes.White, i, j, blockSize, blockSize);
                        Maze.Add(new Tuple<int, int>(i / blockSize, j / blockSize), true);
                    }
                    else if (rd.Next() % 10 < OccupationRate * 10)
                    {
                        //occupied
                        flagGraphics.FillRectangle(Brushes.Black, i, j, blockSize, blockSize);
                        Maze.Add(new Tuple<int, int>(i / blockSize, j / blockSize), false);
                    }
                    else
                    {
                        //valid cells
                        flagGraphics.FillRectangle(Brushes.White, i, j, blockSize, blockSize);
                        Maze.Add(new Tuple<int, int>(i / blockSize, j / blockSize), true);
                    }
                }
            }
            for (int i = 0; i <= bitMapSize; i += blockSize) {
                //draw grid lines
                flagGraphics.DrawLine(Pens.Black, i, 0, i, bitMapSize);
                flagGraphics.DrawLine(Pens.Black, 0, i, bitMapSize, i);
            }
            pictureBox1.Image = flag;
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            enable_pathFinding();
            #endregion
        }
        #endregion
       
        #region DFS ALGS
        private void button1_Click(object sender, EventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int frige_size = 0;
            pictureBox1.ContextMenuStrip = null;
            disable_pathFinding();
            Tuple<int, int> end = new Tuple<int, int>(Dim - 1, Dim - 1);
            Stack<Tuple<int, int>> s = new Stack<Tuple<int, int>>();
            //when expanded successful, we can add the pair to the solution
            Solution = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
            int expandCount = 1;
            s.Push(new Tuple<int, int>(0,0));
            while (!(s.Count == 0)) {
                Tuple<int, int> temp = s.Pop();
                //eliminate re-Expand
                Maze[temp] = false;
                if (temp.Item1==end.Item1 && temp.Item2==end.Item2 ) {
                    //we find a path to the end
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    label7.Text = elapsedMs.ToString() + " ms";
                    markExpanded(ref end);
                    label4.Text = "Path Finding Succeed! ";
                    label3.Text = "Expanded Node Count:" + expandCount;
                    fringe.Text = frige_size.ToString();
                    drawPath();
                    redrawBitmap();

                    return;
                }
                markExpanded(ref temp);
                expandCount++;
                DFS_Expand(ref temp, ref s);
                frige_size = Math.Max(frige_size,s.Count);
            }
            redrawBitmap();
            label4.Text = "Path Finding Fail: dead end";
            watch.Stop();
            //DFS ALG

        }
        private void DFS_Expand(ref Tuple<int, int> node, ref Stack<Tuple<int, int>> s) {
            if (node.Item1 > 0) {
                // if x cord is larger than 1, then we can try push the left node to stack
                Tuple<int, int> ln = new Tuple<int, int>(node.Item1 - 1, node.Item2);
                if (Maze[ln])
                {
                    s.Push(ln);
                    Maze[ln] = false;
                    recordPath(ln,node);
                }
            }
            if (node.Item2 > 0) {
                //if y cord is larger than 1, then we can try push the up node to stack
                Tuple<int, int> un = new Tuple<int, int>(node.Item1, node.Item2-1);
                if (Maze[un])
                {
                    s.Push(un);
                    Maze[un] = false;
                    recordPath(un, node);
                }
            }
            if (node.Item1 < Dim - 1) {
                // if x cord have not reach the right most bound, we can try push the right most one to the stack
                Tuple<int, int> rn = new Tuple<int, int>(node.Item1 +1, node.Item2);
                if (Maze[rn])
                {
                    s.Push(rn);
                    Maze[rn] = false;
                    recordPath(rn, node);
                }
            }
            if (node.Item2 < Dim - 1) {
                //if y cord have not reach the bottom bound, we can try push the bottom most one to stack
                Tuple<int, int> bn = new Tuple<int, int>(node.Item1, node.Item2+1);
                if (Maze[bn])
                {
                    s.Push(bn);
                    Maze[bn] = false;
                    recordPath(bn, node);
                }
            }
            return;
        }

        #endregion
        
        #region BFS ALGS
        private void button2_Click(object sender, EventArgs e)
        {
            //BFS ALG
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int frige_size = 0;
            pictureBox1.ContextMenuStrip = null;
            disable_pathFinding();
            Tuple<int, int> end = new Tuple<int, int>(Dim - 1, Dim - 1);
            Queue<Tuple<int, int>> q = new Queue<Tuple<int, int>>();
            Solution = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
            int expandCount = 1;
            q.Enqueue(new Tuple<int, int>(0, 0));
            while (!(q.Count == 0))
            {
                Tuple<int, int> temp = q.Dequeue();
                //eliminate re-Expand
                Maze[temp] = false;
                if (temp.Item1 == end.Item1 && temp.Item2 == end.Item2)
                {
                    //we find a path to the end
                    watch.Stop();
                    markExpanded(ref end);
                    drawPath();
                    label4.Text = "Path Finding Succeed! ";
                    label3.Text = "Expanded Node Count:" + expandCount;
                    redrawBitmap();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    label7.Text = elapsedMs.ToString()+" ms";
                    fringe.Text = frige_size.ToString();
                    return;
                }
                markExpanded(ref temp);
                expandCount++;
                BFS_Expand(ref temp, ref q);
                frige_size = Math.Max(frige_size, q.Count);
            }
            redrawBitmap();
            label4.Text = "Path Finding Fail: dead end";
            watch.Stop();
        }
        private void BFS_Expand(ref Tuple<int, int> node, ref Queue<Tuple<int, int>> q) {
            
            if (node.Item2 < Dim - 1)
            {
                //if y cord have not reach the bottom bound, we can try push the bottom most one to stack
                Tuple<int, int> bn = new Tuple<int, int>(node.Item1, node.Item2 + 1);
                if (Maze[bn] && !isBnodeAlreadyExist(ref bn,ref q))
                {
                    q.Enqueue(bn);
                    recordPath(bn, node);
                }
            }
            if (node.Item1 < Dim - 1)
            {
                // if x cord have not reach the right most bound, we can try push the right most one to the stack
                Tuple<int, int> rn = new Tuple<int, int>(node.Item1 + 1, node.Item2);
                if (Maze[rn] && !isBnodeAlreadyExist(ref rn, ref q))
                {
                    q.Enqueue(rn);
                    recordPath(rn, node);
                }
            }
            if (node.Item2 > 0)
            {
                //if y cord is larger than 1, then we can try push the up node to stack
                Tuple<int, int> un = new Tuple<int, int>(node.Item1, node.Item2 - 1);
                if (Maze[un] && !isBnodeAlreadyExist(ref un, ref q))
                {
                    q.Enqueue(un);
                    recordPath(un, node);
                }
            }
            if (node.Item1 > 0)
            {
                // if x cord is larger than 1, then we can try push the left node to stack
                Tuple<int, int> ln = new Tuple<int, int>(node.Item1 - 1, node.Item2);
                if (Maze[ln] && !isBnodeAlreadyExist(ref ln, ref q))
                {
                    q.Enqueue(ln);
                    recordPath(ln, node);
                }
            }
        }
        bool isBnodeAlreadyExist(ref Tuple<int, int> node, ref Queue<Tuple<int, int>> q) {
            //check if such node is already exist in the queue
            if (q.Contains(node)) {
                return true;
            }
            return false;
        }

        #endregion

        #region A* ALGS
        #region EDH 
        private void button3_Click(object sender, EventArgs e)
        {
            //A* EDH ALG
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int frige_size = 0;
            pictureBox1.ContextMenuStrip = null;
            disable_pathFinding();
            int expandCount = 1;
            Tuple<int, int> end = new Tuple<int, int>(Dim - 1, Dim - 1);
            //count the G of each expanded node
            Dictionary<Tuple<int, int>, int> Gcount = new Dictionary<Tuple<int, int>, int>();
            PriorityQueue pq = new PriorityQueue();
            Solution = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
            Tuple<int, int> start = new Tuple<int, int>(0, 0);
            Maze[start] = false;
            Gcount.Add(start, 0);
            pq.Enqueue(start, Gcount[start] + cal_EDH(start, end));
            while (pq.count > 0)
            {
                Tuple<int, int> temp = pq.Dequeue();
                Maze[temp] = false;
                if (temp.Item1 == end.Item1 && temp.Item2 == end.Item2)
                {
                    //we find a path to the end
                    watch.Stop();
                    markExpanded(ref end);
                    drawPath();
                    label4.Text = "Path Finding Succeed! ";
                    label3.Text = "Expanded Node Count:" + expandCount;
                    redrawBitmap();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    label7.Text = elapsedMs.ToString() + " ms";
                    fringe.Text = frige_size.ToString();
                    return;
                }
                markExpanded(ref temp);
                //try to pass the dic that have the g indicator inside of it, the parent g is the corresponding value of temp
                expandCount++;
                EDH_expand(ref temp, ref pq, ref Gcount, end);
                frige_size = Math.Max(frige_size, pq.count);
            }
            redrawBitmap();
            label4.Text = "Path Finding Fail: dead end";
            watch.Stop();

        }
        private double cal_EDH(Tuple<int, int> start, Tuple<int, int> end) {
            return Math.Sqrt(Math.Pow(start.Item2-start.Item1,2)+Math.Pow(end.Item2-end.Item1,2));
        }
        private void EDH_expand(ref Tuple<int, int> node, ref PriorityQueue pq, ref Dictionary<Tuple<int, int>, int> Gcount, Tuple<int, int> end) {
            int g = Gcount[node] + 1;
            if (node.Item2 < Dim - 1)
            {   //if Gcout[bn] is already exist-> update the value if the new g is smaller than the pervious one and then do the update
                //if y cord have not reach the bottom bound, we can try push the bottom most one to stack
                Tuple<int, int> bn = new Tuple<int, int>(node.Item1, node.Item2 + 1);
                if (Maze[bn])
                {
                    if (pq.Contain(bn))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[bn] > g)
                        {
                            Gcount[bn] = g;
                        }
                        pq.Update(bn, Gcount[bn] + cal_EDH(bn, end),node,ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(bn, g + cal_EDH(bn, end));
                        Gcount.Add(bn, g);
                        recordPath(bn,node);

                    }
                }
            }
            if (node.Item1 < Dim - 1)
            {
                // if x cord have not reach the right most bound, we can try push the right most one to the stack
                Tuple<int, int> rn = new Tuple<int, int>(node.Item1 + 1, node.Item2);
                if (Maze[rn])
                {
                    if (pq.Contain(rn))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[rn] > g)
                        {
                            Gcount[rn] = g;
                        }
                        pq.Update(rn, Gcount[rn] + cal_EDH(rn, end),node,ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(rn, g + cal_EDH(rn, end));
                        Gcount.Add(rn, g);
                        recordPath(rn, node);

                    }
                }
            }
            if (node.Item2 > 0)
            {
                //if y cord is larger than 1, then we can try push the up node to stack
                Tuple<int, int> un = new Tuple<int, int>(node.Item1, node.Item2 - 1);
                if (Maze[un])
                {
                    if (pq.Contain(un))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[un] > g)
                        {
                            Gcount[un] = g;
                        }
                        pq.Update(un, Gcount[un] + cal_EDH(un, end), node, ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(un, g + cal_EDH(un, end));
                        Gcount.Add(un, g);
                        recordPath(un, node);

                    }
                }
            }
            if (node.Item1 > 0)
            {
                // if x cord is larger than 1, then we can try push the left node to stack
                Tuple<int, int> ln = new Tuple<int, int>(node.Item1 - 1, node.Item2);
                if (Maze[ln])
                {
                    if (pq.Contain(ln))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[ln] > g)
                        {
                            Gcount[ln] = g;
                        }
                        pq.Update(ln, Gcount[ln] + cal_EDH(ln, end), node, ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(ln, g + cal_EDH(ln, end));
                        Gcount.Add(ln, g);
                        recordPath(ln, node);

                    }
                }
            }
        }
        #endregion
        #region MDH
        private void button5_Click(object sender, EventArgs e)

        {

            //A* MDH ALG
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int frige_size = 0;
            pictureBox1.ContextMenuStrip = null;
            disable_pathFinding();
            int expandCount = 1;
            Tuple<int, int> end = new Tuple<int, int>(Dim - 1, Dim - 1);
            //count the G of each expanded node
            Dictionary<Tuple<int, int>, int> Gcount = new Dictionary<Tuple<int, int>, int>();
            PriorityQueue pq = new PriorityQueue();
            Tuple<int, int> start = new Tuple<int, int>(0,0);
            Solution = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
            Maze[start] = false;
            Gcount.Add(start, 0);
            pq.Enqueue(start,Gcount[start]+cal_MDH(start,end));
            while (pq.count > 0) {
                Tuple<int,int> temp= pq.Dequeue();
                Maze[temp] = false;
                if (temp.Item1 == end.Item1 && temp.Item2 == end.Item2)
                {
                    //we find a path to the end
                    watch.Stop();
                    markExpanded(ref end);
                    drawPath();
                    redrawBitmap();
                    label3.Text = "Expanded Node Count:" + expandCount;
                    label4.Text = "Path Finding Succeed! ";
                    var elapsedMs = watch.ElapsedMilliseconds;
                    label7.Text = elapsedMs.ToString() + " ms";
                    fringe.Text = frige_size.ToString();
                    return;
                }
                markExpanded(ref temp);
                //try to pass the dic that have the g indicator inside of it, the parent g is the corresponding value of temp
                expandCount++;
                MDH_expand(ref temp,ref pq, ref Gcount, end);
                frige_size = Math.Max(frige_size, pq.count);
            }
            redrawBitmap();
            label4.Text = "Path Finding Fail: dead end";
            watch.Stop();

        }
        private int cal_MDH(Tuple<int, int> start, Tuple<int, int> end) {
            return Math.Abs(end.Item1 - start.Item1) + Math.Abs(end.Item2-start.Item2);
        }
        private void MDH_expand(ref Tuple<int,int> node, ref PriorityQueue pq, ref Dictionary<Tuple<int, int>, int> Gcount,  Tuple<int,int> end) {
            int g = Gcount[node] + 1;
            if (node.Item2 < Dim - 1)
            {   //if Gcout[bn] is already exist-> update the value if the new g is smaller than the pervious one and then do the update
                //if y cord have not reach the bottom bound, we can try push the bottom most one to stack
                Tuple<int, int> bn = new Tuple<int, int>(node.Item1, node.Item2 + 1);
                if (Maze[bn])
                {
                    if (pq.Contain(bn))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[bn] > g)
                        {
                            Gcount[bn] = g;
                        }
                        pq.Update(bn, Gcount[bn] + cal_MDH(bn, end), node, ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(bn, g + cal_MDH(bn, end));
                        Gcount.Add(bn, g);
                        recordPath(bn, node);
                    }
                }
            }
            if (node.Item1 < Dim - 1)
            {
                // if x cord have not reach the right most bound, we can try push the right most one to the stack
                Tuple<int, int> rn = new Tuple<int, int>(node.Item1 + 1, node.Item2);
                if (Maze[rn] )
                {
                    if (pq.Contain(rn))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[rn] > g)
                        {
                            Gcount[rn] = g;
                        }
                        pq.Update(rn, Gcount[rn] + cal_MDH(rn, end), node, ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(rn, g + cal_MDH(rn, end));
                        Gcount.Add(rn, g);
                        recordPath(rn, node);

                    }
                }
            }
            if (node.Item2 > 0)
            {
                //if y cord is larger than 1, then we can try push the up node to stack
                Tuple<int, int> un = new Tuple<int, int>(node.Item1, node.Item2 - 1);
                if (Maze[un])
                {
                    if(pq.Contain(un))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[un] > g)
                        {
                            Gcount[un] = g;
                        }
                        pq.Update(un, Gcount[un] + cal_MDH(un, end), node, ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(un, g + cal_MDH(un, end));
                        Gcount.Add(un, g);
                        recordPath(un, node);

                    }
                }
            }
            if (node.Item1 > 0)
            {
                // if x cord is larger than 1, then we can try push the left node to stack
                Tuple<int, int> ln = new Tuple<int, int>(node.Item1 - 1, node.Item2);
                if (Maze[ln])
                {
                    if (pq.Contain(ln))
                    {
                        //update the value if possible
                        //if pq contains, we can say that bn is alreay in Gcout, then we first have to try refresh Gcount[bn]
                        if (Gcount[ln] > g)
                        {
                            Gcount[ln] = g;
                        }
                        pq.Update(ln, Gcount[ln] + cal_MDH(ln, end), node, ref Solution);
                    }
                    else
                    {
                        pq.Enqueue(ln, g + cal_MDH(ln, end));
                        Gcount.Add(ln, g);
                        recordPath(ln, node);

                    }
                }
            }
        }
        #endregion
        #endregion

        #region GUI Utils
        private void markExpanded(ref Tuple<int, int> t) {
            if (Dim > 400) { return; }
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            expanded.FillRectangle(Brushes.Orange,t.Item1*blockSize,t.Item2*blockSize,blockSize,blockSize);
        }
        private void markSolution(ref Tuple<int, int> t) {
            if (Dim > 400) { return; }
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            expanded.FillRectangle(Brushes.Green, t.Item1 * blockSize, t.Item2 * blockSize, blockSize, blockSize);
        }
        private void recordPath(Tuple<int, int> child, Tuple<int, int> parent) {

            if (Solution.Count==0 || !Solution.Keys.Contains(child))
            {
                Solution.Add(child, parent);
            }
            return;
        }
        private void drawPath() {
            if (Dim > 400) {
                return;
            }
            Tuple<int, int> end = new Tuple<int, int>(Dim - 1, Dim - 1);
            Tuple<int, int> start = new Tuple<int, int>(0, 0);
            int path_count = 1;
            while (!end.Equals(start))
            {
                foreach (KeyValuePair<Tuple<int, int>, Tuple<int, int>> c in Solution)
                {
                    if (c.Key.Equals(end))
                    {
                        markSolution(ref end);
                        path_count++;
                        end = c.Value;
                    }
                }
            }
            label5.Text = "Path Count:" + path_count;

        }
        private void redrawBitmap() {
            if (Dim > 400) {
                return;
            }
            int bitMapSize = Dim * blockSize;
            Graphics flagGraphics = Graphics.FromImage(pictureBox1.Image);
            for (int i = 0; i <= bitMapSize; i += blockSize)
            {
                //draw grid lines
                flagGraphics.DrawLine(Pens.Black, i, 0, i, bitMapSize);
                flagGraphics.DrawLine(Pens.Black, 0, i, bitMapSize, i);
            }
            pictureBox1.Refresh();
            
        }
        private void disable_pathFinding() {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button5.Enabled = false;

        }
        private void enable_pathFinding() {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button5.Enabled = true;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            foreach (RadioButton r in groupBox1.Controls) {
                r.Checked = false;
            }
        }
        private void Cell_state(object send, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right)
            {
                 x_single_point = e.X/blockSize;
                 y_single_point = e.Y/blockSize;
            }
            return;

        }
        private void group_select(object sender, MouseEventArgs e) {

        }
        private void select_start(object sender, MouseEventArgs e) {
        }
        private void select_end(object sender, MouseEventArgs e)
        {
        }
        private void openThisNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tuple<int, int> change = new Tuple<int, int>(x_single_point,y_single_point);
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            expanded.FillRectangle(Brushes.White, change.Item1 * blockSize, change.Item2 * blockSize, blockSize, blockSize);
            Maze[change] = true;
            redrawBitmap();
        }
        private void closeThisNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tuple<int, int> change = new Tuple<int, int>(x_single_point, y_single_point);
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            expanded.FillRectangle(Brushes.Black, change.Item1 * blockSize, change.Item2 * blockSize, blockSize, blockSize);
            Maze[change] = false;
            redrawBitmap();
        }
      
        #region save a copy 
        private void button6_Click(object sender, EventArgs e)
        {
            //make deep copy here
            BackUpMaze = new Dictionary<Tuple<int, int>, Boolean>();
            foreach (KeyValuePair< Tuple<int, int>, Boolean > kvp in Maze) {
                BackUpMaze.Add(kvp.Key,kvp.Value);
            }
            return;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Maze = new Dictionary<Tuple<int, int>, Boolean>();
            foreach (KeyValuePair<Tuple<int, int>, Boolean> kvp in BackUpMaze)
            {
                Maze.Add(kvp.Key, kvp.Value);
            }
            Dim =(int) Math.Sqrt(Maze.Count);
            if (Dim > 11)
            {
                blockSize = 800 / Dim;
            }
            else {
                blockSize = 40;
            }
            int bitMapSize = Dim * blockSize;
            Bitmap flag = new Bitmap(bitMapSize, bitMapSize);
            Graphics flagGraphics = Graphics.FromImage(flag);
            for (int i = 0; i < bitMapSize; i += blockSize)
            {
                //draw grid view of the maze
                for (int j = 0; j < bitMapSize; j += blockSize)
                {
                    if (Maze[new Tuple<int, int>(i / blockSize, j / blockSize)])
                    {
                        flagGraphics.FillRectangle(Brushes.White, i, j, blockSize, blockSize);
                    }
                    else {
                        flagGraphics.FillRectangle(Brushes.Black, i, j, blockSize, blockSize);
                    }
                }
            }
            for (int i = 0; i <= bitMapSize; i += blockSize)
            {
                //draw grid lines
                flagGraphics.DrawLine(Pens.Black, i, 0, i, bitMapSize);
                flagGraphics.DrawLine(Pens.Black, 0, i, bitMapSize, i);
            }
            pictureBox1.Image = flag;
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            enable_pathFinding();
        }
        #endregion
        #endregion
       
        #region change Occupation Rate
        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.1;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.2;
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.3;
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.4;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.5;
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.6;
        }

        private void radioButton13_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.7;
        }

        private void radioButton14_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.8;
        }

        private void radioButton15_CheckedChanged(object sender, EventArgs e)
        {
            OccupationRate = 0.9;
        }
        #endregion
        
        #region change Dim
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 3;
            textBox1.Text = null;
        }
       

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 4;
            textBox1.Text = null;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 5;
            textBox1.Text = null;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 6;
            textBox1.Text = null;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 7;
            textBox1.Text = null;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 8;
            textBox1.Text = null;
        }

        private void radioButton16_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 9;
            textBox1.Text = null;
        }

        private void radioButton18_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 10;
            textBox1.Text = null;
        }

        private void radioButton17_CheckedChanged(object sender, EventArgs e)
        {
            Dim = 11;
            textBox1.Text = null;
        }



        #endregion

       
    }
}
