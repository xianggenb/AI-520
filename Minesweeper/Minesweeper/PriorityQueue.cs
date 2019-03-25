using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    class PriorityQueue
    {
        private static List<Tuple<Tuple<int, int>, double>> list;
        public int count { get { return list.Count; } }
        public PriorityQueue()
        {
            list = new List<Tuple<Tuple<int, int>, double>>();
        }
        public void Enqueue(Tuple<int, int> x, double H)
        {
            Tuple<Tuple<int, int>, double> newnode = new Tuple<Tuple<int, int>, double>(x, H);
            list.Add(newnode);
            int i = count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (list[p].Item2 < newnode.Item2)
                {
                    break;
                }
                list[i] = list[p];
                i = p;
            }
            if (count > 0) list[i] = newnode;
        }
        public Tuple<int, int> Dequeue()
        {
            Tuple<Tuple<int, int>, double> del = Peek();
            Tuple<Tuple<int, int>, double> root = list[count - 1];
            list.RemoveAt(count - 1);
            int i = 0;
            while (i * 2 + 1 < count)
            {
                int a = i * 2 + 1;
                int b = i * 2 + 2;
                int c = b < count && list[b].Item2 < list[a].Item2 ? b : a;
                if (list[c].Item2 > root.Item2)
                {
                    break;
                }
                list[i] = list[c];
                i = c;
            }
            if (count > 0) { list[i] = root; }
            return del.Item1;
        }
        public Tuple<Tuple<int, int>, double> Peek()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }
            return list[0];
        }
        public bool Contain(Tuple<int, int> t)
        {
            foreach (Tuple<Tuple<int, int>, double> c in list)
            {
                if (c.Item1.Equals(t))
                {
                    return true;
                }
            }
            return false;
        }

        public double getProb(Tuple<int, int> t) {
            foreach (Tuple<Tuple<int, int>, double> c in list)
            {
                if (c.Item1.Equals(t))
                {
                    return c.Item2;
                }
            }
            return -1;
        }

        public void Update(Tuple<int, int> t, double f)
        {
            //since the new value is larger than old value, we need to percolate down
            Tuple<Tuple<int, int>, double> update = new Tuple<Tuple<int, int>, double>(t, f);
            int i = -1;
            foreach (Tuple<Tuple<int, int>, double> c in list)
            {
                if (c.Item1.Equals(t))
                {
                    i = list.IndexOf(c);
                }
            }
            if (i < 0)
            {
                return;
            }
            if (i == 0)
            {
                Dequeue();
                Enqueue(t, f);
            }
            else
            {
                int parent = (i - 1) / 2;
                if (list[parent].Item2 >= update.Item2)
                {
                    while (i > 0)
                    {
                        int p = (i - 1) / 2;
                        if (list[p].Item2 < update.Item2)
                        {
                            break;
                        }
                        list[i] = list[p];
                        i = p;
                    }
                    if (count > 0) list[i] = update;

                }
                else
                {
                        while (i * 2 + 1 < count)
                        {
                            int a = i * 2 + 1;
                            int b = i * 2 + 2;
                            int c = b < count && list[b].Item2 < list[a].Item2 ? b : a;
                            if (list[c].Item2 > update.Item2)
                            {
                                break;
                            }
                            list[i] = list[c];
                            i = c;
                        }
                        if (count > 0) { list[i] = update; }
                    
                }
            }
        }

        public void Delete(Tuple<int, int> t) {
            int i = -1;
            foreach (Tuple<Tuple<int, int>, double> c in list)
            {
                if (c.Item1.Equals(t))
                {
                    i = list.IndexOf(c);
                }
            }
            if (i < 0)
            {
                return;
            }
            else if (i == 0)
            {
                Dequeue();
            }
            else {
                Tuple<Tuple<int, int>, double> root = list[count - 1];
                list.RemoveAt(count - 1);
                if (root.Item1.Equals(t)) {
                    return;
                }
                while (i * 2 + 1 < count)
                {
                    int a = i * 2 + 1;
                    int b = i * 2 + 2;
                    int c = b < count && list[b].Item2 < list[a].Item2 ? b : a;
                    if (list[c].Item2 > root.Item2)
                    {
                        break;
                    }
                    list[i] = list[c];
                    i = c;
                }
                if (count > 0) { list[i] = root; }
            }
        }

        public int RoundingMine_num() {
            double sum = 0;
            foreach (Tuple<Tuple<int, int>, double> t in list) {
                sum += t.Item2;
            }
            return (int)sum;
        }

    }
}

