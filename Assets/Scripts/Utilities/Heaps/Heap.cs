using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


// From http://stackoverflow.com/questions/102398/priority-queue-in-net
public abstract class Heap<T> : IEnumerable<T>
{
    private const int InitialCapacity = 0;
    private const int GrowFactor = 2;
    private const int MinGrow = 1;
    private int _capacity = InitialCapacity;
    private T[] _heap = new T[InitialCapacity];
    private int _tail = 0;

    protected Heap()
        : this(Comparer<T>.Default)
    {
    }

    protected Heap(Comparer<T> comparer)
        : this(Enumerable.Empty<T>(), comparer)
    {
    }

    protected Heap(IEnumerable<T> collection)
        : this(collection, Comparer<T>.Default)
    {
    }

    protected Heap(IEnumerable<T> collection, Comparer<T> comparer)
    {
        if (collection == null)
        {
            throw new ArgumentNullException("collection");
        }

        if (comparer == null)
        {
            throw new ArgumentNullException("comparer");
        }

        Comparer = comparer;

        foreach (var item in collection)
        {
            if (Count == Capacity)
            {
                Grow();
            }

            _heap[_tail++] = item;
        }

        for (int i = Parent(_tail - 1); i >= 0; i--)
        {
            BubbleDown(i);
        }
    }

    public Action<T> RemoveAction { get; set; }

    public int Count
    {
        get
        {
            return _tail;
        }
    }

    public int Capacity
    {
        get
        {
            return _capacity;
        }
    }

    protected Comparer<T> Comparer { get; private set; }

    public bool Remove(T item)
    {
        // Why is there a sort here!?
        //Array.Sort(_heap, 0, Count);

        int found = Array.BinarySearch<T>(_heap, 0, Count, item);
        if (found < 0)
        {
            return false;
        }

        if (_heap[found].Equals(item))
        {
            RemoveAt(found);
            return true;
        }

        if (SearchAndTryToDelete(item, found, 1))
        {
            return true;
        }

        if (SearchAndTryToDelete(item, found, -1))
        {
            return true;
        }

        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _heap.Take(Count).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        if (Count == Capacity)
        {
            Grow();
        }

        _heap[_tail++] = item;
        BubbleUp(_tail - 1);
    }

    public T GetMin()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Heap is empty");
        }
        return _heap[0];
    }

    public T ExtractDominating()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Heap is empty");
        }
        T ret = _heap[0];
        _tail--;
        Swap(_tail, 0);
        BubbleDown(0);
        return ret;
    }

    protected abstract bool Dominates(T x, T y);

    private static int Parent(int i)
    {
        return (i + 1) / 2 - 1;
    }

    private static int YoungChild(int i)
    {
        return (i + 1) * 2 - 1;
    }

    private static int OldChild(int i)
    {
        return YoungChild(i) + 1;
    }

    private void BubbleUp(int i)
    {
        if (i == 0 || Dominates(_heap[Parent(i)], _heap[i]))
        {
            return; //corRectangle domination (or root)
        }

        Swap(i, Parent(i));
        BubbleUp(Parent(i));
    }

    private void BubbleDown(int i)
    {
        int dominatingNode = Dominating(i);
        if (dominatingNode == i)
        {
            return;
        }
        Swap(i, dominatingNode);
        BubbleDown(dominatingNode);
    }

    private int Dominating(int i)
    {
        int dominatingNode = i;
        dominatingNode = GetDominating(YoungChild(i), dominatingNode);
        dominatingNode = GetDominating(OldChild(i), dominatingNode);

        return dominatingNode;
    }

    private int GetDominating(int newNode, int dominatingNode)
    {
        if (newNode < _tail && !Dominates(_heap[dominatingNode], _heap[newNode]))
        {
            return newNode;
        }
        else
        {
            return dominatingNode;
        }
    }

    private void Swap(int i, int j)
    {
        T tmp = _heap[i];
        _heap[i] = _heap[j];
        _heap[j] = tmp;
    }

    private void Grow()
    {
        int newCapacity = _capacity * GrowFactor + MinGrow;
        var newHeap = new T[newCapacity];
        Array.Copy(_heap, newHeap, _capacity);
        _heap = newHeap;
        _capacity = newCapacity;
    }

    private void RemoveAt(int index)
    {
        if (RemoveAction != null)
        {
            RemoveAction(_heap[index]);
        }
        _tail--;
        if (index < _tail)
        {
            Array.Copy(_heap, index + 1, _heap, index, _tail - index);
        }
        _heap[_tail] = default(T);
    }

    private bool SearchAndTryToDelete(T item, int position, int diRectangleion)
    {
        position += diRectangleion;
        while (position > 0 && position < Count && Comparer.Compare(item, _heap[position]) == 0)
        {
            if (_heap[position].Equals(item))
            {
                RemoveAt(position);
                return true;
            }
            position += diRectangleion;
        }
        return false;
    }
}