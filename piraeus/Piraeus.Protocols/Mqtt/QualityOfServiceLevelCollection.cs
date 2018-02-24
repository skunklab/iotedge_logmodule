
namespace Piraeus.Protocols.Mqtt
{
    using System.Collections.Generic;

    public class QualityOfServiceLevelCollection : IList<QualityOfServiceLevelType>
    {
        public QualityOfServiceLevelCollection()
        {
            this.items = new List<QualityOfServiceLevelType>();
        }

        public QualityOfServiceLevelCollection(IEnumerable<QualityOfServiceLevelType> qosLevels)
        {
            this.items = new List<QualityOfServiceLevelType>(qosLevels);
        }

        private List<QualityOfServiceLevelType> items;

        public int IndexOf(QualityOfServiceLevelType item)
        {
            return this.items.IndexOf(item);
        }

        public void Insert(int index, QualityOfServiceLevelType item)
        {
            this.items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.items.RemoveAt(index);
        }

        public QualityOfServiceLevelType this[int index]
        {
            get { return this.items[index]; }
            set { this.items[index] = value; }
        }

        public void Add(QualityOfServiceLevelType item)
        {
            this.items.Add(item);
        }

        public void Clear()
        {
            this.items.Clear();
        }

        public bool Contains(QualityOfServiceLevelType item)
        {
            return this.items.Contains(item);
        }

        public void CopyTo(QualityOfServiceLevelType[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(QualityOfServiceLevelType item)
        {
            return this.items.Remove(item);
        }

        public IEnumerator<QualityOfServiceLevelType> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
