using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

namespace System;

//
// Resumen:
//     Proporciona métodos para la creación, manipulación, búsqueda y ordenación de
//     matrices, por lo tanto, sirve como clase base para todas las matrices de Common
//     Language Runtime.
[Serializable]
[ComVisible(true)]
[__DynamicallyInvokable]
public abstract class Array : ICloneable, IList, ICollection, IEnumerable, IStructuralComparable, IStructuralEquatable
{
    internal sealed class FunctorComparer<T> : IComparer<T>
    {
        private Comparison<T> comparison;

        public FunctorComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }

    private struct SorterObjectArray
    {
        private object[] keys;

        private object[] items;

        private IComparer comparer;

        internal SorterObjectArray(object[] keys, object[] items, IComparer comparer)
        {
            if (comparer == null)
            {
                comparer = Comparer.Default;
            }

            this.keys = keys;
            this.items = items;
            this.comparer = comparer;
        }

        internal void SwapIfGreaterWithItems(int a, int b)
        {
            if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
            {
                object obj = keys[a];
                keys[a] = keys[b];
                keys[b] = obj;
                if (items != null)
                {
                    object obj2 = items[a];
                    items[a] = items[b];
                    items[b] = obj2;
                }
            }
        }

        private void Swap(int i, int j)
        {
            object obj = keys[i];
            keys[i] = keys[j];
            keys[j] = obj;
            if (items != null)
            {
                object obj2 = items[i];
                items[i] = items[j];
                items[j] = obj2;
            }
        }

        internal void Sort(int left, int length)
        {
            if (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
            {
                IntrospectiveSort(left, length);
            }
            else
            {
                DepthLimitedQuickSort(left, length + left - 1, 32);
            }
        }

        private void DepthLimitedQuickSort(int left, int right, int depthLimit)
        {
            do
            {
                if (depthLimit == 0)
                {
                    try
                    {
                        Heapsort(left, right);
                        break;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", comparer));
                    }
                    catch (Exception innerException)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
                    }
                }

                int i = left;
                int num = right;
                int median = GetMedian(i, num);
                try
                {
                    SwapIfGreaterWithItems(i, median);
                    SwapIfGreaterWithItems(i, num);
                    SwapIfGreaterWithItems(median, num);
                }
                catch (Exception innerException2)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException2);
                }

                object obj = keys[median];
                do
                {
                    try
                    {
                        for (; comparer.Compare(keys[i], obj) < 0; i++)
                        {
                        }

                        while (comparer.Compare(obj, keys[num]) < 0)
                        {
                            num--;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", comparer));
                    }
                    catch (Exception innerException3)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException3);
                    }

                    if (i > num)
                    {
                        break;
                    }

                    if (i < num)
                    {
                        object obj2 = keys[i];
                        keys[i] = keys[num];
                        keys[num] = obj2;
                        if (items != null)
                        {
                            object obj3 = items[i];
                            items[i] = items[num];
                            items[num] = obj3;
                        }
                    }

                    i++;
                    num--;
                }
                while (i <= num);
                depthLimit--;
                if (num - left <= right - i)
                {
                    if (left < num)
                    {
                        DepthLimitedQuickSort(left, num, depthLimit);
                    }

                    left = i;
                }
                else
                {
                    if (i < right)
                    {
                        DepthLimitedQuickSort(i, right, depthLimit);
                    }

                    right = num;
                }
            }
            while (left < right);
        }

        private void IntrospectiveSort(int left, int length)
        {
            if (length < 2)
            {
                return;
            }

            try
            {
                IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2(keys.Length));
            }
            catch (IndexOutOfRangeException)
            {
                IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
            }
            catch (Exception innerException)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
            }
        }

        private void IntroSort(int lo, int hi, int depthLimit)
        {
            while (hi > lo)
            {
                int num = hi - lo + 1;
                if (num <= 16)
                {
                    switch (num)
                    {
                        case 1:
                            break;
                        case 2:
                            SwapIfGreaterWithItems(lo, hi);
                            break;
                        case 3:
                            SwapIfGreaterWithItems(lo, hi - 1);
                            SwapIfGreaterWithItems(lo, hi);
                            SwapIfGreaterWithItems(hi - 1, hi);
                            break;
                        default:
                            InsertionSort(lo, hi);
                            break;
                    }

                    break;
                }

                if (depthLimit == 0)
                {
                    Heapsort(lo, hi);
                    break;
                }

                depthLimit--;
                int num2 = PickPivotAndPartition(lo, hi);
                IntroSort(num2 + 1, hi, depthLimit);
                hi = num2 - 1;
            }
        }

        private int PickPivotAndPartition(int lo, int hi)
        {
            int num = lo + (hi - lo) / 2;
            SwapIfGreaterWithItems(lo, num);
            SwapIfGreaterWithItems(lo, hi);
            SwapIfGreaterWithItems(num, hi);
            object obj = keys[num];
            Swap(num, hi - 1);
            int num2 = lo;
            int num3 = hi - 1;
            while (num2 < num3)
            {
                while (comparer.Compare(keys[++num2], obj) < 0)
                {
                }

                while (comparer.Compare(obj, keys[--num3]) < 0)
                {
                }

                if (num2 >= num3)
                {
                    break;
                }

                Swap(num2, num3);
            }

            Swap(num2, hi - 1);
            return num2;
        }

        private void Heapsort(int lo, int hi)
        {
            int num = hi - lo + 1;
            for (int num2 = num / 2; num2 >= 1; num2--)
            {
                DownHeap(num2, num, lo);
            }

            for (int num3 = num; num3 > 1; num3--)
            {
                Swap(lo, lo + num3 - 1);
                DownHeap(1, num3 - 1, lo);
            }
        }

        private void DownHeap(int i, int n, int lo)
        {
            object obj = keys[lo + i - 1];
            object obj2 = ((items != null) ? items[lo + i - 1] : null);
            while (i <= n / 2)
            {
                int num = 2 * i;
                if (num < n && comparer.Compare(keys[lo + num - 1], keys[lo + num]) < 0)
                {
                    num++;
                }

                if (comparer.Compare(obj, keys[lo + num - 1]) >= 0)
                {
                    break;
                }

                keys[lo + i - 1] = keys[lo + num - 1];
                if (items != null)
                {
                    items[lo + i - 1] = items[lo + num - 1];
                }

                i = num;
            }

            keys[lo + i - 1] = obj;
            if (items != null)
            {
                items[lo + i - 1] = obj2;
            }
        }

        private void InsertionSort(int lo, int hi)
        {
            for (int i = lo; i < hi; i++)
            {
                int num = i;
                object obj = keys[i + 1];
                object obj2 = ((items != null) ? items[i + 1] : null);
                while (num >= lo && comparer.Compare(obj, keys[num]) < 0)
                {
                    keys[num + 1] = keys[num];
                    if (items != null)
                    {
                        items[num + 1] = items[num];
                    }

                    num--;
                }

                keys[num + 1] = obj;
                if (items != null)
                {
                    items[num + 1] = obj2;
                }
            }
        }
    }

    private struct SorterGenericArray
    {
        private Array keys;

        private Array items;

        private IComparer comparer;

        internal SorterGenericArray(Array keys, Array items, IComparer comparer)
        {
            if (comparer == null)
            {
                comparer = Comparer.Default;
            }

            this.keys = keys;
            this.items = items;
            this.comparer = comparer;
        }

        internal void SwapIfGreaterWithItems(int a, int b)
        {
            if (a != b && comparer.Compare(keys.GetValue(a), keys.GetValue(b)) > 0)
            {
                object value = keys.GetValue(a);
                keys.SetValue(keys.GetValue(b), a);
                keys.SetValue(value, b);
                if (items != null)
                {
                    object value2 = items.GetValue(a);
                    items.SetValue(items.GetValue(b), a);
                    items.SetValue(value2, b);
                }
            }
        }

        private void Swap(int i, int j)
        {
            object value = keys.GetValue(i);
            keys.SetValue(keys.GetValue(j), i);
            keys.SetValue(value, j);
            if (items != null)
            {
                object value2 = items.GetValue(i);
                items.SetValue(items.GetValue(j), i);
                items.SetValue(value2, j);
            }
        }

        internal void Sort(int left, int length)
        {
            if (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5)
            {
                IntrospectiveSort(left, length);
            }
            else
            {
                DepthLimitedQuickSort(left, length + left - 1, 32);
            }
        }

        private void DepthLimitedQuickSort(int left, int right, int depthLimit)
        {
            do
            {
                if (depthLimit == 0)
                {
                    try
                    {
                        Heapsort(left, right);
                        break;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", comparer));
                    }
                    catch (Exception innerException)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
                    }
                }

                int i = left;
                int num = right;
                int median = GetMedian(i, num);
                try
                {
                    SwapIfGreaterWithItems(i, median);
                    SwapIfGreaterWithItems(i, num);
                    SwapIfGreaterWithItems(median, num);
                }
                catch (Exception innerException2)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException2);
                }

                object value = keys.GetValue(median);
                do
                {
                    try
                    {
                        for (; comparer.Compare(keys.GetValue(i), value) < 0; i++)
                        {
                        }

                        while (comparer.Compare(value, keys.GetValue(num)) < 0)
                        {
                            num--;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", comparer));
                    }
                    catch (Exception innerException3)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException3);
                    }

                    if (i > num)
                    {
                        break;
                    }

                    if (i < num)
                    {
                        object value2 = keys.GetValue(i);
                        keys.SetValue(keys.GetValue(num), i);
                        keys.SetValue(value2, num);
                        if (items != null)
                        {
                            object value3 = items.GetValue(i);
                            items.SetValue(items.GetValue(num), i);
                            items.SetValue(value3, num);
                        }
                    }

                    if (i != int.MaxValue)
                    {
                        i++;
                    }

                    if (num != int.MinValue)
                    {
                        num--;
                    }
                }
                while (i <= num);
                depthLimit--;
                if (num - left <= right - i)
                {
                    if (left < num)
                    {
                        DepthLimitedQuickSort(left, num, depthLimit);
                    }

                    left = i;
                }
                else
                {
                    if (i < right)
                    {
                        DepthLimitedQuickSort(i, right, depthLimit);
                    }

                    right = num;
                }
            }
            while (left < right);
        }

        private void IntrospectiveSort(int left, int length)
        {
            if (length < 2)
            {
                return;
            }

            try
            {
                IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2(keys.Length));
            }
            catch (IndexOutOfRangeException)
            {
                IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
            }
            catch (Exception innerException)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
            }
        }

        private void IntroSort(int lo, int hi, int depthLimit)
        {
            while (hi > lo)
            {
                int num = hi - lo + 1;
                if (num <= 16)
                {
                    switch (num)
                    {
                        case 1:
                            break;
                        case 2:
                            SwapIfGreaterWithItems(lo, hi);
                            break;
                        case 3:
                            SwapIfGreaterWithItems(lo, hi - 1);
                            SwapIfGreaterWithItems(lo, hi);
                            SwapIfGreaterWithItems(hi - 1, hi);
                            break;
                        default:
                            InsertionSort(lo, hi);
                            break;
                    }

                    break;
                }

                if (depthLimit == 0)
                {
                    Heapsort(lo, hi);
                    break;
                }

                depthLimit--;
                int num2 = PickPivotAndPartition(lo, hi);
                IntroSort(num2 + 1, hi, depthLimit);
                hi = num2 - 1;
            }
        }

        private int PickPivotAndPartition(int lo, int hi)
        {
            int num = lo + (hi - lo) / 2;
            SwapIfGreaterWithItems(lo, num);
            SwapIfGreaterWithItems(lo, hi);
            SwapIfGreaterWithItems(num, hi);
            object value = keys.GetValue(num);
            Swap(num, hi - 1);
            int num2 = lo;
            int num3 = hi - 1;
            while (num2 < num3)
            {
                while (comparer.Compare(keys.GetValue(++num2), value) < 0)
                {
                }

                while (comparer.Compare(value, keys.GetValue(--num3)) < 0)
                {
                }

                if (num2 >= num3)
                {
                    break;
                }

                Swap(num2, num3);
            }

            Swap(num2, hi - 1);
            return num2;
        }

        private void Heapsort(int lo, int hi)
        {
            int num = hi - lo + 1;
            for (int num2 = num / 2; num2 >= 1; num2--)
            {
                DownHeap(num2, num, lo);
            }

            for (int num3 = num; num3 > 1; num3--)
            {
                Swap(lo, lo + num3 - 1);
                DownHeap(1, num3 - 1, lo);
            }
        }

        private void DownHeap(int i, int n, int lo)
        {
            object value = keys.GetValue(lo + i - 1);
            object value2 = ((items != null) ? items.GetValue(lo + i - 1) : null);
            while (i <= n / 2)
            {
                int num = 2 * i;
                if (num < n && comparer.Compare(keys.GetValue(lo + num - 1), keys.GetValue(lo + num)) < 0)
                {
                    num++;
                }

                if (comparer.Compare(value, keys.GetValue(lo + num - 1)) >= 0)
                {
                    break;
                }

                keys.SetValue(keys.GetValue(lo + num - 1), lo + i - 1);
                if (items != null)
                {
                    items.SetValue(items.GetValue(lo + num - 1), lo + i - 1);
                }

                i = num;
            }

            keys.SetValue(value, lo + i - 1);
            if (items != null)
            {
                items.SetValue(value2, lo + i - 1);
            }
        }

        private void InsertionSort(int lo, int hi)
        {
            for (int i = lo; i < hi; i++)
            {
                int num = i;
                object value = keys.GetValue(i + 1);
                object value2 = ((items != null) ? items.GetValue(i + 1) : null);
                while (num >= lo && comparer.Compare(value, keys.GetValue(num)) < 0)
                {
                    keys.SetValue(keys.GetValue(num), num + 1);
                    if (items != null)
                    {
                        items.SetValue(items.GetValue(num), num + 1);
                    }

                    num--;
                }

                keys.SetValue(value, num + 1);
                if (items != null)
                {
                    items.SetValue(value2, num + 1);
                }
            }
        }
    }

    [Serializable]
    private sealed class SZArrayEnumerator : IEnumerator, ICloneable
    {
        private Array _array;

        private int _index;

        private int _endIndex;

        public object Current
        {
            get
            {
                if (_index < 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                }

                if (_index >= _endIndex)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                }

                return _array.GetValue(_index);
            }
        }

        internal SZArrayEnumerator(Array array)
        {
            _array = array;
            _index = -1;
            _endIndex = array.Length;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool MoveNext()
        {
            if (_index < _endIndex)
            {
                _index++;
                return _index < _endIndex;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }
    }

    [Serializable]
    private sealed class ArrayEnumerator : IEnumerator, ICloneable
    {
        private Array array;

        private int index;

        private int endIndex;

        private int startIndex;

        private int[] _indices;

        private bool _complete;

        public object Current
        {
            get
            {
                if (index < startIndex)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                }

                if (_complete)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                }

                return array.GetValue(_indices);
            }
        }

        internal ArrayEnumerator(Array array, int index, int count)
        {
            this.array = array;
            this.index = index - 1;
            startIndex = index;
            endIndex = index + count;
            _indices = new int[array.Rank];
            int num = 1;
            for (int i = 0; i < array.Rank; i++)
            {
                _indices[i] = array.GetLowerBound(i);
                num *= array.GetLength(i);
            }

            _indices[_indices.Length - 1]--;
            _complete = num == 0;
        }

        private void IncArray()
        {
            int rank = array.Rank;
            _indices[rank - 1]++;
            for (int num = rank - 1; num >= 0; num--)
            {
                if (_indices[num] > array.GetUpperBound(num))
                {
                    if (num == 0)
                    {
                        _complete = true;
                        break;
                    }

                    for (int i = num; i < rank; i++)
                    {
                        _indices[i] = array.GetLowerBound(i);
                    }

                    _indices[num - 1]++;
                }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool MoveNext()
        {
            if (_complete)
            {
                index = endIndex;
                return false;
            }

            index++;
            IncArray();
            return !_complete;
        }

        public void Reset()
        {
            index = startIndex - 1;
            int num = 1;
            for (int i = 0; i < array.Rank; i++)
            {
                _indices[i] = array.GetLowerBound(i);
                num *= array.GetLength(i);
            }

            _complete = num == 0;
            _indices[_indices.Length - 1]--;
        }
    }

    internal const int MaxArrayLength = 2146435071;

    internal const int MaxByteArrayLength = 2147483591;

    //
    // Resumen:
    //     Obtiene el número total de elementos de todas las dimensiones de System.Array.
    //
    //
    // Devuelve:
    //     Número total de elementos en todas las dimensiones de System.Array; es cero si
    //     no hay elementos en la matriz.
    //
    // Excepciones:
    //   T:System.OverflowException:
    //     La matriz es multidimensional y contiene más de System.Int32.MaxValue elementos.
    [__DynamicallyInvokable]
    public extern int Length
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [__DynamicallyInvokable]
        get;
    }

    //
    // Resumen:
    //     Obtiene un entero de 64 bits que representa el número total de elementos de todas
    //     las dimensiones de System.Array.
    //
    // Devuelve:
    //     Entero de 64 bits que representa el número total de elementos de todas las dimensiones
    //     de la System.Array.
    [ComVisible(false)]
    public extern long LongLength
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        get;
    }

    //
    // Resumen:
    //     Obtiene el rango (número de dimensiones) de System.Array. Por ejemplo, una matriz
    //     unidimensional devuelve 1, una matriz bidimensional devuelve 2, y así sucesivamente.
    //
    //
    // Devuelve:
    //     Rango (número de dimensiones) de System.Array.
    [__DynamicallyInvokable]
    public extern int Rank
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [__DynamicallyInvokable]
        get;
    }

    //
    // Resumen:
    //     Obtiene el número de elementos incluidos en System.Array.
    //
    // Devuelve:
    //     Número de elementos contenidos en la colección.
    [__DynamicallyInvokable]
    int ICollection.Count
    {
        [__DynamicallyInvokable]
        get
        {
            return Length;
        }
    }

    //
    // Resumen:
    //     Obtiene un objeto que se puede usar para sincronizar el acceso a System.Array.
    //
    //
    // Devuelve:
    //     Objeto que se puede usar para sincronizar el acceso a System.Array.
    public object SyncRoot => this;

    //
    // Resumen:
    //     Obtiene un valor que indica si System.Array es de solo lectura.
    //
    // Devuelve:
    //     Esta propiedad es siempre false para todas las matrices.
    public bool IsReadOnly => false;

    //
    // Resumen:
    //     Obtiene un valor que indica si la interfaz System.Array tiene un tamaño fijo.
    //
    //
    // Devuelve:
    //     Esta propiedad es siempre true para todas las matrices.
    public bool IsFixedSize => true;

    //
    // Resumen:
    //     Obtiene un valor que indica si el acceso a la interfaz System.Array está sincronizado
    //     (es seguro para subprocesos).
    //
    // Devuelve:
    //     Esta propiedad es siempre false para todas las matrices.
    public bool IsSynchronized => false;

    //
    // Resumen:
    //     Obtiene o establece el elemento en el índice especificado.
    //
    // Parámetros:
    //   index:
    //     Índice del elemento que se va a obtener o establecer.
    //
    // Devuelve:
    //     Elemento en el índice especificado.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que cero. O bien index es igual o mayor que System.Collections.ICollection.Count.
    //
    //
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente una dimensión.
    [__DynamicallyInvokable]
    object IList.this[int index]
    {
        [__DynamicallyInvokable]
        get
        {
            return GetValue(index);
        }
        [__DynamicallyInvokable]
        set
        {
            SetValue(value, index);
        }
    }

    internal Array()
    {
    }

    //
    // Resumen:
    //     Devuelve un contenedor de solo lectura para la matriz especificada.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional de base cero para ajustar en un contenedor System.Collections.ObjectModel.ReadOnlyCollection`1
    //     de solo lectura.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Contenedor System.Collections.ObjectModel.ReadOnlyCollection`1 de solo lectura
    //     para la matriz especificada.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    public static ReadOnlyCollection<T> AsReadOnly<T>(T[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return new ReadOnlyCollection<T>(array);
    }

    //
    // Resumen:
    //     Cambia el número de elementos de una matriz unidimensional al nuevo tamaño especificado.
    //
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional de base cero a la que se va a cambiar el tamaño o null
    //     para crear una nueva matriz con el tamaño especificado.
    //
    //   newSize:
    //     Tamaño de la nueva matriz.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     newSize es menor que cero.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Resize<T>(ref T[] array, int newSize)
    {
        if (newSize < 0)
        {
            throw new ArgumentOutOfRangeException("newSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        T[] array2 = array;
        if (array2 == null)
        {
            array = new T[newSize];
        }
        else if (array2.Length != newSize)
        {
            T[] array3 = new T[newSize];
            Copy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
            array = array3;
        }
    }

    //
    // Resumen:
    //     Crea una matriz System.Array unidimensional de la longitud y el System.Type especificados,
    //     con una indización de base cero.
    //
    // Parámetros:
    //   elementType:
    //     System.Type de la matriz System.Array que se va a crear.
    //
    //   length:
    //     Tamaño de la matriz System.Array que se va a crear.
    //
    // Devuelve:
    //     Nueva matriz System.Array unidimensional de la longitud y el System.Type especificados,
    //     usando una indización de base cero.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     elementType es null.
    //
    //   T:System.ArgumentException:
    //     elementType no es un System.Type válido.
    //
    //   T:System.NotSupportedException:
    //     No se admite elementType. Por ejemplo, no se admite System.Void. O bien elementType
    //     es un tipo genérico abierto.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     length es menor que cero.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe static Array CreateInstance(Type elementType, int length)
    {
        if ((object)elementType == null)
        {
            throw new ArgumentNullException("elementType");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
        if (runtimeType == null)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
        }

        return InternalCreate((void*)runtimeType.TypeHandle.Value, 1, &length, null);
    }

    //
    // Resumen:
    //     Crea una matriz System.Array bidimensional de las longitudes de dimensión y el
    //     System.Type especificados, con una indización de base cero.
    //
    // Parámetros:
    //   elementType:
    //     System.Type de la matriz System.Array que se va a crear.
    //
    //   length1:
    //     Tamaño de la primera dimensión de la System.Array que se va a crear.
    //
    //   length2:
    //     Tamaño de la segunda dimensión de la System.Array que se va a crear.
    //
    // Devuelve:
    //     Nueva matriz System.Array bidimensional del tipo System.Type especificado con
    //     la longitud especificada para cada dimensión, usando una indización de base cero.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     elementType es null.
    //
    //   T:System.ArgumentException:
    //     elementType no es un System.Type válido.
    //
    //   T:System.NotSupportedException:
    //     No se admite elementType. Por ejemplo, no se admite System.Void. O bien elementType
    //     es un tipo genérico abierto.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     length1 es menor que cero. O bien length2 es menor que cero.
    [SecuritySafeCritical]
    public unsafe static Array CreateInstance(Type elementType, int length1, int length2)
    {
        if ((object)elementType == null)
        {
            throw new ArgumentNullException("elementType");
        }

        if (length1 < 0 || length2 < 0)
        {
            throw new ArgumentOutOfRangeException((length1 < 0) ? "length1" : "length2", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
        if (runtimeType == null)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
        }

        int* ptr = stackalloc int[2];
        *ptr = length1;
        ptr[1] = length2;
        return InternalCreate((void*)runtimeType.TypeHandle.Value, 2, ptr, null);
    }

    //
    // Resumen:
    //     Crea una System.Array tridimensional de las longitudes de dimensión y el System.Type
    //     especificados, con una indización de base cero.
    //
    // Parámetros:
    //   elementType:
    //     System.Type de la matriz System.Array que se va a crear.
    //
    //   length1:
    //     Tamaño de la primera dimensión de la System.Array que se va a crear.
    //
    //   length2:
    //     Tamaño de la segunda dimensión de la matriz System.Array que se va a crear.
    //
    //   length3:
    //     Tamaño de la tercera dimensión de la matriz System.Array que se va a crear.
    //
    // Devuelve:
    //     Nueva matriz System.Array tridimensional del tipo System.Type especificado con
    //     la longitud especificada para cada dimensión, usando una indización de base cero.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     elementType es null.
    //
    //   T:System.ArgumentException:
    //     elementType no es un System.Type válido.
    //
    //   T:System.NotSupportedException:
    //     No se admite elementType. Por ejemplo, no se admite System.Void. O bien elementType
    //     es un tipo genérico abierto.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     length1 es menor que cero. O bien length2 es menor que cero. O bien length3 es
    //     menor que cero.
    [SecuritySafeCritical]
    public unsafe static Array CreateInstance(Type elementType, int length1, int length2, int length3)
    {
        if ((object)elementType == null)
        {
            throw new ArgumentNullException("elementType");
        }

        if (length1 < 0)
        {
            throw new ArgumentOutOfRangeException("length1", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (length2 < 0)
        {
            throw new ArgumentOutOfRangeException("length2", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (length3 < 0)
        {
            throw new ArgumentOutOfRangeException("length3", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
        if (runtimeType == null)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
        }

        int* ptr = stackalloc int[3];
        *ptr = length1;
        ptr[1] = length2;
        ptr[2] = length3;
        return InternalCreate((void*)runtimeType.TypeHandle.Value, 3, ptr, null);
    }

    //
    // Resumen:
    //     Crea una System.Array multidimensional de las longitudes de dimensión y el tipo
    //     System.Type especificados, con una indización de base cero. Las longitudes de
    //     dimensión se especifican en una matriz de enteros de 32 bits.
    //
    // Parámetros:
    //   elementType:
    //     System.Type de la matriz System.Array que se va a crear.
    //
    //   lengths:
    //     Matriz de enteros de 32 bits que representa el tamaño de cada una de las dimensiones
    //     de System.Array que se van a crear.
    //
    // Devuelve:
    //     Nueva matriz System.Array multidimensional del tipo System.Type especificado
    //     con la longitud especificada para cada dimensión, usando una indización de base
    //     cero.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     elementType es null. O bien lengths es null.
    //
    //   T:System.ArgumentException:
    //     elementType no es un System.Type válido. O bien La matriz lengths contiene menos
    //     de un elemento.
    //
    //   T:System.NotSupportedException:
    //     No se admite elementType. Por ejemplo, no se admite System.Void. O bien elementType
    //     es un tipo genérico abierto.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     Cualquier valor en lengths es menor que cero.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe static Array CreateInstance(Type elementType, params int[] lengths)
    {
        if ((object)elementType == null)
        {
            throw new ArgumentNullException("elementType");
        }

        if (lengths == null)
        {
            throw new ArgumentNullException("lengths");
        }

        if (lengths.Length == 0)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
        }

        RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
        if (runtimeType == null)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
        }

        for (int i = 0; i < lengths.Length; i++)
        {
            if (lengths[i] < 0)
            {
                throw new ArgumentOutOfRangeException("lengths[" + i + "]", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
        }

        fixed (int* pLengths = lengths)
        {
            return InternalCreate((void*)runtimeType.TypeHandle.Value, lengths.Length, pLengths, null);
        }
    }

    //
    // Resumen:
    //     Crea una System.Array multidimensional de las longitudes de dimensión y el tipo
    //     System.Type especificados, con una indización de base cero. Las longitudes de
    //     dimensión se especifican en una matriz de enteros de 64 bits.
    //
    // Parámetros:
    //   elementType:
    //     System.Type de la matriz System.Array que se va a crear.
    //
    //   lengths:
    //     Matriz de enteros de 64 bits que representa el tamaño de cada una de las dimensiones
    //     de System.Array que se van a crear. Cada entero de la matriz debe estar entre
    //     cero y System.Int32.MaxValue, ambos incluidos.
    //
    // Devuelve:
    //     Nueva matriz System.Array multidimensional del tipo System.Type especificado
    //     con la longitud especificada para cada dimensión, usando una indización de base
    //     cero.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     elementType es null. O bien lengths es null.
    //
    //   T:System.ArgumentException:
    //     elementType no es un System.Type válido. O bien La matriz lengths contiene menos
    //     de un elemento.
    //
    //   T:System.NotSupportedException:
    //     No se admite elementType. Por ejemplo, no se admite System.Void. O bien elementType
    //     es un tipo genérico abierto.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     Cualquier valor de lengths es menor que cero o mayor que System.Int32.MaxValue.
    public static Array CreateInstance(Type elementType, params long[] lengths)
    {
        if (lengths == null)
        {
            throw new ArgumentNullException("lengths");
        }

        if (lengths.Length == 0)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
        }

        int[] array = new int[lengths.Length];
        for (int i = 0; i < lengths.Length; i++)
        {
            long num = lengths[i];
            if (num > int.MaxValue || num < int.MinValue)
            {
                throw new ArgumentOutOfRangeException("len", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }

            array[i] = (int)num;
        }

        return CreateInstance(elementType, array);
    }

    //
    // Resumen:
    //     Crea una matriz System.Array multidimensional de las longitudes de dimensión
    //     y el tipo System.Type especificados, con los límites inferiores especificados.
    //
    //
    // Parámetros:
    //   elementType:
    //     System.Type de la matriz System.Array que se va a crear.
    //
    //   lengths:
    //     Matriz unidimensional que contiene el tamaño de cada una de las dimensiones de
    //     la matriz System.Array que se va a crear.
    //
    //   lowerBounds:
    //     Matriz unidimensional que contiene el límite inferior (índice inicial) de cada
    //     una de las dimensiones de la matriz System.Array que se va a crear.
    //
    // Devuelve:
    //     Nueva matriz System.Array multidimensional del tipo System.Type especificado
    //     con la longitud y los límites inferiores especificados para cada dimensión.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     elementType es null. O bien lengths es null. O bien lowerBounds es null.
    //
    //   T:System.ArgumentException:
    //     elementType no es un System.Type válido. O bien La matriz lengths contiene menos
    //     de un elemento. O bien Las matrices lengths y lowerBounds no contienen el mismo
    //     número de elementos.
    //
    //   T:System.NotSupportedException:
    //     No se admite elementType. Por ejemplo, no se admite System.Void. O bien elementType
    //     es un tipo genérico abierto.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     Cualquier valor en lengths es menor que cero. O bien Cualquier valor de lowerBounds
    //     es demasiado grande, tanto que la suma del límite inferior y de la longitud de
    //     una dimensión es mayor que System.Int32.MaxValue.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe static Array CreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
    {
        if (elementType == null)
        {
            throw new ArgumentNullException("elementType");
        }

        if (lengths == null)
        {
            throw new ArgumentNullException("lengths");
        }

        if (lowerBounds == null)
        {
            throw new ArgumentNullException("lowerBounds");
        }

        if (lengths.Length != lowerBounds.Length)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_RanksAndBounds"));
        }

        if (lengths.Length == 0)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
        }

        RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
        if (runtimeType == null)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
        }

        for (int i = 0; i < lengths.Length; i++)
        {
            if (lengths[i] < 0)
            {
                throw new ArgumentOutOfRangeException("lengths[" + i + "]", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
        }

        fixed (int* pLengths = lengths)
        {
            fixed (int* pLowerBounds = lowerBounds)
            {
                return InternalCreate((void*)runtimeType.TypeHandle.Value, lengths.Length, pLengths, pLowerBounds);
            }
        }
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    private unsafe static extern Array InternalCreate(void* elementType, int rank, int* pLengths, int* pLowerBounds);

    [SecurityCritical]
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal static Array UnsafeCreateInstance(Type elementType, int length)
    {
        return CreateInstance(elementType, length);
    }

    [SecurityCritical]
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal static Array UnsafeCreateInstance(Type elementType, int length1, int length2)
    {
        return CreateInstance(elementType, length1, length2);
    }

    [SecurityCritical]
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal static Array UnsafeCreateInstance(Type elementType, params int[] lengths)
    {
        return CreateInstance(elementType, lengths);
    }

    [SecurityCritical]
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal static Array UnsafeCreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
    {
        return CreateInstance(elementType, lengths, lowerBounds);
    }

    //
    // Resumen:
    //     Copia un intervalo de elementos de una matriz System.Array comenzando en el primer
    //     elemento y los pega en otra matriz System.Array comenzando en el primer elemento.
    //     La longitud se especifica como un entero de 32 bits.
    //
    // Parámetros:
    //   sourceArray:
    //     System.Array que contiene los datos que se van a copiar.
    //
    //   destinationArray:
    //     System.Array que recibe los datos.
    //
    //   length:
    //     Entero de 32 bits que representa el número de elementos que se van a copiar.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     sourceArray es null. O bien destinationArray es null.
    //
    //   T:System.RankException:
    //     sourceArray y destinationArray tienen clasificaciones diferentes.
    //
    //   T:System.ArrayTypeMismatchException:
    //     sourceArray y destinationArray son de tipos incompatibles.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de la sourceArray no se puede convertir al tipo de destinationArray.
    //
    //
    //   T:System.ArgumentOutOfRangeException:
    //     length es menor que cero.
    //
    //   T:System.ArgumentException:
    //     length es mayor que el número de elementos de sourceArray. O bien length es mayor
    //     que el número de elementos de destinationArray.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Copy(Array sourceArray, Array destinationArray, int length)
    {
        if (sourceArray == null)
        {
            throw new ArgumentNullException("sourceArray");
        }

        if (destinationArray == null)
        {
            throw new ArgumentNullException("destinationArray");
        }

        Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length, reliable: false);
    }

    //
    // Resumen:
    //     Copia un intervalo de elementos de un objeto System.Array a partir del índice
    //     de origen especificado y los pega en otro objeto System.Array a partir del índice
    //     de destino especificado. La longitud y los índices se especifican como enteros
    //     de 32 bits.
    //
    // Parámetros:
    //   sourceArray:
    //     System.Array que contiene los datos que se van a copiar.
    //
    //   sourceIndex:
    //     Entero de 32 bits que representa el índice de la sourceArray en la que se empieza
    //     a copiar.
    //
    //   destinationArray:
    //     System.Array que recibe los datos.
    //
    //   destinationIndex:
    //     Entero de 32 bits que representa el índice de la destinationArray en la que se
    //     empieza a almacenar.
    //
    //   length:
    //     Entero de 32 bits que representa el número de elementos que se van a copiar.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     sourceArray es null. O bien destinationArray es null.
    //
    //   T:System.RankException:
    //     sourceArray y destinationArray tienen clasificaciones diferentes.
    //
    //   T:System.ArrayTypeMismatchException:
    //     sourceArray y destinationArray son de tipos incompatibles.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de sourceArray no se puede convertir al tipo de destinationArray.
    //
    //
    //   T:System.ArgumentOutOfRangeException:
    //     sourceIndex es menor que el límite inferior de la primera dimensión de sourceArray.
    //     O bien destinationIndex es menor que el límite inferior de la primera dimensión
    //     de destinationArray. O bien length es menor que cero.
    //
    //   T:System.ArgumentException:
    //     length es mayor que el número de elementos desde sourceIndex hasta el final de
    //     sourceArray. O bien length es mayor que el número de elementos desde destinationIndex
    //     hasta el final de destinationArray.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
    {
        Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, reliable: false);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    internal static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable);

    //
    // Resumen:
    //     Copia un intervalo de elementos de un objeto System.Array a partir del índice
    //     de origen especificado y los pega en otro objeto System.Array a partir del índice
    //     de destino especificado. Garantiza que se deshacen todos los cambios si la copia
    //     no se realiza de forma totalmente correcta.
    //
    // Parámetros:
    //   sourceArray:
    //     System.Array que contiene los datos que se van a copiar.
    //
    //   sourceIndex:
    //     Entero de 32 bits que representa el índice de la sourceArray en la que se empieza
    //     a copiar.
    //
    //   destinationArray:
    //     System.Array que recibe los datos.
    //
    //   destinationIndex:
    //     Entero de 32 bits que representa el índice de la destinationArray en la que se
    //     empieza a almacenar.
    //
    //   length:
    //     Entero de 32 bits que representa el número de elementos que se van a copiar.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     sourceArray es null. O bien destinationArray es null.
    //
    //   T:System.RankException:
    //     sourceArray y destinationArray tienen clasificaciones diferentes.
    //
    //   T:System.ArrayTypeMismatchException:
    //     El tipo sourceArray no es el mismo ni se deriva del tipo destinationArray.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de sourceArray no se puede convertir al tipo de destinationArray.
    //
    //
    //   T:System.ArgumentOutOfRangeException:
    //     sourceIndex es menor que el límite inferior de la primera dimensión de sourceArray.
    //     O bien destinationIndex es menor que el límite inferior de la primera dimensión
    //     de destinationArray. O bien length es menor que cero.
    //
    //   T:System.ArgumentException:
    //     length es mayor que el número de elementos desde sourceIndex hasta el final de
    //     sourceArray. O bien length es mayor que el número de elementos desde destinationIndex
    //     hasta el final de destinationArray.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [__DynamicallyInvokable]
    public static void ConstrainedCopy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
    {
        Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, reliable: true);
    }

    //
    // Resumen:
    //     Copia un intervalo de elementos de una matriz System.Array comenzando en el primer
    //     elemento y los pega en otra matriz System.Array comenzando en el primer elemento.
    //     La longitud se especifica como un entero de 64 bits.
    //
    // Parámetros:
    //   sourceArray:
    //     System.Array que contiene los datos que se van a copiar.
    //
    //   destinationArray:
    //     System.Array que recibe los datos.
    //
    //   length:
    //     Entero de 64 bits que representa el número de elementos que se van a copiar.
    //     El entero debe estar entre cero y System.Int32.MaxValue, ambos inclusive.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     sourceArray es null. O bien destinationArray es null.
    //
    //   T:System.RankException:
    //     sourceArray y destinationArray tienen clasificaciones diferentes.
    //
    //   T:System.ArrayTypeMismatchException:
    //     sourceArray y destinationArray son de tipos incompatibles.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de sourceArray no se puede convertir al tipo de destinationArray.
    //
    //
    //   T:System.ArgumentOutOfRangeException:
    //     length es menor que 0 o mayor que System.Int32.MaxValue.
    //
    //   T:System.ArgumentException:
    //     length es mayor que el número de elementos de sourceArray. O bien length es mayor
    //     que el número de elementos de destinationArray.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    public static void Copy(Array sourceArray, Array destinationArray, long length)
    {
        if (length > int.MaxValue || length < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        Copy(sourceArray, destinationArray, (int)length);
    }

    //
    // Resumen:
    //     Copia un intervalo de elementos de un objeto System.Array a partir del índice
    //     de origen especificado y los pega en otro objeto System.Array a partir del índice
    //     de destino especificado. La longitud y los índices se especifican como enteros
    //     de 64 bits.
    //
    // Parámetros:
    //   sourceArray:
    //     System.Array que contiene los datos que se van a copiar.
    //
    //   sourceIndex:
    //     Entero de 64 bits que representa el índice de sourceArray donde comienza la copia.
    //
    //
    //   destinationArray:
    //     System.Array que recibe los datos.
    //
    //   destinationIndex:
    //     Entero de 64 bits que representa el índice de destinationArray donde comienza
    //     el almacenamiento.
    //
    //   length:
    //     Entero de 64 bits que representa el número de elementos que se van a copiar.
    //     El entero debe estar entre cero y System.Int32.MaxValue, ambos inclusive.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     sourceArray es null. O bien destinationArray es null.
    //
    //   T:System.RankException:
    //     sourceArray y destinationArray tienen clasificaciones diferentes.
    //
    //   T:System.ArrayTypeMismatchException:
    //     sourceArray y destinationArray son de tipos incompatibles.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de sourceArray no se puede convertir al tipo de destinationArray.
    //
    //
    //   T:System.ArgumentOutOfRangeException:
    //     sourceIndex está fuera del intervalo de índices válidos para sourceArray. O bien
    //     destinationIndex está fuera del intervalo de índices válidos para destinationArray.
    //     O bien length es menor que 0 o mayor que System.Int32.MaxValue.
    //
    //   T:System.ArgumentException:
    //     length es mayor que el número de elementos desde sourceIndex hasta el final de
    //     sourceArray. O bien length es mayor que el número de elementos desde destinationIndex
    //     hasta el final de destinationArray.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
    {
        if (sourceIndex > int.MaxValue || sourceIndex < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (destinationIndex > int.MaxValue || destinationIndex < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (length > int.MaxValue || length < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        Copy(sourceArray, (int)sourceIndex, destinationArray, (int)destinationIndex, (int)length);
    }

    //
    // Resumen:
    //     Establece un intervalo de elementos de una matriz en el valor predeterminado
    //     de cada tipo de elemento.
    //
    // Parámetros:
    //   array:
    //     La matriz cuyos elementos deben borrarse.
    //
    //   index:
    //     Índice inicial del intervalo de elementos que se va a borrar.
    //
    //   length:
    //     Número de elementos que se van a borrar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.IndexOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //     O bien La suma de index y length es mayor que el tamaño de array.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [__DynamicallyInvokable]
    public static extern void Clear(Array array, int index, int length);

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de System.Array multidimensional.
    //     Los índices se especifican en forma de una matriz de enteros de 32 bits.
    //
    // Parámetros:
    //   indices:
    //     Matriz unidimensional de enteros de 32 bits que representan los índices que especifican
    //     la posición del elemento System.Array que se debe obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada en la System.Array multidimensional.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     indices es null.
    //
    //   T:System.ArgumentException:
    //     El número de dimensiones de la System.Array actual no es igual al número de elementos
    //     de indices.
    //
    //   T:System.IndexOutOfRangeException:
    //     Cualquier elemento de indices está fuera del intervalo de índices válidos para
    //     la dimensión correspondiente de la System.Array actual.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe object GetValue(params int[] indices)
    {
        if (indices == null)
        {
            throw new ArgumentNullException("indices");
        }

        if (Rank != indices.Length)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
        }

        TypedReference typedReference = default(TypedReference);
        fixed (int* pIndices = indices)
        {
            InternalGetReference(&typedReference, indices.Length, pIndices);
        }

        return TypedReference.InternalToObject(&typedReference);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de la matriz System.Array unidimensional.
    //     El índice se especifica como un entero de 32 bits.
    //
    // Parámetros:
    //   index:
    //     Entero de 32 bits que representa la posición del elemento System.Array que se
    //     va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada de la matriz System.Array unidimensional.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente una dimensión.
    //
    //   T:System.IndexOutOfRangeException:
    //     index está fuera del intervalo de índices válidos para el System.Array actual.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe object GetValue(int index)
    {
        if (Rank != 1)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_Need1DArray"));
        }

        TypedReference typedReference = default(TypedReference);
        InternalGetReference(&typedReference, 1, &index);
        return TypedReference.InternalToObject(&typedReference);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de la matriz bidimensional System.Array.
    //     Los índices se especifican como enteros de 32 bits.
    //
    // Parámetros:
    //   index1:
    //     Entero de 32 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a obtener.
    //
    //   index2:
    //     Entero de 32 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada de la System.Array bidimensional.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente dos dimensiones.
    //
    //   T:System.IndexOutOfRangeException:
    //     index1 o index2 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [SecuritySafeCritical]
    public unsafe object GetValue(int index1, int index2)
    {
        if (Rank != 2)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_Need2DArray"));
        }

        int* ptr = stackalloc int[2];
        *ptr = index1;
        ptr[1] = index2;
        TypedReference typedReference = default(TypedReference);
        InternalGetReference(&typedReference, 2, ptr);
        return TypedReference.InternalToObject(&typedReference);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de la matriz System.Array tridimensional.
    //     Los índices se especifican como enteros de 32 bits.
    //
    // Parámetros:
    //   index1:
    //     Entero de 32 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a obtener.
    //
    //   index2:
    //     Entero de 32 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a obtener.
    //
    //   index3:
    //     Entero de 32 bits que representa el índice de la tercera dimensión del elemento
    //     System.Array que se va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada de la System.Array tridimensional.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente tres dimensiones.
    //
    //   T:System.IndexOutOfRangeException:
    //     index1, index2 o index3 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [SecuritySafeCritical]
    public unsafe object GetValue(int index1, int index2, int index3)
    {
        if (Rank != 3)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_Need3DArray"));
        }

        int* ptr = stackalloc int[3];
        *ptr = index1;
        ptr[1] = index2;
        ptr[2] = index3;
        TypedReference typedReference = default(TypedReference);
        InternalGetReference(&typedReference, 3, ptr);
        return TypedReference.InternalToObject(&typedReference);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de la matriz System.Array unidimensional.
    //     El índice se especifica como un entero de 64 bits.
    //
    // Parámetros:
    //   index:
    //     Entero de 64 bits que representa la posición del elemento System.Array que se
    //     va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada de la matriz System.Array unidimensional.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente una dimensión.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index está fuera del intervalo de índices válidos para el System.Array actual.
    [ComVisible(false)]
    public object GetValue(long index)
    {
        if (index > int.MaxValue || index < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        return GetValue((int)index);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de la matriz bidimensional System.Array.
    //     Los índices se especifican como enteros de 64 bits.
    //
    // Parámetros:
    //   index1:
    //     Entero de 64 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a obtener.
    //
    //   index2:
    //     Entero de 64 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada de la System.Array bidimensional.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente dos dimensiones.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index1 o index2 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [ComVisible(false)]
    public object GetValue(long index1, long index2)
    {
        if (index1 > int.MaxValue || index1 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (index2 > int.MaxValue || index2 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        return GetValue((int)index1, (int)index2);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de la matriz System.Array tridimensional.
    //     Los índices se especifican como enteros de 64 bits.
    //
    // Parámetros:
    //   index1:
    //     Entero de 64 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a obtener.
    //
    //   index2:
    //     Entero de 64 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a obtener.
    //
    //   index3:
    //     Entero de 64 bits que representa el índice de la tercera dimensión del elemento
    //     System.Array que se va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada de la System.Array tridimensional.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente tres dimensiones.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index1, index2 o index3 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [ComVisible(false)]
    public object GetValue(long index1, long index2, long index3)
    {
        if (index1 > int.MaxValue || index1 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (index2 > int.MaxValue || index2 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (index3 > int.MaxValue || index3 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index3", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        return GetValue((int)index1, (int)index2, (int)index3);
    }

    //
    // Resumen:
    //     Obtiene el valor de la posición especificada de System.Array multidimensional.
    //     Los índices se especifican en forma de una matriz de enteros de 64 bits.
    //
    // Parámetros:
    //   indices:
    //     Matriz unidimensional de enteros de 64 bits que representan los índices que especifican
    //     la posición del elemento System.Array que se va a obtener.
    //
    // Devuelve:
    //     Valor de la posición especificada en la System.Array multidimensional.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     indices es null.
    //
    //   T:System.ArgumentException:
    //     El número de dimensiones de la System.Array actual no es igual al número de elementos
    //     de indices.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     Cualquier elemento de indices está fuera del intervalo de índices válidos para
    //     la dimensión correspondiente de la System.Array actual.
    [ComVisible(false)]
    public object GetValue(params long[] indices)
    {
        if (indices == null)
        {
            throw new ArgumentNullException("indices");
        }

        if (Rank != indices.Length)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
        }

        int[] array = new int[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            long num = indices[i];
            if (num > int.MaxValue || num < int.MinValue)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }

            array[i] = (int)num;
        }

        return GetValue(array);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de una
    //     matriz System.Array unidimensional. El índice se especifica como un entero de
    //     32 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   index:
    //     Entero de 32 bits que representa la posición del elemento System.Array que se
    //     va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente una dimensión.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento de la System.Array actual.
    //
    //   T:System.IndexOutOfRangeException:
    //     index está fuera del intervalo de índices válidos para el System.Array actual.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe void SetValue(object value, int index)
    {
        if (Rank != 1)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_Need1DArray"));
        }

        TypedReference typedReference = default(TypedReference);
        InternalGetReference(&typedReference, 1, &index);
        InternalSetValue(&typedReference, value);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de la System.Array
    //     bidimensional. Los índices se especifican como enteros de 32 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   index1:
    //     Entero de 32 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a establecer.
    //
    //   index2:
    //     Entero de 32 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente dos dimensiones.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento de la System.Array actual.
    //
    //   T:System.IndexOutOfRangeException:
    //     index1 o index2 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [SecuritySafeCritical]
    public unsafe void SetValue(object value, int index1, int index2)
    {
        if (Rank != 2)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_Need2DArray"));
        }

        int* ptr = stackalloc int[2];
        *ptr = index1;
        ptr[1] = index2;
        TypedReference typedReference = default(TypedReference);
        InternalGetReference(&typedReference, 2, ptr);
        InternalSetValue(&typedReference, value);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de la System.Array
    //     tridimensional. Los índices se especifican como enteros de 32 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   index1:
    //     Entero de 32 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a establecer.
    //
    //   index2:
    //     Entero de 32 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a establecer.
    //
    //   index3:
    //     Entero de 32 bits que representa el índice de la tercera dimensión del elemento
    //     System.Array que se va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente tres dimensiones.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento del System.Array actual.
    //
    //   T:System.IndexOutOfRangeException:
    //     index1, index2 o index3 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [SecuritySafeCritical]
    public unsafe void SetValue(object value, int index1, int index2, int index3)
    {
        if (Rank != 3)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_Need3DArray"));
        }

        int* ptr = stackalloc int[3];
        *ptr = index1;
        ptr[1] = index2;
        ptr[2] = index3;
        TypedReference typedReference = default(TypedReference);
        InternalGetReference(&typedReference, 3, ptr);
        InternalSetValue(&typedReference, value);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de una
    //     matriz System.Array multidimensional. Los índices se especifican en forma de
    //     una matriz de enteros de 32 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   indices:
    //     Matriz unidimensional de enteros de 32 bits que representan los índices que especifican
    //     la posición del elemento que se va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     indices es null.
    //
    //   T:System.ArgumentException:
    //     El número de dimensiones de la System.Array actual no es igual al número de elementos
    //     de indices.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento de la System.Array actual.
    //
    //   T:System.IndexOutOfRangeException:
    //     Cualquier elemento de indices está fuera del intervalo de índices válidos para
    //     la dimensión correspondiente de la System.Array actual.
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public unsafe void SetValue(object value, params int[] indices)
    {
        if (indices == null)
        {
            throw new ArgumentNullException("indices");
        }

        if (Rank != indices.Length)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
        }

        TypedReference typedReference = default(TypedReference);
        fixed (int* pIndices = indices)
        {
            InternalGetReference(&typedReference, indices.Length, pIndices);
        }

        InternalSetValue(&typedReference, value);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de una
    //     matriz System.Array unidimensional. El índice se especifica como un entero de
    //     64 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   index:
    //     Entero de 64 bits que representa la posición del elemento System.Array que se
    //     va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente una dimensión.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento de la System.Array actual.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index está fuera del intervalo de índices válidos para el System.Array actual.
    [ComVisible(false)]
    public void SetValue(object value, long index)
    {
        if (index > int.MaxValue || index < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        SetValue(value, (int)index);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de la System.Array
    //     bidimensional. Los índices se especifican como enteros de 64 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   index1:
    //     Entero de 64 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a establecer.
    //
    //   index2:
    //     Entero de 64 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente dos dimensiones.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento de la System.Array actual.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index1 o index2 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [ComVisible(false)]
    public void SetValue(object value, long index1, long index2)
    {
        if (index1 > int.MaxValue || index1 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (index2 > int.MaxValue || index2 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        SetValue(value, (int)index1, (int)index2);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de la System.Array
    //     tridimensional. Los índices se especifican como enteros de 64 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   index1:
    //     Entero de 64 bits que representa el índice de la primera dimensión del elemento
    //     System.Array que se va a establecer.
    //
    //   index2:
    //     Entero de 64 bits que representa el índice de la segunda dimensión del elemento
    //     System.Array que se va a establecer.
    //
    //   index3:
    //     Entero de 64 bits que representa el índice de la tercera dimensión del elemento
    //     System.Array que se va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentException:
    //     El System.Array actual no tiene exactamente tres dimensiones.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento del System.Array actual.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index1, index2 o index3 está fuera del intervalo de índices válidos para la dimensión
    //     correspondiente del System.Array actual.
    [ComVisible(false)]
    public void SetValue(object value, long index1, long index2, long index3)
    {
        if (index1 > int.MaxValue || index1 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (index2 > int.MaxValue || index2 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        if (index3 > int.MaxValue || index3 < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index3", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        SetValue(value, (int)index1, (int)index2, (int)index3);
    }

    //
    // Resumen:
    //     Establece un valor en el elemento situado en la posición especificada de una
    //     matriz System.Array multidimensional. Los índices se especifican en forma de
    //     una matriz de enteros de 64 bits.
    //
    // Parámetros:
    //   value:
    //     Nuevo valor para el elemento especificado.
    //
    //   indices:
    //     Matriz unidimensional de enteros de 64 bits que representan los índices que especifican
    //     la posición del elemento que se va a establecer.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     indices es null.
    //
    //   T:System.ArgumentException:
    //     El número de dimensiones de la System.Array actual no es igual al número de elementos
    //     de indices.
    //
    //   T:System.InvalidCastException:
    //     value no se puede convertir al tipo de elemento de la System.Array actual.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     Cualquier elemento de indices está fuera del intervalo de índices válidos para
    //     la dimensión correspondiente de la System.Array actual.
    [ComVisible(false)]
    public void SetValue(object value, params long[] indices)
    {
        if (indices == null)
        {
            throw new ArgumentNullException("indices");
        }

        if (Rank != indices.Length)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
        }

        int[] array = new int[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            long num = indices[i];
            if (num > int.MaxValue || num < int.MinValue)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
            }

            array[i] = (int)num;
        }

        SetValue(value, array);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    private unsafe extern void InternalGetReference(void* elemRef, int rank, int* pIndices);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    private unsafe static extern void InternalSetValue(void* target, object value);

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    private static int GetMedian(int low, int hi)
    {
        return low + (hi - low >> 1);
    }

    //
    // Resumen:
    //     Obtiene un entero de 32 bits que representa el número de elementos de la dimensión
    //     especificada de System.Array.
    //
    // Parámetros:
    //   dimension:
    //     Dimensión de base cero de System.Array cuya longitud debe determinarse.
    //
    // Devuelve:
    //     Entero de 32 bits que representa el número de elementos de la dimensión especificada.
    //
    //
    // Excepciones:
    //   T:System.IndexOutOfRangeException:
    //     dimension es menor que cero. O bien dimension es igual o mayor que System.Array.Rank.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public extern int GetLength(int dimension);

    //
    // Resumen:
    //     Obtiene un entero de 64 bits que representa el número de elementos de la dimensión
    //     especificada de System.Array.
    //
    // Parámetros:
    //   dimension:
    //     Dimensión de base cero de System.Array cuya longitud debe determinarse.
    //
    // Devuelve:
    //     Entero de 64 bits que representa el número de elementos de la dimensión especificada.
    //
    //
    // Excepciones:
    //   T:System.IndexOutOfRangeException:
    //     dimension es menor que cero. O bien dimension es igual o mayor que System.Array.Rank.
    [ComVisible(false)]
    public long GetLongLength(int dimension)
    {
        return GetLength(dimension);
    }

    //
    // Resumen:
    //     Obtiene el índice del último elemento de la dimensión especificada en la matriz.
    //
    //
    // Parámetros:
    //   dimension:
    //     Dimensión de base cero de la matriz cuyo límite superior debe determinarse.
    //
    // Devuelve:
    //     Índice del último elemento de la dimensión especificada en la matriz o -1 si
    //     la dimensión especificada está vacía.
    //
    // Excepciones:
    //   T:System.IndexOutOfRangeException:
    //     dimension es menor que cero. O bien dimension es igual o mayor que System.Array.Rank.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [__DynamicallyInvokable]
    public extern int GetUpperBound(int dimension);

    //
    // Resumen:
    //     Obtiene el índice del primer elemento de la dimensión especificada en la matriz.
    //
    //
    // Parámetros:
    //   dimension:
    //     Dimensión de base cero de la matriz cuyo índice de inicio debe determinarse.
    //
    //
    // Devuelve:
    //     Índice del primer elemento de la dimensión especificada en la matriz.
    //
    // Excepciones:
    //   T:System.IndexOutOfRangeException:
    //     dimension es menor que cero. O bien dimension es igual o mayor que System.Array.Rank.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [__DynamicallyInvokable]
    public extern int GetLowerBound(int dimension);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal extern int GetDataPtrOffsetInternal();

    //
    // Resumen:
    //     Cuando se llama a este método, siempre se produce una excepción System.NotSupportedException.
    //
    //
    // Parámetros:
    //   value:
    //     Objeto que se va a agregar a System.Collections.IList.
    //
    // Devuelve:
    //     No se admite el agregar un valor a una matriz. No se devuelve ningún valor.
    //
    // Excepciones:
    //   T:System.NotSupportedException:
    //     System.Collections.IList tiene un tamaño fijo.
    [__DynamicallyInvokable]
    int IList.Add(object value)
    {
        throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
    }

    //
    // Resumen:
    //     Determina si un elemento se encuentra en System.Collections.IList.
    //
    // Parámetros:
    //   value:
    //     Objeto que se va a buscar en la lista actual. El elemento que se va a buscar
    //     puede ser null para los tipos de referencia.
    //
    // Devuelve:
    //     true si value se encuentra en la matriz System.Collections.IList; en caso contrario,
    //     false.
    [__DynamicallyInvokable]
    bool IList.Contains(object value)
    {
        return IndexOf(this, value) >= GetLowerBound(0);
    }

    //
    // Resumen:
    //     Quita todos los elementos de System.Collections.IList.
    //
    // Excepciones:
    //   T:System.NotSupportedException:
    //     System.Collections.IList es de solo lectura.
    [__DynamicallyInvokable]
    void IList.Clear()
    {
        Clear(this, GetLowerBound(0), Length);
    }

    //
    // Resumen:
    //     Determina el índice de un elemento específico de System.Collections.IList.
    //
    // Parámetros:
    //   value:
    //     Objeto que se va a buscar en la lista actual.
    //
    // Devuelve:
    //     Es el índice del valor si se encuentra en la lista; en caso contrario, es -1.
    [__DynamicallyInvokable]
    int IList.IndexOf(object value)
    {
        return IndexOf(this, value);
    }

    //
    // Resumen:
    //     Inserta un elemento en la interfaz System.Collections.IList, en el índice especificado.
    //
    //
    // Parámetros:
    //   index:
    //     Índice en el que debe insertarse value.
    //
    //   value:
    //     Objeto que se va a insertar.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     index no es un índice válido para System.Collections.IList.
    //
    //   T:System.NotSupportedException:
    //     System.Collections.IList es de solo lectura. O bien System.Collections.IList
    //     tiene un tamaño fijo.
    //
    //   T:System.NullReferenceException:
    //     value es una referencia nula en la System.Collections.IList.
    [__DynamicallyInvokable]
    void IList.Insert(int index, object value)
    {
        throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
    }

    //
    // Resumen:
    //     Quita la primera aparición de un objeto específico de la interfaz System.Collections.IList.
    //
    //
    // Parámetros:
    //   value:
    //     Objeto que se va a quitar de System.Collections.IList.
    //
    // Excepciones:
    //   T:System.NotSupportedException:
    //     System.Collections.IList es de solo lectura. O bien System.Collections.IList
    //     tiene un tamaño fijo.
    [__DynamicallyInvokable]
    void IList.Remove(object value)
    {
        throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
    }

    //
    // Resumen:
    //     Quita el elemento de la interfaz System.Collections.IList que se encuentra en
    //     el índice especificado.
    //
    // Parámetros:
    //   index:
    //     Índice del elemento que se va a quitar.
    //
    // Excepciones:
    //   T:System.ArgumentOutOfRangeException:
    //     El índice no es válido en System.Collections.IList.
    //
    //   T:System.NotSupportedException:
    //     System.Collections.IList es de solo lectura. O bien System.Collections.IList
    //     tiene un tamaño fijo.
    [__DynamicallyInvokable]
    void IList.RemoveAt(int index)
    {
        throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
    }

    //
    // Resumen:
    //     Crea una copia superficial de la colección System.Array.
    //
    // Devuelve:
    //     Copia superficial de la colección System.Array.
    [__DynamicallyInvokable]
    public object Clone()
    {
        return MemberwiseClone();
    }

    //
    // Resumen:
    //     Determina si el objeto de colección actual precede o sigue a otro objeto en el
    //     criterio de ordenación, o aparece en la misma posición que él.
    //
    // Parámetros:
    //   other:
    //     Objeto que se va a comparar con la instancia actual.
    //
    //   comparer:
    //     Objeto que compara el objeto actual y other.
    //
    // Devuelve:
    //     Un entero que indica la relación del objeto de la colección actual con otros,
    //     tal y como se muestra en la tabla siguiente. Valor devuelto Descripción -1 La
    //     instancia actual precede a other. 0 La instancia actual y other son iguales.
    //     1 La instancia actual se encuentra detrás de other.
    [__DynamicallyInvokable]
    int IStructuralComparable.CompareTo(object other, IComparer comparer)
    {
        if (other == null)
        {
            return 1;
        }

        if (!(other is Array array) || Length != array.Length)
        {
            throw new ArgumentException(Environment.GetResourceString("ArgumentException_OtherNotArrayOfCorrectLength"), "other");
        }

        int i = 0;
        int num = 0;
        for (; i < array.Length; i++)
        {
            if (num != 0)
            {
                break;
            }

            object value = GetValue(i);
            object value2 = array.GetValue(i);
            num = comparer.Compare(value, value2);
        }

        return num;
    }

    //
    // Resumen:
    //     Determina si un objeto especificado es igual a la instancia actual.
    //
    // Parámetros:
    //   other:
    //     Objeto que se va a comparar con la instancia actual.
    //
    //   comparer:
    //     Un objeto que determina si la instancia actual y other son iguales.
    //
    // Devuelve:
    //     Es true si los dos objetos son iguales; en caso contrario, es false.
    [__DynamicallyInvokable]
    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
    {
        if (other == null)
        {
            return false;
        }

        if (this == other)
        {
            return true;
        }

        if (!(other is Array array) || array.Length != Length)
        {
            return false;
        }

        for (int i = 0; i < array.Length; i++)
        {
            object value = GetValue(i);
            object value2 = array.GetValue(i);
            if (!comparer.Equals(value, value2))
            {
                return false;
            }
        }

        return true;
    }

    internal static int CombineHashCodes(int h1, int h2)
    {
        return ((h1 << 5) + h1) ^ h2;
    }

    //
    // Resumen:
    //     Devuelve un código hash de la instancia actual.
    //
    // Parámetros:
    //   comparer:
    //     Objeto que calcula el código hash del objeto actual.
    //
    // Devuelve:
    //     Código hash de la instancia actual.
    [__DynamicallyInvokable]
    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        if (comparer == null)
        {
            throw new ArgumentNullException("comparer");
        }

        int num = 0;
        for (int i = ((Length >= 8) ? (Length - 8) : 0); i < Length; i++)
        {
            num = CombineHashCodes(num, comparer.GetHashCode(GetValue(i)));
        }

        return num;
    }

    //
    // Resumen:
    //     Busca un elemento específico en toda una matriz unidimensional y ordenada, usando
    //     la interfaz System.IComparable implementada por cada elemento de la matriz y
    //     por el objeto especificado.
    //
    // Parámetros:
    //   array:
    //     Matriz System.Array unidimensional y ordenada en la que se va a realizar la búsqueda.
    //
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentException:
    //     value es de un tipo que no es compatible con los elementos de array.
    //
    //   T:System.InvalidOperationException:
    //     value no implementa la interfaz System.IComparable y la búsqueda encuentra un
    //     elemento que no implementa la interfaz System.IComparable.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch(Array array, object value)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        return BinarySearch(array, lowerBound, array.Length, value, null);
    }

    //
    // Resumen:
    //     Busca un valor en un intervalo de elementos de una matriz unidimensional y ordenada,
    //     usando la interfaz System.IComparable implementada por cada elemento de la matriz
    //     y por el valor especificado.
    //
    // Parámetros:
    //   array:
    //     Matriz System.Array unidimensional y ordenada en la que se va a realizar la búsqueda.
    //
    //
    //   index:
    //     Índice inicial del intervalo en el que se va a buscar.
    //
    //   length:
    //     Longitud del intervalo en el que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array. O bien value es de
    //     un tipo que no es compatible con los elementos de array.
    //
    //   T:System.InvalidOperationException:
    //     value no implementa la interfaz System.IComparable y la búsqueda encuentra un
    //     elemento que no implementa la interfaz System.IComparable.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch(Array array, int index, int length, object value)
    {
        return BinarySearch(array, index, length, value, null);
    }

    //
    // Resumen:
    //     Busca un valor por toda una matriz unidimensional y ordenada, usando la interfaz
    //     System.Collections.IComparer especificada.
    //
    // Parámetros:
    //   array:
    //     Matriz System.Array unidimensional y ordenada en la que se va a realizar la búsqueda.
    //
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    //   comparer:
    //     Implementación de System.Collections.IComparer que se va a usar al comparar elementos.
    //     O bien null para usar la implementación de System.IComparable de cada elemento.
    //
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentException:
    //     comparer es null y value es de un tipo que no es compatible con los elementos
    //     de array.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null, value no implementa la interfaz System.IComparable y la búsqueda
    //     encuentra un elemento que no implementa la interfaz System.IComparable.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch(Array array, object value, IComparer comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        return BinarySearch(array, lowerBound, array.Length, value, comparer);
    }

    //
    // Resumen:
    //     Busca un valor en un intervalo de elementos de una matriz unidimensional y ordenada,
    //     usando la interfaz System.Collections.IComparer especificada.
    //
    // Parámetros:
    //   array:
    //     Matriz System.Array unidimensional y ordenada en la que se va a realizar la búsqueda.
    //
    //
    //   index:
    //     Índice inicial del intervalo en el que se va a buscar.
    //
    //   length:
    //     Longitud del intervalo en el que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    //   comparer:
    //     Implementación de System.Collections.IComparer que se va a usar al comparar elementos.
    //     O bien null para usar la implementación de System.IComparable de cada elemento.
    //
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array. O bien comparer es
    //     null, y value es de un tipo que no es compatible con los elementos de array.
    //
    //
    //   T:System.InvalidOperationException:
    //     comparer es null, value no implementa la interfaz System.IComparable y la búsqueda
    //     encuentra un elemento que no implementa la interfaz System.IComparable.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch(Array array, int index, int length, object value, IComparer comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        if (index < lowerBound || length < 0)
        {
            throw new ArgumentOutOfRangeException((index < lowerBound) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (array.Length - (index - lowerBound) < length)
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
        }

        if (array.Rank != 1)
        {
            throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
        }

        if (comparer == null)
        {
            comparer = Comparer.Default;
        }

        if (comparer == Comparer.Default && TrySZBinarySearch(array, index, length, value, out var retVal))
        {
            return retVal;
        }

        int num = index;
        int num2 = index + length - 1;
        if (array is object[] array2)
        {
            while (num <= num2)
            {
                int median = GetMedian(num, num2);
                int num3;
                try
                {
                    num3 = comparer.Compare(array2[median], value);
                }
                catch (Exception innerException)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
                }

                if (num3 == 0)
                {
                    return median;
                }

                if (num3 < 0)
                {
                    num = median + 1;
                }
                else
                {
                    num2 = median - 1;
                }
            }
        }
        else
        {
            while (num <= num2)
            {
                int median2 = GetMedian(num, num2);
                int num4;
                try
                {
                    num4 = comparer.Compare(array.GetValue(median2), value);
                }
                catch (Exception innerException2)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException2);
                }

                if (num4 == 0)
                {
                    return median2;
                }

                if (num4 < 0)
                {
                    num = median2 + 1;
                }
                else
                {
                    num2 = median2 - 1;
                }
            }
        }

        return ~num;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    private static extern bool TrySZBinarySearch(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

    //
    // Resumen:
    //     Busca un elemento específico en una matriz unidimensional ordenada completa,
    //     usando la interfaz genérica System.IComparable`1 que implementan cada elemento
    //     de System.Array y el objeto especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional ordenada de base cero en la que se va a realizar
    //     la búsqueda.
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.InvalidOperationException:
    //     T no implementa la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch<T>(T[] array, T value)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return BinarySearch(array, 0, array.Length, value, null);
    }

    //
    // Resumen:
    //     Busca un valor por toda una matriz unidimensional y ordenada, usando la interfaz
    //     genérica System.Collections.Generic.IComparer`1 especificada.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional ordenada de base cero en la que se va a realizar
    //     la búsqueda.
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    //   comparer:
    //     Implementación de System.Collections.Generic.IComparer`1 que se va a usar al
    //     comparar elementos. O bien null para usar la implementación de System.IComparable`1
    //     de cada elemento.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentException:
    //     comparer es null, y value es de un tipo que no es compatible con los elementos
    //     de array.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y T no implementa la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch<T>(T[] array, T value, IComparer<T> comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return BinarySearch(array, 0, array.Length, value, comparer);
    }

    //
    // Resumen:
    //     Busca un valor en un intervalo de elementos de una matriz unidimensional y ordenada,
    //     usando la interfaz genérica System.IComparable`1 que implementan cada elemento
    //     de System.Array y el valor especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array de base cero, unidimensional y ordenada en la que se va a buscar.
    //
    //
    //   index:
    //     Índice inicial del intervalo en el que se va a buscar.
    //
    //   length:
    //     Longitud del intervalo en el que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array. O bien value es de
    //     un tipo que no es compatible con los elementos de array.
    //
    //   T:System.InvalidOperationException:
    //     T no implementa la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch<T>(T[] array, int index, int length, T value)
    {
        return BinarySearch(array, index, length, value, null);
    }

    //
    // Resumen:
    //     Busca un valor en un intervalo de elementos de una matriz unidimensional y ordenada,
    //     usando la interfaz genérica System.Collections.Generic.IComparer`1 especificada.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array de base cero, unidimensional y ordenada en la que se va a buscar.
    //
    //
    //   index:
    //     Índice inicial del intervalo en el que se va a buscar.
    //
    //   length:
    //     Longitud del intervalo en el que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar.
    //
    //   comparer:
    //     Implementación de System.Collections.Generic.IComparer`1 que se va a usar al
    //     comparar elementos. O bien null para usar la implementación de System.IComparable`1
    //     de cada elemento.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice del elemento value especificado en el objeto array especificado, si se
    //     encuentra value; en caso contrario, un número negativo. Si no se encuentra value
    //     y value es menor que uno o varios elementos de array, el número negativo devuelto
    //     es el complemento bit a bit del índice del primer elemento que sea mayor que
    //     value. Si no se encuentra value y value es mayor que cualquiera de los elementos
    //     de array, el número negativo devuelto es el complemento bit a bit del índice
    //     del último elemento más 1. Si se llama a este método con un objeto array sin
    //     clasificar, el valor devuelto puede ser incorrecto y podría devolverse un número
    //     negativo, aunque value esté presente en array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array. O bien comparer es
    //     null, y value es de un tipo que no es compatible con los elementos de array.
    //
    //
    //   T:System.InvalidOperationException:
    //     comparer es null, y T no implementa la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T> comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (index < 0 || length < 0)
        {
            throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (array.Length - index < length)
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
        }

        return ArraySortHelper<T>.Default.BinarySearch(array, index, length, value, comparer);
    }

    //
    // Resumen:
    //     Convierte una matriz de un tipo en una matriz de otro tipo.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero que se va a convertir en un tipo de
    //     destino.
    //
    //   converter:
    //     System.Converter`2 que convierte cada elemento en otro tipo.
    //
    // Parámetros de tipo:
    //   TInput:
    //     Tipo de los elementos de la matriz de origen.
    //
    //   TOutput:
    //     Tipo de los elementos de la matriz de destino.
    //
    // Devuelve:
    //     Matriz del tipo de destino que contiene los elementos convertidos de la matriz
    //     de origen.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien converter es null.
    public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (converter == null)
        {
            throw new ArgumentNullException("converter");
        }

        TOutput[] array2 = new TOutput[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array2[i] = converter(array[i]);
        }

        return array2;
    }

    //
    // Resumen:
    //     Copia todos los elementos de la matriz unidimensional actual en la matriz unidimensional
    //     especificada, empezando en el índice especificado de la matriz de destino. El
    //     índice se especifica como un entero de 32 bits.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional que constituye el destino de los elementos copiados desde
    //     la matriz actual.
    //
    //   index:
    //     Entero de 32 bits que representa el índice de array donde comienza la copia.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array.
    //
    //   T:System.ArgumentException:
    //     array es multidimensional. O bien El número de elementos de la matriz de origen
    //     es mayor que el número de elementos disponible desde index hasta el final de
    //     la array de destino.
    //
    //   T:System.ArrayTypeMismatchException:
    //     El tipo de la System.Array de origen no puede convertirse automáticamente al
    //     tipo de la array de destino.
    //
    //   T:System.RankException:
    //     La matriz de origen es multidimensional.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de la System.Array de origen no se puede convertir al tipo
    //     de array de destino.
    [__DynamicallyInvokable]
    public void CopyTo(Array array, int index)
    {
        if (array != null && array.Rank != 1)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
        }

        Copy(this, GetLowerBound(0), array, index, Length);
    }

    //
    // Resumen:
    //     Copia todos los elementos de la matriz unidimensional actual en la matriz unidimensional
    //     especificada, empezando en el índice especificado de la matriz de destino. El
    //     índice se especifica como un entero de 64 bits.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional que constituye el destino de los elementos copiados desde
    //     la matriz actual.
    //
    //   index:
    //     Entero de 64 bits que representa el índice de array donde comienza la copia.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index está fuera del intervalo de índices válidos para array.
    //
    //   T:System.ArgumentException:
    //     array es multidimensional. O bien El número de elementos de la matriz de origen
    //     es mayor que el número de elementos disponible desde index hasta el final de
    //     la array de destino.
    //
    //   T:System.ArrayTypeMismatchException:
    //     El tipo de la System.Array de origen no puede convertirse automáticamente al
    //     tipo de la array de destino.
    //
    //   T:System.RankException:
    //     La System.Array de origen es multidimensional.
    //
    //   T:System.InvalidCastException:
    //     Al menos un elemento de la System.Array de origen no se puede convertir al tipo
    //     de array de destino.
    [ComVisible(false)]
    public void CopyTo(Array array, long index)
    {
        if (index > int.MaxValue || index < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
        }

        CopyTo(array, (int)index);
    }

    //
    // Resumen:
    //     Devuelve una matriz vacía.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Matriz vacía.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static T[] Empty<T>()
    {
        return EmptyArray<T>.Value;
    }

    //
    // Resumen:
    //     Determina si la matriz especificada contiene elementos que coinciden con las
    //     condiciones definidas por el predicado especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   match:
    //     System.Predicate`1 que define las condiciones de los elementos que se van a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     true si array contiene uno o varios elementos que coinciden con las condiciones
    //     definidas por el predicado especificado; de lo contrario, false.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static bool Exists<T>(T[] array, Predicate<T> match)
    {
        return FindIndex(array, match) != -1;
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve la primera aparición en toda la matriz System.Array.
    //
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional de base cero en la que se va a buscar.
    //
    //   match:
    //     Predicado que define las condiciones del elemento que se va a buscar.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Primer elemento que coincide con las condiciones definidas por el predicado especificado,
    //     si se encuentra; de lo contrario, valor predeterminado para el tipo T.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static T Find<T>(T[] array, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }

        for (int i = 0; i < array.Length; i++)
        {
            if (match(array[i]))
            {
                return array[i];
            }
        }

        return default(T);
    }

    //
    // Resumen:
    //     Recupera todos los elementos que coinciden con las condiciones definidas por
    //     el predicado especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   match:
    //     System.Predicate`1 que define las condiciones de los elementos que se van a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     System.Array que contiene todos los elementos que cumplen las condiciones definidas
    //     por el predicado especificado, si se encuentran; en caso contrario, devuelve
    //     una System.Array vacía.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static T[] FindAll<T>(T[] array, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }

        List<T> list = new List<T>();
        for (int i = 0; i < array.Length; i++)
        {
            if (match(array[i]))
            {
                list.Add(array[i]);
            }
        }

        return list.ToArray();
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve el índice de base cero de la primera aparición en toda
    //     la matriz System.Array.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la primera aparición de un elemento que coincide con las
    //     condiciones definidas por match, si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static int FindIndex<T>(T[] array, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return FindIndex(array, 0, array.Length, match);
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve el índice de base cero de la primera aparición en el
    //     intervalo de elementos de la matriz System.Array que va desde el índice especificado
    //     hasta el último elemento.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda.
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la primera aparición de un elemento que coincide con las
    //     condiciones definidas por match, si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array.
    [__DynamicallyInvokable]
    public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return FindIndex(array, startIndex, array.Length - startIndex, match);
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve el índice de base cero de la primera aparición en el
    //     intervalo de elementos de la matriz System.Array que comienza en el índice especificado
    //     y contiene el número especificado de elementos.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda.
    //
    //   count:
    //     Número de elementos de la sección en la que se va a realizar la búsqueda.
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la primera aparición de un elemento que coincide con las
    //     condiciones definidas por match, si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array. O bien
    //     count es menor que cero. O bien startIndex y count no especifican una sección
    //     válida en array.
    [__DynamicallyInvokable]
    public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (startIndex < 0 || startIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        if (count < 0 || startIndex > array.Length - count)
        {
            throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }

        int num = startIndex + count;
        for (int i = startIndex; i < num; i++)
        {
            if (match(array[i]))
            {
                return i;
            }
        }

        return -1;
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve la última aparición en toda la matriz System.Array.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Último elemento que coincide con las condiciones definidas por el predicado especificado,
    //     si se encuentra; de lo contrario, valor predeterminado para el tipo T.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static T FindLast<T>(T[] array, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }

        for (int num = array.Length - 1; num >= 0; num--)
        {
            if (match(array[num]))
            {
                return array[num];
            }
        }

        return default(T);
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve el índice de base cero de la última aparición en toda
    //     la matriz System.Array.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la última aparición de un elemento que coincide con las
    //     condiciones definidas por match, si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static int FindLastIndex<T>(T[] array, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return FindLastIndex(array, array.Length - 1, array.Length, match);
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve el índice de base cero de la última aparición en el intervalo
    //     de elementos de la matriz System.Array que va desde el primer elemento hasta
    //     el índice especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda hacia atrás.
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la última aparición de un elemento que coincide con las
    //     condiciones definidas por match, si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array.
    [__DynamicallyInvokable]
    public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return FindLastIndex(array, startIndex, startIndex + 1, match);
    }

    //
    // Resumen:
    //     Busca un elemento que coincida con las condiciones definidas por el predicado
    //     especificado y devuelve el índice de base cero de la última aparición en el intervalo
    //     de elementos de la matriz System.Array que contiene el número especificado de
    //     elementos y termina en el índice especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda hacia atrás.
    //
    //   count:
    //     Número de elementos de la sección en la que se va a realizar la búsqueda.
    //
    //   match:
    //     System.Predicate`1 que define las condiciones del elemento que se va a buscar.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la última aparición de un elemento que coincide con las
    //     condiciones definidas por match, si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array. O bien
    //     count es menor que cero. O bien startIndex y count no especifican una sección
    //     válida en array.
    [__DynamicallyInvokable]
    public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }

        if (array.Length == 0)
        {
            if (startIndex != -1)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
        }
        else if (startIndex < 0 || startIndex >= array.Length)
        {
            throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        if (count < 0 || startIndex - count + 1 < 0)
        {
            throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
        }

        int num = startIndex - count;
        for (int num2 = startIndex; num2 > num; num2--)
        {
            if (match(array[num2]))
            {
                return num2;
            }
        }

        return -1;
    }

    //
    // Resumen:
    //     Realiza la acción especificada en cada elemento de la matriz especificada.
    //
    // Parámetros:
    //   array:
    //     Matriz System.Array unidimensional de base cero en cuyos elementos se va a llevar
    //     a cabo la acción.
    //
    //   action:
    //     Delegado System.Action`1 que se va a ejecutar en cada elemento de array.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien action es null.
    public static void ForEach<T>(T[] array, Action<T> action)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        for (int i = 0; i < array.Length; i++)
        {
            action(array[i]);
        }
    }

    //
    // Resumen:
    //     Devuelve una interfaz System.Collections.IEnumerator para la interfaz System.Array.
    //
    //
    // Devuelve:
    //     Estructura System.Collections.IEnumerator para la colección System.Array.
    [__DynamicallyInvokable]
    public IEnumerator GetEnumerator()
    {
        int lowerBound = GetLowerBound(0);
        if (Rank == 1 && lowerBound == 0)
        {
            return new SZArrayEnumerator(this);
        }

        return new ArrayEnumerator(this, lowerBound, Length);
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de su primera aparición en
    //     una matriz unidimensional.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional en la que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    // Devuelve:
    //     Índice de la primera aparición de value en la matriz array, si se encuentra;
    //     en caso contrario, límite inferior de la matriz menos 1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int IndexOf(Array array, object value)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        return IndexOf(array, value, lowerBound, array.Length);
    }

    //
    // Resumen:
    //     Busca el objeto especificado en un intervalo de elementos de la matriz unidimensional
    //     y devuelve el índice de su primera aparición. El intervalo se extiende desde
    //     un índice especificado hasta el final de la matriz.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional en la que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de la búsqueda. 0 (cero) es válido en una matriz vacía.
    //
    // Devuelve:
    //     Índice de la primera aparición de value, si se encuentra, dentro del intervalo
    //     de elementos de array que abarca desde startIndex hasta el último elemento; de
    //     lo contrario, límite inferior de la matriz menos 1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para array.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int IndexOf(Array array, object value, int startIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        return IndexOf(array, value, startIndex, array.Length - startIndex + lowerBound);
    }

    //
    // Resumen:
    //     Busca el objeto especificado en un intervalo de elementos de una matriz unidimensional
    //     y devuelve el índice de su primera aparición. El intervalo se extiende desde
    //     un índice especificado durante un número especificado de elementos.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional en la que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de la búsqueda. 0 (cero) es válido en una matriz vacía.
    //
    //   count:
    //     Número de elementos que se van a buscar.
    //
    // Devuelve:
    //     Índice de la primera aparición de value, si se encuentra, en array desde el índice
    //     startIndex hasta startIndex + count - 1; de lo contrario, límite inferior de
    //     la matriz menos 1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array. O bien
    //     count es menor que cero. O bien startIndex y count no especifican una sección
    //     válida en array.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int IndexOf(Array array, object value, int startIndex, int count)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (array.Rank != 1)
        {
            throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
        }

        int lowerBound = array.GetLowerBound(0);
        if (startIndex < lowerBound || startIndex > array.Length + lowerBound)
        {
            throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        if (count < 0 || count > array.Length - startIndex + lowerBound)
        {
            throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
        }

        if (TrySZIndexOf(array, startIndex, count, value, out var retVal))
        {
            return retVal;
        }

        object[] array2 = array as object[];
        int num = startIndex + count;
        if (array2 != null)
        {
            if (value == null)
            {
                for (int i = startIndex; i < num; i++)
                {
                    if (array2[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int j = startIndex; j < num; j++)
                {
                    object obj = array2[j];
                    if (obj != null && obj.Equals(value))
                    {
                        return j;
                    }
                }
            }
        }
        else
        {
            for (int k = startIndex; k < num; k++)
            {
                object value2 = array.GetValue(k);
                if (value2 == null)
                {
                    if (value == null)
                    {
                        return k;
                    }
                }
                else if (value2.Equals(value))
                {
                    return k;
                }
            }
        }

        return lowerBound - 1;
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de su primera aparición en
    //     una matriz unidimensional.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional de base cero en la que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la primera aparición de value en la totalidad de array,
    //     si se encuentra; en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    [__DynamicallyInvokable]
    public static int IndexOf<T>(T[] array, T value)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return IndexOf(array, value, 0, array.Length);
    }

    //
    // Resumen:
    //     Busca el objeto especificado en un intervalo de elementos de la matriz unidimensional
    //     y devuelve el índice de su primera aparición. El intervalo se extiende desde
    //     un índice especificado hasta el final de la matriz.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional de base cero en la que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda. 0 (cero) es válido en una matriz
    //     vacía.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la primera aparición de value dentro del intervalo de
    //     elementos de la matriz array que abarca desde startIndex hasta el último elemento,
    //     si se encuentra; de lo contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array.
    [__DynamicallyInvokable]
    public static int IndexOf<T>(T[] array, T value, int startIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return IndexOf(array, value, startIndex, array.Length - startIndex);
    }

    //
    // Resumen:
    //     Busca el objeto especificado en un intervalo de elementos de la matriz unidimensional
    //     y devuelve el índice de su primera aparición. El intervalo se extiende desde
    //     un índice especificado durante un número especificado de elementos.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional de base cero en la que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda. 0 (cero) es válido en una matriz
    //     vacía.
    //
    //   count:
    //     Número de elementos de la sección en la que se va a realizar la búsqueda.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la primera aparición de value dentro del intervalo de
    //     elementos de array que comienza en startIndex y contiene el número de elementos
    //     especificados en count, si se encuentra; de lo contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array. O bien
    //     count es menor que cero. O bien startIndex y count no especifican una sección
    //     válida en array.
    [__DynamicallyInvokable]
    public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (startIndex < 0 || startIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        if (count < 0 || count > array.Length - startIndex)
        {
            throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
        }

        return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    private static extern bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de la última aparición en toda
    //     la matriz System.Array unidimensional.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    // Devuelve:
    //     Índice de la última aparición de value en toda la matriz array, si se encuentra;
    //     en caso contrario, límite inferior de la matriz menos 1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int LastIndexOf(Array array, object value)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        return LastIndexOf(array, value, array.Length - 1 + lowerBound, array.Length);
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de la última aparición en el
    //     intervalo de elementos de la matriz System.Array unidimensional que se extiende
    //     desde el primer elemento hasta el índice especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de la búsqueda hacia atrás.
    //
    // Devuelve:
    //     Índice de la última aparición de value en el intervalo de elementos de array
    //     que se extiende desde el primer elemento hasta startIndex, si se encuentra; en
    //     caso contrario, límite inferior de la matriz menos 1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para array.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int LastIndexOf(Array array, object value, int startIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        return LastIndexOf(array, value, startIndex, startIndex + 1 - lowerBound);
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de la última aparición dentro
    //     del intervalo de elementos de la System.Array unidimensional que contiene el
    //     número especificado de elementos y termina en el índice especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a buscar.
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de la búsqueda hacia atrás.
    //
    //   count:
    //     Número de elementos de la sección en la que se va a realizar la búsqueda.
    //
    // Devuelve:
    //     El índice de la última aparición de value dentro del intervalo de elementos de
    //     array que contiene el número de elementos especificado en count y termina en
    //     startIndex, si se encuentra; en caso contrario, el límite inferior de la matriz
    //     menos 1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array. O bien
    //     count es menor que cero. O bien startIndex y count no especifican una sección
    //     válida en array.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static int LastIndexOf(Array array, object value, int startIndex, int count)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        int lowerBound = array.GetLowerBound(0);
        if (array.Length == 0)
        {
            return lowerBound - 1;
        }

        if (startIndex < lowerBound || startIndex >= array.Length + lowerBound)
        {
            throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
        }

        if (count > startIndex - lowerBound + 1)
        {
            throw new ArgumentOutOfRangeException("endIndex", Environment.GetResourceString("ArgumentOutOfRange_EndIndexStartIndex"));
        }

        if (array.Rank != 1)
        {
            throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
        }

        if (TrySZLastIndexOf(array, startIndex, count, value, out var retVal))
        {
            return retVal;
        }

        object[] array2 = array as object[];
        int num = startIndex - count + 1;
        if (array2 != null)
        {
            if (value == null)
            {
                for (int num2 = startIndex; num2 >= num; num2--)
                {
                    if (array2[num2] == null)
                    {
                        return num2;
                    }
                }
            }
            else
            {
                for (int num3 = startIndex; num3 >= num; num3--)
                {
                    object obj = array2[num3];
                    if (obj != null && obj.Equals(value))
                    {
                        return num3;
                    }
                }
            }
        }
        else
        {
            for (int num4 = startIndex; num4 >= num; num4--)
            {
                object value2 = array.GetValue(num4);
                if (value2 == null)
                {
                    if (value == null)
                    {
                        return num4;
                    }
                }
                else if (value2.Equals(value))
                {
                    return num4;
                }
            }
        }

        return lowerBound - 1;
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de la última aparición de toda
    //     la System.Array.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la última aparición de value en toda la array, si se encuentra;
    //     en caso contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    [__DynamicallyInvokable]
    public static int LastIndexOf<T>(T[] array, T value)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return LastIndexOf(array, value, array.Length - 1, array.Length);
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de la última aparición en el
    //     intervalo de elementos de la System.Array que se extiende desde el primer elemento
    //     hasta el índice especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda hacia atrás.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la última aparición de value dentro del intervalo de elementos
    //     de array que abarca desde el primer elemento hasta startIndex, si se encuentra;
    //     de lo contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array.
    [__DynamicallyInvokable]
    public static int LastIndexOf<T>(T[] array, T value, int startIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        return LastIndexOf(array, value, startIndex, (array.Length != 0) ? (startIndex + 1) : 0);
    }

    //
    // Resumen:
    //     Busca el objeto especificado y devuelve el índice de la última aparición en el
    //     intervalo de elementos de la System.Array que contiene el número de elementos
    //     especificado y termina en el índice especificado.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero en la que se realizará la búsqueda.
    //
    //
    //   value:
    //     Objeto que se va a buscar en array.
    //
    //   startIndex:
    //     Índice inicial de base cero de la búsqueda hacia atrás.
    //
    //   count:
    //     Número de elementos de la sección en la que se va a realizar la búsqueda.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     Índice de base cero de la última aparición de value dentro del intervalo de elementos
    //     de array que contiene el número de elementos especificado en count y termina
    //     en startIndex, si se encuentra; de lo contrario, -1.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     startIndex está fuera del intervalo de índices válidos para la array. O bien
    //     count es menor que cero. O bien startIndex y count no especifican una sección
    //     válida en array.
    [__DynamicallyInvokable]
    public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (array.Length == 0)
        {
            if (startIndex != -1 && startIndex != 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }

            if (count != 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }

            return -1;
        }

        if (startIndex < 0 || startIndex >= array.Length)
        {
            throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
        }

        if (count < 0 || startIndex - count + 1 < 0)
        {
            throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
        }

        return EqualityComparer<T>.Default.LastIndexOf(array, value, startIndex, count);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    private static extern bool TrySZLastIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

    //
    // Resumen:
    //     Invierte la secuencia de los elementos de toda la matriz System.Array unidimensional.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a invertir.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Reverse(Array array)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        Reverse(array, array.GetLowerBound(0), array.Length);
    }

    //
    // Resumen:
    //     Invierte la secuencia de los elementos de un intervalo de elementos de la System.Array
    //     unidimensional.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a invertir.
    //
    //   index:
    //     Índice inicial de la sección que se va a invertir.
    //
    //   length:
    //     Número de elementos de la sección que se van a invertir.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Reverse(Array array, int index, int length)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (index < array.GetLowerBound(0) || length < 0)
        {
            throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (array.Length - (index - array.GetLowerBound(0)) < length)
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
        }

        if (array.Rank != 1)
        {
            throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
        }

        if (TrySZReverse(array, index, length))
        {
            return;
        }

        int num = index;
        int num2 = index + length - 1;
        if (array is object[] array2)
        {
            while (num < num2)
            {
                object obj = array2[num];
                array2[num] = array2[num2];
                array2[num2] = obj;
                num++;
                num2--;
            }
        }
        else
        {
            while (num < num2)
            {
                object value = array.GetValue(num);
                array.SetValue(array.GetValue(num2), num);
                array.SetValue(value, num2);
                num++;
                num2--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    private static extern bool TrySZReverse(Array array, int index, int count);

    //
    // Resumen:
    //     Ordena los elementos de toda una matriz System.Array unidimensional usando la
    //     implementación de System.IComparable de cada elemento de la matriz System.Array.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a ordenar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos de la array no implementan la interfaz de System.IComparable.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array array)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        Sort(array, null, array.GetLowerBound(0), array.Length, null);
    }

    //
    // Resumen:
    //     Ordena un par de objetos System.Array unidimensionales (uno contiene las claves
    //     y el otro contiene los elementos correspondientes) en función de las claves de
    //     la primera System.Array usando la implementación de System.IComparable de cada
    //     clave.
    //
    // Parámetros:
    //   keys:
    //     La System.Array unidimensional que contiene las claves que se van a ordenar.
    //
    //
    //   items:
    //     La System.Array unidimensional que contiene los elementos que se corresponden
    //     con cada una de las claves de keysSystem.Array. O bien null para ordenar solo
    //     las keysSystem.Array.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.RankException:
    //     El objeto System.Array de keys es multidimensional. O bien El objeto System.Array
    //     de items es multidimensional.
    //
    //   T:System.ArgumentException:
    //     itemsno es null y la longitud de keys es mayor que la longitud de items.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos del objeto System.Array de keys no implementan la interfaz
    //     System.IComparable.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array keys, Array items)
    {
        if (keys == null)
        {
            throw new ArgumentNullException("keys");
        }

        Sort(keys, items, keys.GetLowerBound(0), keys.Length, null);
    }

    //
    // Resumen:
    //     Ordena los elementos de un intervalo de elementos de una matriz System.Array
    //     unidimensional mediante el uso de la implementación de System.IComparable de
    //     cada elemento de la matriz System.Array.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a ordenar.
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos de la array no implementan la interfaz de System.IComparable.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array array, int index, int length)
    {
        Sort(array, null, index, length, null);
    }

    //
    // Resumen:
    //     Ordena un intervalo de elementos en un par de objetos System.Array unidimensionales
    //     (uno contiene las claves y el otro contiene los elementos correspondientes) en
    //     función de las claves de la primera matriz System.Array usando la implementación
    //     de System.IComparable de cada clave.
    //
    // Parámetros:
    //   keys:
    //     La System.Array unidimensional que contiene las claves que se van a ordenar.
    //
    //
    //   items:
    //     La System.Array unidimensional que contiene los elementos que se corresponden
    //     con cada una de las claves de keysSystem.Array. O bien null para ordenar solo
    //     las keysSystem.Array.
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.RankException:
    //     El objeto System.Array de keys es multidimensional. O bien El objeto System.Array
    //     de items es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de keys. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     items no es null y la longitud de keys es mayor que la longitud de items. O bien
    //     index y length no especifican un intervalo válido en el objeto System.Array de
    //     keys. O bien items no es null, y index y length no especifican un intervalo válido
    //     en el objeto System.Array de items.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos del objeto System.Array de keys no implementan la interfaz
    //     System.IComparable.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array keys, Array items, int index, int length)
    {
        Sort(keys, items, index, length, null);
    }

    //
    // Resumen:
    //     Ordena los elementos de una matriz System.Array unidimensional usando la interfaz
    //     System.Collections.IComparer especificada.
    //
    // Parámetros:
    //   array:
    //     Matriz unidimensional que se va a ordenar.
    //
    //   comparer:
    //     Implementación que se va a usar al comparar elementos. O bien null para usar
    //     la implementación de System.IComparable de cada elemento.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y uno o más elementos de la array no implementan la interfaz
    //     de System.IComparable.
    //
    //   T:System.ArgumentException:
    //     La implementación de comparer produjo un error durante la ordenación. Por ejemplo,
    //     es posible que comparer no devuelva 0 al comparar un elemento consigo mismo.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array array, IComparer comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        Sort(array, null, array.GetLowerBound(0), array.Length, comparer);
    }

    //
    // Resumen:
    //     Ordena un par de objetos System.Array unidimensionales (uno contiene las claves
    //     y el otro contiene los elementos correspondientes) en función de las claves de
    //     la primera System.Array usando la System.Collections.IComparer. especificada.
    //
    //
    // Parámetros:
    //   keys:
    //     La System.Array unidimensional que contiene las claves que se van a ordenar.
    //
    //
    //   items:
    //     La System.Array unidimensional que contiene los elementos que se corresponden
    //     con cada una de las claves de keysSystem.Array. O bien null para ordenar solo
    //     las keysSystem.Array.
    //
    //   comparer:
    //     Implementación de System.Collections.IComparer que se va a usar al comparar elementos.
    //     O bien null para usar la implementación de System.IComparable de cada elemento.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.RankException:
    //     El objeto System.Array de keys es multidimensional. O bien El objeto System.Array
    //     de items es multidimensional.
    //
    //   T:System.ArgumentException:
    //     items no es null y la longitud de keys es mayor que la longitud de items. O bien
    //     La implementación de comparer produjo un error durante la ordenación. Por ejemplo,
    //     es posible que comparer no devuelva 0 al comparar un elemento consigo mismo.
    //
    //
    //   T:System.InvalidOperationException:
    //     comparer es null, y uno o más elementos del objeto System.Array de keys no implementan
    //     la interfaz de System.IComparable.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array keys, Array items, IComparer comparer)
    {
        if (keys == null)
        {
            throw new ArgumentNullException("keys");
        }

        Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);
    }

    //
    // Resumen:
    //     Ordena los elementos de un intervalo de elementos de una matriz System.Array
    //     unidimensional utilizando la interfaz System.Collections.IComparer especificada.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional que se va a ordenar.
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    //   comparer:
    //     Implementación de System.Collections.IComparer que se va a usar al comparar elementos.
    //     O bien null para usar la implementación de System.IComparable de cada elemento.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.RankException:
    //     array es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array. O bien La implementación
    //     de comparer produjo un error durante la ordenación. Por ejemplo, es posible que
    //     comparer no devuelva 0 al comparar un elemento consigo mismo.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y uno o más elementos de la array no implementan la interfaz
    //     de System.IComparable.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array array, int index, int length, IComparer comparer)
    {
        Sort(array, null, index, length, comparer);
    }

    //
    // Resumen:
    //     Ordena un intervalo de elementos de un par de objetos System.Array unidimensionales
    //     (uno contiene las claves y el otro contiene los elementos correspondientes) en
    //     función de las claves de la primera System.Array usando la interfaz System.Collections.IComparer
    //     especificada.
    //
    // Parámetros:
    //   keys:
    //     La System.Array unidimensional que contiene las claves que se van a ordenar.
    //
    //
    //   items:
    //     La System.Array unidimensional que contiene los elementos que se corresponden
    //     con cada una de las claves de keysSystem.Array. O bien null para ordenar solo
    //     las keysSystem.Array.
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    //   comparer:
    //     Implementación de System.Collections.IComparer que se va a usar al comparar elementos.
    //     O bien null para usar la implementación de System.IComparable de cada elemento.
    //
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.RankException:
    //     El objeto System.Array de keys es multidimensional. O bien El objeto System.Array
    //     de items es multidimensional.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de keys. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     items no es null y el límite inferior de keys no coincide con el límite inferior
    //     de items. O bien items no es null y la longitud de keys es mayor que la longitud
    //     de items. O bien index y length no especifican un intervalo válido en el objeto
    //     System.Array de keys. O bien items no es null, y index y length no especifican
    //     un intervalo válido en el objeto System.Array de items. O bien La implementación
    //     de comparer produjo un error durante la ordenación. Por ejemplo, es posible que
    //     comparer no devuelva 0 al comparar un elemento consigo mismo.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null, y uno o más elementos del objeto System.Array de keys no implementan
    //     la interfaz de System.IComparable.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort(Array keys, Array items, int index, int length, IComparer comparer)
    {
        if (keys == null)
        {
            throw new ArgumentNullException("keys");
        }

        if (keys.Rank != 1 || (items != null && items.Rank != 1))
        {
            throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
        }

        if (items != null && keys.GetLowerBound(0) != items.GetLowerBound(0))
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_LowerBoundsMustMatch"));
        }

        if (index < keys.GetLowerBound(0) || length < 0)
        {
            throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (keys.Length - (index - keys.GetLowerBound(0)) < length || (items != null && index - items.GetLowerBound(0) > items.Length - length))
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
        }

        if (length > 1 && ((comparer != Comparer.Default && comparer != null) || !TrySZSort(keys, items, index, index + length - 1)))
        {
            object[] array = keys as object[];
            object[] array2 = null;
            if (array != null)
            {
                array2 = items as object[];
            }

            if (array != null && (items == null || array2 != null))
            {
                new SorterObjectArray(array, array2, comparer).Sort(index, length);
            }
            else
            {
                new SorterGenericArray(keys, items, comparer).Sort(index, length);
            }
        }
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecurityCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    private static extern bool TrySZSort(Array keys, Array items, int left, int right);

    //
    // Resumen:
    //     Ordena los elementos de toda una matriz System.Array usando la implementación
    //     de la interfaz genérica System.IComparable`1 de cada elemento de System.Array.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero que se va a ordenar.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos de array no implementan la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<T>(T[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        Sort(array, array.GetLowerBound(0), array.Length, null);
    }

    //
    // Resumen:
    //     Ordena un par de objetos System.Array (uno contiene las claves y el otro contiene
    //     los elementos correspondientes) en función de las claves de la primera System.Array
    //     usando la implementación de la interfaz genérica System.IComparable`1 de cada
    //     clave.
    //
    // Parámetros:
    //   keys:
    //     System.Array unidimensional de base cero que contiene las claves que se van a
    //     ordenar.
    //
    //   items:
    //     System.Array unidimensional de base cero que contiene los elementos que se corresponden
    //     con las claves de keys o null para ordenar solo keys.
    //
    // Parámetros de tipo:
    //   TKey:
    //     Tipo de los elementos de la matriz de claves.
    //
    //   TValue:
    //     Tipo de los elementos de la matriz de elementos.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.ArgumentException:
    //     items no es null y el límite inferior de keys no coincide con el límite inferior
    //     de items. O bien items no es null y la longitud de keys es mayor que la longitud
    //     de items.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos del objeto System.Array de keys no implementan la interfaz
    //     genérica System.IComparable`1.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items)
    {
        if (keys == null)
        {
            throw new ArgumentNullException("keys");
        }

        Sort(keys, items, 0, keys.Length, null);
    }

    //
    // Resumen:
    //     Ordena los elementos en un intervalo de elementos en una System.Array mediante
    //     la implementación de interfaz genérica System.IComparable`1 de cada elemento
    //     de la System.Array.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional, basado en cero, que se va a ordenar
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos de array no implementan la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<T>(T[] array, int index, int length)
    {
        Sort(array, index, length, null);
    }

    //
    // Resumen:
    //     Ordena un intervalo de elementos en un par de objetos System.Array (uno contiene
    //     las claves y el otro contiene los elementos correspondientes) en función de las
    //     claves de la primera matriz System.Array usando la implementación de la interfaz
    //     genérica System.IComparable`1 de cada clave.
    //
    // Parámetros:
    //   keys:
    //     System.Array unidimensional de base cero que contiene las claves que se van a
    //     ordenar.
    //
    //   items:
    //     Matriz System.Array unidimensional de base cero que contiene los elementos que
    //     se corresponden con las claves del parámetro keys; o null para ordenar solo keys.
    //
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    // Parámetros de tipo:
    //   TKey:
    //     Tipo de los elementos de la matriz de claves.
    //
    //   TValue:
    //     Tipo de los elementos de la matriz de elementos.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de keys. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     items no es null y el límite inferior de keys no coincide con el límite inferior
    //     de items. O bien items no es null y la longitud de keys es mayor que la longitud
    //     de items. O bien index y length no especifican un intervalo válido en el objeto
    //     System.Array de keys. O bien items no es null, y index y length no especifican
    //     un intervalo válido en el objeto System.Array de items.
    //
    //   T:System.InvalidOperationException:
    //     Uno o más elementos del objeto System.Array de keys no implementan la interfaz
    //     genérica System.IComparable`1.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length)
    {
        Sort(keys, items, index, length, null);
    }

    //
    // Resumen:
    //     Ordena los elementos de una System.Array usando la interfaz genérica System.Collections.Generic.IComparer`1
    //     especificada.
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero que se va a ordenar.
    //
    //   comparer:
    //     La implementación de la interfaz genérica System.Collections.Generic.IComparer`1
    //     que se va a usar al comparar elementos o null para usar la implementación de
    //     la interfaz genérica System.IComparable`1 de cada elemento.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y uno o varios elementos de array no implementan la interfaz
    //     genérica System.IComparable`1.
    //
    //   T:System.ArgumentException:
    //     La implementación de comparer produjo un error durante la ordenación. Por ejemplo,
    //     es posible que comparer no devuelva 0 al comparar un elemento consigo mismo.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<T>(T[] array, IComparer<T> comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        Sort(array, 0, array.Length, comparer);
    }

    //
    // Resumen:
    //     Ordena un par de objetos System.Array (uno contiene las claves y el otro contiene
    //     los elementos correspondientes) en función de las claves de la primera matriz
    //     System.Array usando la interfaz genérica System.Collections.Generic.IComparer`1
    //     especificada.
    //
    // Parámetros:
    //   keys:
    //     System.Array unidimensional de base cero que contiene las claves que se van a
    //     ordenar.
    //
    //   items:
    //     System.Array unidimensional de base cero que contiene los elementos que se corresponden
    //     con las claves de keys o null para ordenar solo keys.
    //
    //   comparer:
    //     La implementación de la interfaz genérica System.Collections.Generic.IComparer`1
    //     que se va a usar al comparar elementos o null para usar la implementación de
    //     la interfaz genérica System.IComparable`1 de cada elemento.
    //
    // Parámetros de tipo:
    //   TKey:
    //     Tipo de los elementos de la matriz de claves.
    //
    //   TValue:
    //     Tipo de los elementos de la matriz de elementos.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.ArgumentException:
    //     items no es null y el límite inferior de keys no coincide con el límite inferior
    //     de items. O bien items no es null y la longitud de keys es mayor que la longitud
    //     de items. O bien La implementación de comparer produjo un error durante la ordenación.
    //     Por ejemplo, es posible que comparer no devuelva 0 al comparar un elemento consigo
    //     mismo.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y uno o más elementos del objeto System.Array de keys no implementan
    //     la interfaz genérica System.IComparable`1.
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, IComparer<TKey> comparer)
    {
        if (keys == null)
        {
            throw new ArgumentNullException("keys");
        }

        Sort(keys, items, 0, keys.Length, comparer);
    }

    //
    // Resumen:
    //     Ordena los elementos de un intervalo de elementos en un elemento System.Array
    //     mediante la interfaz genérica System.Collections.Generic.IComparer`1 especificada.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional de base cero que se va a ordenar.
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    //   comparer:
    //     La implementación de la interfaz genérica System.Collections.Generic.IComparer`1
    //     que se va a usar al comparar elementos o null para usar la implementación de
    //     la interfaz genérica System.IComparable`1 de cada elemento.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de array. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     index y length no especifican un intervalo válido en array. O bien La implementación
    //     de comparer produjo un error durante la ordenación. Por ejemplo, es posible que
    //     comparer no devuelva 0 al comparar un elemento consigo mismo.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y uno o varios elementos de array no implementan la interfaz
    //     genérica System.IComparable`1.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (index < 0 || length < 0)
        {
            throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (array.Length - index < length)
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
        }

        if (length > 1 && ((comparer != null && comparer != Comparer<T>.Default) || !TrySZSort(array, null, index, index + length - 1)))
        {
            ArraySortHelper<T>.Default.Sort(array, index, length, comparer);
        }
    }

    //
    // Resumen:
    //     Ordena un intervalo de elementos de un par de objetos System.Array (uno contiene
    //     las claves y el otro contiene los elementos correspondientes) en función de las
    //     claves de la primera matriz System.Array usando la interfaz genérica System.Collections.Generic.IComparer`1
    //     especificada.
    //
    // Parámetros:
    //   keys:
    //     System.Array unidimensional de base cero que contiene las claves que se van a
    //     ordenar.
    //
    //   items:
    //     Matriz System.Array unidimensional de base cero que contiene los elementos que
    //     se corresponden con las claves del parámetro keys; o null para ordenar solo keys.
    //
    //
    //   index:
    //     Índice inicial del intervalo que se va a ordenar.
    //
    //   length:
    //     Número de elementos del intervalo que se va a ordenar.
    //
    //   comparer:
    //     La implementación de la interfaz genérica System.Collections.Generic.IComparer`1
    //     que se va a usar al comparar elementos o null para usar la implementación de
    //     la interfaz genérica System.IComparable`1 de cada elemento.
    //
    // Parámetros de tipo:
    //   TKey:
    //     Tipo de los elementos de la matriz de claves.
    //
    //   TValue:
    //     Tipo de los elementos de la matriz de elementos.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     keys es null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index es menor que el límite inferior de keys. O bien length es menor que cero.
    //
    //
    //   T:System.ArgumentException:
    //     items no es null y el límite inferior de keys no coincide con el límite inferior
    //     de items. O bien items no es null y la longitud de keys es mayor que la longitud
    //     de items. O bien index y length no especifican un intervalo válido en el objeto
    //     System.Array de keys. O bien items no es null, y index y length no especifican
    //     un intervalo válido en el objeto System.Array de items. O bien La implementación
    //     de comparer produjo un error durante la ordenación. Por ejemplo, es posible que
    //     comparer no devuelva 0 al comparar un elemento consigo mismo.
    //
    //   T:System.InvalidOperationException:
    //     comparer es null y uno o más elementos del objeto System.Array de keys no implementan
    //     la interfaz genérica System.IComparable`1.
    [SecuritySafeCritical]
    [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
    [__DynamicallyInvokable]
    public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, IComparer<TKey> comparer)
    {
        if (keys == null)
        {
            throw new ArgumentNullException("keys");
        }

        if (index < 0 || length < 0)
        {
            throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        }

        if (keys.Length - index < length || (items != null && index > items.Length - length))
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
        }

        if (length > 1 && ((comparer != null && comparer != Comparer<TKey>.Default) || !TrySZSort(keys, items, index, index + length - 1)))
        {
            if (items == null)
            {
                Sort(keys, index, length, comparer);
            }
            else
            {
                ArraySortHelper<TKey, TValue>.Default.Sort(keys, items, index, length, comparer);
            }
        }
    }

    //
    // Resumen:
    //     Ordena los elementos de una System.Array usando el System.Comparison`1 especificado.
    //
    //
    // Parámetros:
    //   array:
    //     System.Array unidimensional, basado en cero, que se va a ordenar
    //
    //   comparison:
    //     System.Comparison`1 que se va a utilizar al comparar elementos.
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien comparison es null.
    //
    //   T:System.ArgumentException:
    //     La implementación de comparison ha producido un error durante la ordenación.
    //     Por ejemplo, es posible que comparison no devuelva 0 al comparar un elemento
    //     consigo mismo.
    [__DynamicallyInvokable]
    public static void Sort<T>(T[] array, Comparison<T> comparison)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (comparison == null)
        {
            throw new ArgumentNullException("comparison");
        }

        IComparer<T> comparer = new FunctorComparer<T>(comparison);
        Sort(array, comparer);
    }

    //
    // Resumen:
    //     Determina si cada elemento de la matriz cumple las condiciones definidas por
    //     el predicado especificado.
    //
    // Parámetros:
    //   array:
    //     Matriz System.Array unidimensional de base cero en la que se van a comprobar
    //     las condiciones.
    //
    //   match:
    //     El predicado que define las condiciones que se van a comprobar en los elementos.
    //
    //
    // Parámetros de tipo:
    //   T:
    //     Tipo de los elementos de la matriz.
    //
    // Devuelve:
    //     true si cada elemento de array cumple las condiciones definidas por el predicado
    //     especificado; de lo contrario, false. Si no hay ningún elemento en la matriz,
    //     el valor devuelto es true.
    //
    // Excepciones:
    //   T:System.ArgumentNullException:
    //     array es null. O bien match es null.
    [__DynamicallyInvokable]
    public static bool TrueForAll<T>(T[] array, Predicate<T> match)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }

        for (int i = 0; i < array.Length; i++)
        {
            if (!match(array[i]))
            {
                return false;
            }
        }

        return true;
    }

    //
    // Resumen:
    //     Inicializa todos los elementos de la matriz System.Array de tipo de valor llamando
    //     al constructor predeterminado del tipo de valor.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [SecuritySafeCritical]
    [__DynamicallyInvokable]
    public extern void Initialize();
}
