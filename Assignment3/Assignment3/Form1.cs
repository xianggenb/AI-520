using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assignment3
{
    public partial class Form1 : Form
    {   

        // int as indicator of type
        // 1->flat , 2->hilly, 3->forested, 4->maze of caves
        //flat->0.2, hilly 0.3, forest 0.3, maze of caves 0.2
        private static Dictionary<Tuple<int, int>, int> Map;
        private static List<Tuple<int, int>> MapHelper;
        private static Dictionary<Tuple<int, int>, double> clues;
        private static List<Tuple<int, int>> searched;
        private static Tuple<int, int> target;
        private static int blockSize = 10;
        private static int mapSize = 500;
        private static int search_count = 0;
        private static Tuple<int,int> currentCell;
        private static Random rd = new Random();

        public Form1()
        {
            InitializeComponent();
        }
        #region utils
        private void initMap() {
            Map = new Dictionary<Tuple<int, int>, int>();
            clues = new Dictionary<Tuple<int, int>, double>();
            searched = new List<Tuple<int, int>>();
            search_count = 0;
            count_indicator.Text = search_count.ToString();
            //initialize map 
            Bitmap flag = new Bitmap(mapSize, mapSize);
            Graphics flagGraphics = Graphics.FromImage(flag);
            for (int i = 0; i < 50; i++) {
                for (int j = 0; j < 50; j++) {
                    flagGraphics.FillRectangle(Brushes.White, i*blockSize,j*blockSize, blockSize,blockSize);
                    Map.Add(new Tuple<int, int>(i, j), getType());
                }
            }
            //initialize target
            int x = rd.Next() % 50;
            int y = rd.Next() % 50;
            target = new Tuple<int, int>(x,y);
            // initialize the clues
            for (int i = 0; i < 50; i++) {
                for (int j = 0; j < 50; j++) {
                    clues.Add(new Tuple<int, int>(i, j), 0.0004);
                }
            }
            
            MapHelper = Map.Keys.ToList();
           
           
            pictureBox1.Image = flag; 
            mark_Map();
            for (int i = 0; i <= mapSize; i += blockSize)
            {
                flagGraphics.DrawLine(Pens.Black, i, 0, i, mapSize);

            }
            for (int j = 0; j <= mapSize; j += blockSize)
            {
                flagGraphics.DrawLine(Pens.Black, 0, j, mapSize, j);
            }
            pictureBox1.Refresh();
        }
        private int getType() {
            int res = rd.Next() % 10;
            if (res < 2)
            {
                return 1;
            }
            else if (res < 5)
            {
                return 2;
            }
            else if (res < 8)
            {
                return 3;
            }
            else {
                return 4;
            }
        }

        private void mark_Map() {
            Graphics mark = Graphics.FromImage(pictureBox1.Image);
            foreach (KeyValuePair<Tuple<int,int>, int> kvp in Map) {
                if (kvp.Value == 1) {
                    mark.FillRectangle(Brushes.White, kvp.Key.Item1*blockSize,kvp.Key.Item2*blockSize,blockSize,blockSize);
                }
                else if (kvp.Value == 2)
                {
                    mark.FillRectangle(Brushes.Gray, kvp.Key.Item1 * blockSize, kvp.Key.Item2 * blockSize, blockSize, blockSize);
                }
                else if (kvp.Value == 3)
                {
                    mark.FillRectangle(Brushes.Green, kvp.Key.Item1 * blockSize, kvp.Key.Item2 * blockSize, blockSize, blockSize);
                }
                else
                {
                    mark.FillRectangle(Brushes.Black, kvp.Key.Item1 * blockSize, kvp.Key.Item2 * blockSize, blockSize, blockSize);
                }
            }
        }
        private void updateProb(Tuple<int,int> searched) {
            int type = Map[searched];
            if (type == 1)
            {
                // a flat
                double init = clues[searched];
                double scale = 1 -init;
                clues[searched] = init * 0.1;
                double loss = init*0.9;
                // distribute the possibility among all the other cells
                foreach (Tuple<int,int> kvp in MapHelper) {
                    if (!kvp.Equals(searched)) {
                        clues[kvp] = clues[kvp] * (1 + (loss / scale));
                    }
                }

            }
            else if (type == 2)
            {
                double init = clues[searched];
                double scale = 1 - init;
                clues[searched] = init * 0.3;
                double loss = init * 0.7;
                // distribute the possibility among all the other cells
                foreach (Tuple<int, int> kvp in MapHelper)
                {
                    if (!kvp.Equals(searched))
                    {
                        clues[kvp] = clues[kvp] * (1 + (loss / scale));
                    }
                }
                // a hilly
            }
            else if (type == 3)
            {
                double init = clues[searched];
                double scale = 1 - init;
                clues[searched] = init * 0.7;
                double loss = init * 0.3;
                // distribute the possibility among all the other cells
                foreach (Tuple<int, int> kvp in MapHelper)
                {
                    if (!kvp.Equals(searched))
                    {
                        clues[kvp] = clues[kvp] * (1 + (loss / scale));
                    }
                }
                // a forest
            }
            else {
                // a maze of cave
                double init = clues[searched];
                double scale = 1 - init;
                clues[searched] = init * 0.9;
                double loss = init * 0.1;
                // distribute the possibility among all the other cells
                foreach (Tuple<int, int> kvp in MapHelper)
                {
                    if (!kvp.Equals(searched))
                    {
                        clues[kvp] = clues[kvp] * (1 + (loss / scale));
                    }
                }
            }
        }
        private Tuple<int, int> Rule1Cell() {
            Tuple<int, int> res = new Tuple<int, int>(-1,-1);
            double max_prob = -1 ;
            foreach (KeyValuePair<Tuple<int, int>, double> kvp in clues) {
                if (kvp.Value > max_prob)
                {
                    max_prob = kvp.Value;
                    res = kvp.Key;
                }
                
            }
            return res;
        }
        private Tuple<int, int> Rule2Cell()
        {
            Tuple<int, int> res = new Tuple<int, int>(-1, -1);
            double max_prob = -1;
            foreach (KeyValuePair<Tuple<int, int>, double> kvp in clues)
            {
                    double rate = -1;
                    if (Map[kvp.Key] == 1)
                    {
                        // is flat
                        rate = kvp.Value * 0.9;
                    }
                    else if (Map[kvp.Key] == 2)
                    {
                        rate = kvp.Value * 0.7;
                    }
                    else if (Map[kvp.Key] == 3)
                    {
                        rate = kvp.Value * 0.3;
                    }
                    else
                    {
                        rate = kvp.Value * 0.1;
                    }
                
               
                if (rate > max_prob)
                {
                    max_prob = rate;
                    res = kvp.Key;
                }
                
            }
            return res;
        }

        private Tuple<int, int> adjRule1() {
            // here we should calculate the distance first and then divided the probablity by the distance
            // and in this step we shoud keep update the current cell after we found one
            Tuple<int, int> res = new Tuple<int, int>(-1, -1);
            double max_prob = -1;
            foreach (KeyValuePair<Tuple<int, int>, double> kvp in clues)
            {
                double dist = 1;
                if (!kvp.Key.Equals(currentCell)) {
                    dist = Math.Abs(currentCell.Item1 - kvp.Key.Item1) + Math.Abs(currentCell.Item2-kvp.Key.Item2);
                if (kvp.Value/dist > max_prob)
                {
                    max_prob = kvp.Value/dist;
                    res = kvp.Key;
                }

                }
            }
            currentCell = res;
            return res;

        }

        private Tuple<int, int> adjRule2() {
            Tuple<int, int> res = new Tuple<int, int>(-1, -1);
            double max_prob = -1;
            foreach (KeyValuePair<Tuple<int, int>, double> kvp in clues)
            {
                double dist = 1;
                if (!kvp.Key.Equals(currentCell))
                {
                    dist = Math.Abs(currentCell.Item1 - kvp.Key.Item1) + Math.Abs(currentCell.Item2 - kvp.Key.Item2);
                    double rate = -1;
                if (Map[kvp.Key] == 1)
                {
                    // is flat
                    rate = kvp.Value * 0.9;
                }
                else if (Map[kvp.Key] == 2)
                {
                    rate = kvp.Value * 0.7;
                }
                else if (Map[kvp.Key] == 3)
                {
                    rate = kvp.Value * 0.3;
                }
                else
                {
                    rate = kvp.Value * 0.1;
                }

                    rate = rate / dist;

                if (rate > max_prob)
                {
                    max_prob = rate;
                    res = kvp.Key;
                }

                }
            }
            currentCell = res;
            return res;
        }

        private Tuple<int, int> moveTarget() {
            List<Tuple<int, int>> moveable = new List<Tuple<int, int>>();
            int from = Map[target];
            if (target.Item1 - 1 >= 0) {
                moveable.Add(new Tuple<int, int>(target.Item1 - 1, target.Item2));
            }
            if (target.Item1 + 1 <= 49) {
                moveable.Add(new Tuple<int, int>(target.Item1+1, target.Item2));
            }
            if (target.Item2 - 1 >= 0)
            {
                moveable.Add(new Tuple<int, int>(target.Item1, target.Item2-1));
            }
            if (target.Item2 + 1 <= 49)
            {
                moveable.Add(new Tuple<int, int>(target.Item1, target.Item2+1));
            }
            int s = moveable.Count;
            int index = rd.Next() % s;
            target = moveable[index];
            int to = Map[target];
            return new Tuple<int, int>(from,to);
        }

       
        private bool checkCell(Tuple<int, int> input) {
            if (!searched.Contains(input)) {
                searched.Add(input);
            }
            updateProb(input);
            if (input.Equals(target)) {
                if (Map[input] == 1)
                {
                    //flat
                    if (rd.Next() % 10 < 1)
                    {
                        return false;
                    }
                    return true;
                }
                else if (Map[input] == 2)
                {
                    if (rd.Next() % 10 < 3)
                    {
                        return false;
                    }
                    return true;
                }
                else if (Map[input] == 3) {
                    if (rd.Next() % 10 < 7)
                    {
                        return false;
                    }
                    return true;
                }
                else {
                    if (rd.Next() % 10 < 9)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private bool checkCellMove(Tuple<int, int> input) {

            if (!searched.Contains(input))
            {
                searched.Add(input);
            }
            if (input.Equals(target))
            {
                if (Map[input] == 1)
                {
                    //flat
                    if (rd.Next() % 10 < 1)
                    {
                        updateProbMove(moveTarget());// do the update upon fail
                        return false;
                    }
                    return true;
                }
                else if (Map[input] == 2)
                {
                    if (rd.Next() % 10 < 3)
                    {
                        updateProbMove(moveTarget());// do the update upon fail
                        return false;
                    }
                    return true;
                }
                else if (Map[input] == 3)
                {
                    if (rd.Next() % 10 < 7)
                    {
                        updateProbMove(moveTarget());// do the update upon fail
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (rd.Next() % 10 < 9)
                    {
                        updateProbMove(moveTarget());// do the update upon fail
                        return false;
                    }
                    return true;
                }
            }
            updateProbMove(moveTarget());// do the update upon fail
            return false;

        }

        private void getTargetType() {

            if (Map[target] == 1)
            {
                target_type.Text = "flat";
            }
            else if (Map[target] == 2)
            {
                target_type.Text = "hilly";
            }
            else if (Map[target] == 3)
            {
                target_type.Text = "forest";
            }
            else
            {
                target_type.Text = "maze of caves";
            }
        }
        private void updateProbMove(Tuple<int, int> change) {
            // at this step we should keep a list to record the type that matches target change info
            List<Tuple<int, int>> shareProb = new List<Tuple<int, int>>();
            //first get the prob to share among all
            double sharedAmount = 0;
            foreach (Tuple<int, int> t in searched) {
                if (Map[t] != change.Item1 && Map[t] != change.Item2)
                {
                    sharedAmount += clues[t];
                    clues[t] = 0;
                }
                else {
                    shareProb.Add(t);
                }
            }
            foreach (Tuple<int, int> tp in MapHelper) {
                if (!searched.Contains(tp)) {
                    //not searched, simply add it
                    shareProb.Add(tp);
                }
            }
            int shareCount = shareProb.Count;
            double shareFactor = sharedAmount / shareCount;
            foreach (Tuple<int, int> t in shareProb) {
                clues[t] += shareFactor;
            }
        }
        #endregion
        private void button3_Click(object sender, EventArgs e)
        {
            initMap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            while (!checkCell(Rule1Cell())) {
                search_count++;
            }
            getTargetType();
            count_indicator.Text = search_count.ToString();
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (!checkCell(Rule2Cell()))
            {
                search_count++;
            }
            getTargetType();
            count_indicator.Text = search_count.ToString();
            return;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // assume we start from the location (0,0)
            currentCell = new Tuple<int, int>(0, 0);
            // so what is the probablity now to search for the cell?
            // we can fist calculate the distance to reach the target cell and then find out the highest probablity scaled by the distance.
            while (!checkCell(adjRule1()))
            {
                search_count++;
            }
            getTargetType();
            count_indicator.Text = search_count.ToString();
            return;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // assume we start from the location (0,0)
            currentCell = new Tuple<int, int>(0, 0);
            // so what is the probablity now to search for the cell?
            // we can fist calculate the distance to reach the target cell and then find out the highest probablity scaled by the distance.
            while (!checkCell(adjRule2()))
            {
                search_count++;
            }
            getTargetType();
            count_indicator.Text = search_count.ToString();
            return;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // new rule of finding:
            // upon a fail search, check out the the move infomation of the target
            // then we scan all the cells, to see if the type of the cell matches the info given(either of the type)

            // if the cells is of the types, reduce its prob to 0 and split it among all other cells, including none-searched cells and 
            // searched cells that is of the proper type.
            // then check out the cell with the move prob and repeat this procedure until we find one
            // however, here is a problem, when we move through cells, the previous probablity may not work anymore, so here we cannot 
            // increase the probablity propotionally as before, that is to say, we sum up all the probablity that is reduced to 0, and 
            // equally distribute the probablity among all
            // and there is another way of optimization. for each cells, we may check out the combinations between this cells and surrounded cells,
            // and if there is no such combination, we can ignore this cells and set its probabliy to zero
            target_type.Text = "TBD";
            while (!checkCell(Rule2Cell()))
            {
                search_count++;
            }
            if (Map[target] == 1)
            {
                target_type.Text = "flat";
            }
            else if (Map[target] == 2)
            {
                target_type.Text = "hilly";
            }
            else if (Map[target] == 3)
            {
                target_type.Text = "forest";
            }
            else
            {
                target_type.Text = "maze of caves";
            }
            count_indicator.Text = search_count.ToString();
            return;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            target_type.Text = "TBD";
            while (!checkCell(Rule1Cell()))
            {
                search_count++;
            }
            if (Map[target] == 1)
            {
                target_type.Text = "flat";
            }
            else if (Map[target] == 2)
            {
                target_type.Text = "hilly";
            }
            else if (Map[target] == 3)
            {
                target_type.Text = "forest";
            }
            else
            {
                target_type.Text = "maze of caves";
            }
            count_indicator.Text = search_count.ToString();
            return;
        }
    }
}
