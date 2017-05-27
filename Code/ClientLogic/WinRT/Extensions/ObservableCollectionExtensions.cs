using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.WinRT.Extensions
{
    /// <summary>
    /// Extensions for ObservableCollection.
    /// Because I am lazy!
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        public static void AddIfDoesNotExist<T>(this ObservableCollection<T> thisCollection, T item)
        {
            if (!thisCollection.Contains(item))
            {
                thisCollection.Add(item);
            }
        }

        public static void InsertFirstIfDoesNotExist<T>(this ObservableCollection<T> thisCollection, T item)
        {
            if (!thisCollection.Contains(item))
            {
                thisCollection.Insert(0, item);
            }
        }

        public static void AddRangeIfDoesNotExists<T>(this ObservableCollection<T> thisCollection, IEnumerable<T> itemsList)
        {
            foreach (var obj in itemsList)
            {
                thisCollection.AddIfDoesNotExist(obj);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> thisCollection, IEnumerable<T> itemsList)
        {
            foreach (var obj in itemsList)
            {
                thisCollection.Add(obj);
            }
        }

    }
}
