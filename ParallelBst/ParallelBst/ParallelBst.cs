using System;
using System.Collections.Generic;
using System.Threading;


namespace ParallelBst
{
    public class ParallelBst<TKey> where TKey : IComparable<TKey>
    {
        public int Sim;
        public int AreDeleted;
        public int AreInserted;
        private readonly object _insertRoot = new object();
        private readonly object _inc = new object();


        private Node<TKey> _root;


        public Node<TKey> Find(TKey k)
        {
            var node = _root;
            while (node != null)
            {
                if (k.CompareTo(node.Key) < 0) node = node.LeftChild;
                else if (k.CompareTo(node.Key) > 0) node = node.RightChild;
                else
                {
                    lock (node)
                    {
                        if (node.LeftChild != null && node.LeftChild.Father == node ||
                            node.RightChild != null && node.RightChild.Father == node) return node;
                        return null;
                    }
                }
            }

            return null;
        }


        public void ParallelInsert(TKey key)
        {
            lock (_insertRoot)
            {
                if (_root == null)
                {
                    _root = new Node<TKey>(key);
                    return;
                }
            }

            var current = _root;

            var father = current;

            while (current != null)
            {
                lock (father)
                {
                    if (current.Key.CompareTo(key) == 0)
                    {
                        lock (_inc) AreInserted++;
                        return;
                    }

                    father = current;

                    current = current.Key.CompareTo(key) > 0 ? current.LeftChild : current.RightChild;

                    if (current != null) continue;
                    if (key.CompareTo(father.Key) < 0)
                    {
                        father.LeftChild = new Node<TKey>(key) {Father = father};
                    }
                    else
                    {
                        father.RightChild = new Node<TKey>(key) {Father = father};
                    }

                    break;
                }
            }
        }


        public void ParallelRemove(TKey key)
        {
            var delNode = Find(key);

            do
            {

                if (delNode == null)
                {
                }

                else if (delNode.Father == null && delNode.LeftChild == null && delNode.RightChild == null)
                {

                    lock (delNode)
                    {
                        if (delNode.Father == null && delNode.LeftChild == null && delNode.RightChild == null
                            && Find(key) == delNode)
                        {
                            _root = null;
                            Interlocked.Increment(ref AreInserted);
                            return;
                        }
                    }
                }
                else if (delNode.Father != null && delNode.LeftChild == null && delNode.RightChild == null)
                {

                    lock (delNode.Father)
                    {
                        lock (delNode)
                        {
                            if (delNode.Father != null && delNode.LeftChild == null && delNode.RightChild == null &&
                                Find(key) == delNode)
                            {
                                if (delNode == delNode.Father.LeftChild) delNode.Father.LeftChild = null;

                                if (delNode == delNode.Father.RightChild) delNode.Father.RightChild = null;

                                Interlocked.Increment(ref AreDeleted);
                                return;
                            }

                        }
                    }

                }
                else if (delNode.Father == null && (delNode.LeftChild == null || delNode.RightChild == null))
                {
                    lock (delNode)
                    {
                        if (delNode.Father == null && (delNode.LeftChild == null || delNode.RightChild == null) &&
                            Find(key) == delNode)
                        {
                            _root = delNode.LeftChild ?? delNode.RightChild;
                            _root.Father = null;
                            Interlocked.Increment(ref AreDeleted);
                            return;
                        }
                    }
                }
                else if (delNode.Father != null && (delNode.RightChild == null || delNode.LeftChild == null))
                {

                    lock (delNode.Father)
                    {
                        lock (delNode)
                        {
                            if (delNode.Father != null &&
                                (delNode.RightChild == null && delNode.LeftChild != null ||
                                 delNode.LeftChild == null && delNode.RightChild != null) &&
                                Find(key) == delNode)
                            {
                                if (delNode.RightChild != null)
                                {
                                    delNode.RightChild.Father = delNode.Father;

                                    if (delNode.Father.LeftChild == delNode)
                                        delNode.Father.LeftChild = delNode.RightChild;

                                    else if (delNode.Father.RightChild == delNode)
                                        delNode.Father.RightChild = delNode.RightChild;
                                }
                                else if (delNode.LeftChild != null)
                                {
                                    delNode.LeftChild.Father = delNode.Father;

                                    if (delNode.Father.LeftChild == delNode)
                                        delNode.Father.LeftChild = delNode.LeftChild;

                                    else if (delNode.Father.RightChild == delNode)
                                        delNode.Father.RightChild = delNode.LeftChild;
                                }
                                Interlocked.Increment(ref AreDeleted);
                                return;
                            }
                        }
                    }

                }
                else if (delNode.RightChild != null && delNode.LeftChild != null)
                {

                    lock (delNode)
                    {
                        if (delNode.RightChild != null && delNode.LeftChild != null && Find(key) == delNode)
                        {
                            var minKeyNode = delNode.RightChild;
                            while (minKeyNode.LeftChild != null) minKeyNode = minKeyNode.LeftChild;
                            delNode.Key = minKeyNode.Key;
                            lock (minKeyNode.Father)
                            {
                                lock (minKeyNode)
                                {
                                    if (minKeyNode.Father.LeftChild == minKeyNode)
                                        minKeyNode.Father.LeftChild = minKeyNode.RightChild;

                                    else minKeyNode.Father.RightChild = minKeyNode.RightChild;

                                    if (minKeyNode.RightChild != null) minKeyNode.RightChild.Father = minKeyNode.Father;
                                    Interlocked.Increment(ref AreDeleted);
                                    return;
                                }
                            }
                        }
                    }

                }

                delNode = Find(key);

            } while (delNode != null);

            lock (_inc) Sim++;
        }

        public void Print()
        {
            Print(new List<Node<TKey>> {_root}, _root, 0);
            Console.WriteLine();
        }

        private static void Print(IList<Node<TKey>> listOfNodes, Node<TKey> node, int index)
        {
            int count;
            if (index < 0) count = listOfNodes.Count - 1;
            else count = index;

            listOfNodes.RemoveAt(0);
            count--;

            if (node != null)
            {
                listOfNodes.Add(node.LeftChild);
                listOfNodes.Add(node.RightChild);
            }


            if (node == null) Console.Write("null");
            else Console.Write(node.Key);
            Console.Write("   ");

            if (count < 0) Console.WriteLine();
            if (listOfNodes.Count > 0) Print(listOfNodes, listOfNodes[0], count);
        }

        public int Keys()
        {
            return Keys(_root);
        }

        private int Keys(Node<TKey> node)
        {
            if (node != null) return 1 + Keys(node.LeftChild) + Keys(node.RightChild);
            return 0;
        }
    }
}