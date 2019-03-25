using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazerunner
{
    class PriorityQueue
    {
        private static List<Tuple<Tuple<int,int>,double>> list;
        public int count { get { return list.Count; } }
        public PriorityQueue() {
            list = new List<Tuple<Tuple<int, int>, double>>();
        }
        public void Enqueue(Tuple<int, int> x, double H) {
            Tuple<Tuple<int, int>, double> newnode = new Tuple<Tuple<int, int>, double>(x,H);
            list.Add(newnode);
            int i = count - 1;
            while (i > 0) {
                int p = (i - 1) / 2;
                if (list[p].Item2 < newnode.Item2) {
                    break;
                }
                list[i] = list[p];
                i = p;
            }
            if (count > 0) list[i] = newnode;
        }
        public Tuple<int, int> Dequeue() {
            Tuple<Tuple<int, int>, double> del = Peek();
            Tuple<Tuple<int, int>, double> root = list[count - 1];
            list.RemoveAt(count-1);
            int i = 0;
            while (i * 2 + 1 < count) {
                int a = i * 2 + 1;
                int b = i * 2 + 2;
                int c= b < count && list[b].Item2 < list[a].Item2 ? b : a;
                if (list[c].Item2 > root.Item2) {
                    break;
                }
                list[i] = list[c];
                i = c;
            }
            if (count > 0) { list[i] = root; }
            return del.Item1;
        }
        public Tuple<Tuple<int, int>, double> Peek() {
            if (count == 0) {
                throw new InvalidOperationException("Queue is empty.");
            }
                return list[0];
        }
        public bool Contain(Tuple<int, int> t) {
            foreach (Tuple<Tuple<int, int>, double> c in list) {
                if (c.Item1.Equals(t)) {
                    return true;
                }
            }
            return false;
        }
        public void Update(Tuple<int, int> t, double f, Tuple<int,int> previous, ref Dictionary<Tuple<int,int>,Tuple<int,int>> Solution) {
            //if new value is small than parent, percolate up
            Tuple<Tuple<int, int>, double> update = new Tuple<Tuple<int, int>, double>(t, f);
            int i = -1;
            foreach (Tuple<Tuple<int, int>, double> c in list) {
                if (c.Item1.Equals(t)) {
                    i = list.IndexOf(c);
                }
            }
            if (i < 0) {
                return;
            }
            if (list[i].Item2 > update.Item2) {
                Solution[t] = previous;
            }
            if (i == 0)
            {
                if (list[0].Item2 > update.Item2)
                {
                    list[0] = update;
                    Solution[t] = previous;
                }
            }
            else {
                int parent= (i - 1) / 2;
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
                else {
                    if (i * 2 + 1 < count)
                    {
                        if (list[i].Item2>update.Item2) {
                            list[i] = update;
                        }
                        return;
                    }
                    else {
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
        }
    }
}
