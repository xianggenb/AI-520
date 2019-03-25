using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class Form1 : Form
    {   
        // Map contains the info if a cell is a mine or not
        //reserve int value -1 to be mine
        // or the number will be 0-8;
        private static Dictionary<Tuple<int, int>, int> Map;
        private static Dictionary<Tuple<int, int>, int> ClearList;
        private static Dictionary<Tuple<int, int>, int> CoverList;
        private static Dictionary<Tuple<int, int>, int> Mines;
        private static Queue<Tuple<int, int>> safelist;
        // first create the whole graph in the Map
        // the clear list is for un-covered cells
        // the coverlist is for unkonw cells
        private static int height = 1;
        private static int width = 1;
        private static int Mine_num = 0;
        private static int blockSize = 16;
        // following container are used for tracking the status of the map
        private static Queue<Tuple<int, int>> safeCells;
        private static List<Tuple<int, int>> mineFound;
        //self implemented priority queue to contain the surrounded cells
        // put element with lowst rate of being a mine to the front
        private static PriorityQueue pq;
        private List<Tuple<int, int>> cellPool;
        private static double pool_prob;

 
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        // initialize map
        private void button1_Click(object sender, EventArgs e)
        {
            this.pictureBox1.MouseClick -= new MouseEventHandler(Cell_state);
            if (!string.IsNullOrWhiteSpace(textBox1.Text)
                && !string.IsNullOrWhiteSpace(textBox2.Text)
                && !string.IsNullOrWhiteSpace(textBox3.Text)
                )
            {
                if (int.TryParse(textBox1.Text, out height)
                    && int.TryParse(textBox2.Text, out width)
                    && int.TryParse(textBox3.Text, out Mine_num)
                    )
                {
                    // generate a bitmap with these three load factors.
                    Map = new Dictionary<Tuple<int, int>, int>();
                    Mines = new Dictionary<Tuple<int, int>, int>();
                    CoverList = new Dictionary<Tuple<int, int>, int>();
                    ClearList = new Dictionary<Tuple<int, int>, int>();
                    int W = width * blockSize;
                    int H = height * blockSize;
                    Bitmap flag = new Bitmap(W, H);
                    Graphics flagGraphics = Graphics.FromImage(flag);
                    for (int i = 0; i < W; i+=blockSize) {
                        for (int j = 0; j < H; j+=blockSize) {
                            flagGraphics.FillRectangle(Brushes.White, i, j, blockSize, blockSize);
                            Map.Add(new Tuple<int, int>(i / blockSize, j / blockSize), 0);
                        }
                    }
                    for (int i = 0; i <= W; i += blockSize)
                    {
                           flagGraphics.DrawLine(Pens.Black, i, 0, i, H);
                           
                    }
                    for (int j = 0; j <= H; j += blockSize) {
                        flagGraphics.DrawLine(Pens.Black, 0, j, W, j);
                    }
                    pictureBox1.Image = flag;
                    pictureBox1.Refresh();
                    Minerm.Text = Mine_num.ToString();
                    
                }
                else
                {
                    return;
                }
            }
            else {
                return;
            }
        }
        // set up user define mines
        private void button5_Click(object sender, EventArgs e)
        {
            this.pictureBox1.MouseClick += new MouseEventHandler(Cell_state);
        }
        // auto generate mines
        private void button7_Click(object sender, EventArgs e)
        {
            int size = width * height;
            Random rd = new Random();
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            //deep copy everything in Map to coverlist
            while (Mine_num > 0) {
                int index = rd.Next() % size;
                int x_index = index % width;
                int y_index = index / width;
                if (Map[new Tuple<int, int>(x_index, y_index)] != -1) {
                    Map[new Tuple<int, int>(x_index, y_index)] = -1;
                    Mines.Add(new Tuple<int, int>(x_index, y_index), -1);
                    Mine_num--;
                    Minerm.Text = Mine_num.ToString();
                    expanded.FillRectangle(Brushes.Red, x_index * blockSize, y_index * blockSize, blockSize, blockSize);
                }
            }
            pictureBox1.Refresh();
            return;
        }
        // set up clue number for initialization
        private void button4_Click(object sender, EventArgs e)
        {
            Graphics mark = Graphics.FromImage(pictureBox1.Image);
            foreach (Tuple<int, int> kvp in Map.Keys.ToList()) {
                switch (get_cluenum(kvp)) {
                    case -1:
                        break;
                    case 0:
                        break;
                    case 1:
                        Map[kvp] = 1;
                        mark.DrawImage(Properties.Resources.Image1, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 2:
                        Map[kvp] = 2;
                        mark.DrawImage(Properties.Resources.Image2, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 3:
                        Map[kvp] = 3;
                        mark.DrawImage(Properties.Resources.Image3, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 4:
                        Map[kvp] = 4;
                        mark.DrawImage(Properties.Resources.Image4, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 5:
                        Map[kvp] = 5;
                        mark.DrawImage(Properties.Resources.Image5, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 6:
                        Map[kvp] = 6;
                        mark.DrawImage(Properties.Resources.Image6, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 7:
                        Map[kvp] = 7;
                        mark.DrawImage(Properties.Resources.Image7, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;
                    case 8:
                        Map[kvp] = 8;
                        mark.DrawImage(Properties.Resources.Image8, kvp.Item1 * blockSize, kvp.Item2 * blockSize);
                        break;

                }
            }
            redraw_grid();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            expanded.FillRectangle(Brushes.Gray,0,0, width * blockSize, height * blockSize);
            CoverList = new Dictionary<Tuple<int, int>, int>();
            foreach (KeyValuePair<Tuple<int, int>, int> kvp in Map)
            {
                CoverList.Add(kvp.Key,kvp.Value);
            }
            redraw_grid();
        }
        private void button6_Click(object sender, EventArgs e)
        {   // open the first entry until there is not 0 around
            safelist = new Queue<Tuple<int, int>>();
            statusIndicator.Text = "N/A";
            Tuple<int, int> entry = new Tuple<int, int>(-1,-1);
            int size = width * height;
            Random rd = new Random();
            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            // reserve the first save point for entry
            while (entry.Item1 < 0) {
                int index = rd.Next() % size;
                int x_index = index % width;
                int y_index = index / width;
                if (Map[new Tuple<int, int>(x_index, y_index)] ==0
                    &&CoverList.ContainsKey(new Tuple<int, int>(x_index, y_index))
                    ) {
                    entry = new Tuple<int, int>(x_index,y_index);
                }
            }
            safelist.Enqueue(entry);
            while (safelist.Count > 0) {
                Tuple<int, int> temp = safelist.Peek();
                safelist.Dequeue();
                ClearList.Add(temp, Map[temp]);
                CoverList.Remove(temp);
                open_clue(temp);
                open_nextcell(temp);
            }
            pictureBox1.Refresh();
            //initialize the pre conditions for following solve
            safeCells = new Queue<Tuple<int, int>>();
            mineFound = new List<Tuple<int, int>>();
            pq = new PriorityQueue();
            cellPool = new List<Tuple<int, int>>();
            // push all cell in CoverList to cellPool first
            foreach (KeyValuePair<Tuple<int, int>, int> t in CoverList) {
                cellPool.Add(t.Key);
            }
            //first try to find mines that is sure to be 
            tryUpdate();
            return;
        }
        private double getPoolProb() {
            int remain_mine = Mines.Count - mineFound.Count;
            int size = CoverList.Count-safeCells.Count;
            double res = 10;
            if (size > 0)
            {
                 res = (double)remain_mine / size;
            }
            return res;

        }
        private int getSurMineNum(Tuple<int, int> input) {
            int res = 0;
            List<Tuple<int, int>> temp = get_sur_cover(input);
            foreach (Tuple<int, int> t in temp)
            {
                if (mineFound.Contains(t))
                {
                    res++;
                }
            }
            return res;
        }
        private List<Tuple<int, int>> getAllSurUncov(Tuple<int,int> input) {
            List<Tuple<int,int>> res=new List<Tuple<int, int>>();
            List<Tuple<int, int>> temp = get_sur_cover(input);
            foreach (Tuple<int, int> t in temp) {
                if (CoverList.ContainsKey(t)) {
                    res.Add(t);
                }
            }
            return res;
        }
        private int get_sur_covered_num(Tuple<int,int> input) {
            List<Tuple<int, int>> sur = get_sur_cover(input);
            int res = 0;
            foreach (Tuple<int, int> t in sur) {
                if (CoverList.ContainsKey(t)) {
                    res++;
                }
            }
            foreach (Tuple<int, int> t in sur) {
                if (mineFound.Contains(t)) {
                    res++;
                }
            }
            return res;

        }
        private int get_cluenum(Tuple<int, int> input) {
            if (Map[input] == -1)
            {
                return -1;
            }
            return get_sur_minnum(input);
        }
        private int get_sur_minnum(Tuple<int, int> input) {
            // get number of mines of surrounding cells
            int sum = 0;
            List<Tuple<int, int>> l = new List<Tuple<int, int>>();
            l.Add(new Tuple<int, int>(input.Item1 - 1, input.Item2 - 1));
            l.Add(new Tuple<int, int>(input.Item1 - 1, input.Item2));
            l.Add(new Tuple<int, int>(input.Item1 - 1, input.Item2 +1));
            l.Add(new Tuple<int, int>(input.Item1 , input.Item2 - 1));
            l.Add(new Tuple<int, int>(input.Item1 , input.Item2 + 1));
            l.Add(new Tuple<int, int>(input.Item1 + 1, input.Item2 - 1));
            l.Add(new Tuple<int, int>(input.Item1 + 1, input.Item2 ));
            l.Add(new Tuple<int, int>(input.Item1 + 1, input.Item2 + 1));
            foreach (Tuple<int, int> t in l) {
                if (Map.ContainsKey(t)) {
                    if (Map[t] == -1) {
                        sum++;
                    }
                }
            }
            return sum;
        }
        private List<Tuple<int, int>> get_sur_cover(Tuple<int,int> input) {
            List<Tuple<int, int>> l = new List<Tuple<int, int>>();
            l.Add(new Tuple<int, int>(input.Item1 - 1, input.Item2 - 1));
            l.Add(new Tuple<int, int>(input.Item1 - 1, input.Item2));
            l.Add(new Tuple<int, int>(input.Item1 - 1, input.Item2 + 1));
            l.Add(new Tuple<int, int>(input.Item1, input.Item2 - 1));
            l.Add(new Tuple<int, int>(input.Item1, input.Item2 + 1));
            l.Add(new Tuple<int, int>(input.Item1 + 1, input.Item2 - 1));
            l.Add(new Tuple<int, int>(input.Item1 + 1, input.Item2));
            l.Add(new Tuple<int, int>(input.Item1 + 1, input.Item2 + 1));
            return l;
        }

        private void Cell_state(object send, MouseEventArgs e)
        {
            if (Mine_num <= 0) {
                this.pictureBox1.MouseClick -= new MouseEventHandler(Cell_state);
                return; }
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X / blockSize;
                int y = e.Y / blockSize;
                Map[new Tuple<int, int>(x, y)] = -1;
                Mines.Add(new Tuple<int, int>(x,y), -1);
                Mine_num--;
                Minerm.Text = Mine_num.ToString();
                Graphics expanded = Graphics.FromImage(pictureBox1.Image);
                expanded.FillRectangle(Brushes.Red, x * blockSize, y * blockSize, blockSize, blockSize);
                if (Mine_num == 0) {
                    this.pictureBox1.MouseClick -= new MouseEventHandler(Cell_state);
                }
                this.Refresh();
            }
            return;

        }

        private void open_nextcell(Tuple<int, int> input) {
            // if there is a mine
            
            if (Map[input] == -1)
            {
                // we hit a mine
                statusIndicator.Text = "Fail";
                return;
            }
            
            // then opern the cell
           
             if (Map[input] == 0) {
                List<Tuple<int, int>> to_open = get_sur_cover(input);
                foreach (Tuple<int, int> t in to_open) {
                    if (CoverList.ContainsKey(t)
                        &&!ClearList.ContainsKey(t)
                        &&!safelist.Contains(t)
                        ) {
                        safelist.Enqueue(t);
                    }
                }
            }

        }
        private void open_clue(Tuple<int, int> input) {
            Graphics mark = Graphics.FromImage(pictureBox1.Image);
            switch (Map[input]) {
                case -1:
                    mark.FillRectangle(Brushes.Red, input.Item1 * blockSize, input.Item2 * blockSize, blockSize,blockSize);
                    break;
                case 0:
                    mark.FillRectangle(Brushes.White, input.Item1 * blockSize, input.Item2 * blockSize, blockSize, blockSize);
                    break;
                case 1:
                    mark.DrawImage(Properties.Resources.Image1, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 2:
                    mark.DrawImage(Properties.Resources.Image2, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 3:
                    mark.DrawImage(Properties.Resources.Image3, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 4:
                    mark.DrawImage(Properties.Resources.Image4, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 5:
                    mark.DrawImage(Properties.Resources.Image5, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 6:
                    mark.DrawImage(Properties.Resources.Image6, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 7:
                    mark.DrawImage(Properties.Resources.Image7, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;
                case 8:
                    mark.DrawImage(Properties.Resources.Image8, input.Item1 * blockSize, input.Item2 * blockSize);
                    break;


            }
        }
        private void redraw_grid() {
            int W = width * blockSize;
            int H = height * blockSize;
            Graphics flagGraphics = Graphics.FromImage(pictureBox1.Image);
            
            for (int i = 0; i <= W; i += blockSize)
            {
                flagGraphics.DrawLine(Pens.Black, i, 0, i, H);

            }
            for (int j = 0; j <= H; j += blockSize)
            {
                flagGraphics.DrawLine(Pens.Black, 0, j, W, j);
            }
            pictureBox1.Refresh();
        }
        // next step method
        private void button2_Click(object sender, EventArgs e)
        {
            // here we already get some clue about the map
            // we have a clearlist indicated opened cells, and a cover list indicated the closed cells
            // how can we decide what next cell to open?
            // my solution is to find the probablity of one cell to be mine
            // here we will maintain 4 dictionaries, one is safe cell, indicate that the cell cannot be a mine
            // the second one is mine dictinary, every node in the dic is the mine that we found already,
            // the third one is the close cells that surround by at least one number, we can cacluate the probability of 
            // the cell to be a mine with clues
            // the last one is closed cell pool, where there is no surround cells and the rate of it can be calculated by
            // total mines- found mines-mines in sourround cells/total number of cells in the poll
            // for each step we first see if the safe collection is empty, if not, open one and update all the info
            // if it is empty and we have to make decision, if the cell pool have less prob to hit a mine, randomly open one
            // if the surrounded cells have lower prob, open the cell with lowest prob and updata all info
            // terminals when there is no uncovered cells or hit a mine by accident
            tryUpdate();
            nextStep();
        }

        private void nextStep() {
            if (CoverList.Count == 0) {
                statusIndicator.Text = "success";
                return;
            }
            tryUpdate();
            // first we check out if there is any safe cells;
            if (safeCells.Count > 0)
            {
                // safe cell is not empty, we can safely open a cell
                Tuple<int, int> temp = safeCells.Dequeue();
                openThisCell(temp);
            }
            else {
                if (pq.count > 0) {
                    Tuple<Tuple<int, int>, double> top_pq = pq.Peek();
                    double pq_rate = top_pq.Item2;
                    if (pool_prob < pq_rate && cellPool.Count  >0)
                    {
                        // here we radomly open a cell in the cell pool
                        Random rd = new Random();
                        int index = rd.Next() % cellPool.Count;
                        Tuple<int, int> lucky = cellPool[index];
                        cellPool.RemoveAt(index);
                        openThisCell(lucky);
                    }
                    else {
                        pq.Dequeue();
                        // here we open this cell at top of pq;
                        openThisCell(top_pq.Item1);
                    }
                }
            }

        }
        private void testAlg() {
            foreach (Tuple<int, int> t in safeCells) {
                if (Map[t] == -1) {
                    System.Diagnostics.Debug.WriteLine("Alg error");
                }
            }
        }
        private int openThisCell(Tuple<int, int> input) {
            if (Map[input] == -1)
            { // if we hit a mine
                Random rd = new Random();
                if (rd.Next() % 10 < 5)
                {
                    statusIndicator.Text = "fail";
                    Graphics mark = Graphics.FromImage(pictureBox1.Image);
                    mark.DrawImage(Properties.Resources.explode, input.Item1 * blockSize, input.Item2 * blockSize);
                }
                return -1;
            }
            else {
                if (CoverList.ContainsKey(input)){
                    CoverList.Remove(input);
                }
                if (!ClearList.ContainsKey(input)) {
                    ClearList.Add(input, Map[input]);
                }
                open_clue(input);
                // open this cell and update info to clear list
                //delete from cover cell
                return 1;
            }
        }
        // most important function of this project
        // this function analysis all the open cell and put proper cell to its container
        private void tryUpdate() {

            Graphics expanded = Graphics.FromImage(pictureBox1.Image);
            foreach (KeyValuePair<Tuple<int, int>, int> kvp in ClearList)
            {
                int num = get_sur_covered_num(kvp.Key);
                if (num == 0)
                {
                    //here we know its not an edge cell or a dead cell
                    continue;
                }
                else if (num == kvp.Value)
                {
                    // here we know surrounded elements are all mines
                    // put every uncovered cells to mine and mark them if not in the mine list
                    // remove all cells in the Coverlist to mine list
                    List<Tuple<int, int>> tomark = getAllSurUncov(kvp.Key);//every node in coverlist
                    
                    foreach (Tuple<int, int> t in tomark)
                    {
                        if (!mineFound.Contains(t))
                        {
                            mineFound.Add(t);
                            expanded.DrawImage(Properties.Resources.mineflag, t.Item1 * blockSize, t.Item2 * blockSize);
                        }
                        if (CoverList.ContainsKey(t))
                        {
                            CoverList.Remove(t);
                        }
                        if (pq.Contain(t))
                        {
                            pq.Delete(t);
                        }
                        if (cellPool.Contains(t))
                        {
                            cellPool.Remove(t);
                        }

                    }
                }
                else if (num > kvp.Value)
                {
                    // here we know that the surround 
                    if (getSurMineNum(kvp.Key) == kvp.Value)
                    {
                        // all mine already found
                        //push else to safe list
                        // mines are not in the coverlist
                        // but remember tuples in the safelist is still in the coverlist
                        List<Tuple<int, int>> safe = getAllSurUncov(kvp.Key);
                        if (safe.Count != 0)
                        {
                            foreach (Tuple<int, int> t in safe)
                            {
                                if (!safeCells.Contains(t))
                                {
                                    safeCells.Enqueue(t);
                                }
                                if (cellPool.Contains(t))
                                {
                                    cellPool.Remove(t);
                                }
                                if (pq.Contain(t))
                                {
                                    pq.Delete(t);
                                }
                            }
                        }

                    }
                    else
                    {
                        // we cannot find safe node, but we can update the probablity for the corresponding uncovered cells
                        List<Tuple<int, int>> uncover = getAllSurUncov(kvp.Key);
                        int surmine = getSurMineNum(kvp.Key);
                        int mine_left = kvp.Value - surmine;
                        int total = uncover.Count;
                        double rate = (double)mine_left / total;
                        foreach (Tuple<int, int> t in uncover)
                        {

                            //try implement new algorithm
                            if (cellPool.Contains(t))
                            {
                                cellPool.Remove(t);
                            }
                            double sum = 0;
                            foreach (Tuple<int, int> tt in uncover) {
                                if (!tt.Equals(t) && pq.Contain(tt)) {
                                    sum += pq.getProb(tt);
                                }
                            }
                            if (sum >= kvp.Value)
                            {
                                if (!safeCells.Contains(t))
                                {
                                    safeCells.Enqueue(t);
                                }
                                if (pq.Contain(t))
                                {
                                    pq.Delete(t);
                                }
                            }
                            else {

                                if (!pq.Contain(t))
                                {
                                    pq.Enqueue(t, rate);
                                }
                                else {

                                    if (pq.getProb(t) > rate
                                        && pq.getProb(t) != -1
                                        )
                                    {
                                        pq.Update(t, rate);

                                    }



                                }
                            }

                           // if (cellPool.Contains(t))
                           // {
                           //     cellPool.Remove(t);
                           // }
                           // if (!pq.Contain(t))
                           // {
                           //     pq.Enqueue(t, rate);
                           // }
                           // else
                           // {   if (surmine > 0)
                           //     {
                           //         if (pq.getProb(t) > rate
                           //             && pq.getProb(t) != -1
                           //             )
                           //         {
                           //             pq.Update(t, rate);
                           //
                           //         }
                           //     }
                           //     else {
                           //         if (pq.getProb(t) < rate
                           //             && pq.getProb(t) != -1
                           //             )
                           //         {
                           //             pq.Update(t, rate);
                           //
                           //         }
                           //     }
                           // }
                        }

                    }
                }
            }
            pool_prob = getPoolProb();
            pictureBox1.Refresh();
            testAlg(); 
            return;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tryUpdate();
            while (statusIndicator.Text != "success"
                  && statusIndicator.Text != "fail") {
                button2_Click(sender,e);
            }
        }
    }
}
