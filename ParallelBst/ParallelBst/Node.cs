using System;

namespace ParallelBst
{
    public class Node<TKey> where TKey : IComparable<TKey>
    {
        
        public TKey Key { get; set; }
            
        public Node(TKey k)
        {
            Key = k;
        }

        public Node<TKey> Father = null;
        public Node<TKey> LeftChild = null;
        public Node<TKey> RightChild = null;

        
    }
}