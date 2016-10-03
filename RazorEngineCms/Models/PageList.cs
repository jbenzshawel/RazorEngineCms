using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RazorEngineCms.Models
{
    /// <summary>
    /// Collection of Page objects used for creating Page List view 
    /// </summary>
    [NotMapped]
    public class PageList : IList<Page>
    {
        public int Count
        {
            get
            {
                return this.Pages.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IList<Page> Pages { get; set; }

        public Page this[int index]
        {
            get
            {
                return this.Pages[index];
            }

            set
            {
                this.Pages[index] = value;
            }
        }

        public void Add(Page item)
        {
            if (!Contains(item))
            {
                this.Pages.Add(item);
            }
            else
            {
                var page = this.Pages.FirstOrDefault(p => p.Id == item.Id);
                if (page != null)
                {
                    page = item;
                }
            }
        }

        public void Clear()
        {
            this.Pages.Clear();
        }

        public bool Contains(Page item)
        {
            return this.Pages.Contains(item);
        }

        public void CopyTo(Page[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < this.Pages.Count; i++)
            {
                array[i + arrayIndex] = this.Pages[i];
            }
        }

        public IEnumerator<Page> GetEnumerator()
        {
            return this.Pages.GetEnumerator();
        }

        public bool Remove(Page item)
        {
            return this.Pages.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Pages.GetEnumerator();
        }

        public int IndexOf(Page item)
        {
           return this.Pages.IndexOf(item);
        }

        public void Insert(int index, Page item)
        {
            this.Pages.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.Pages.RemoveAt(index);
        }

        public PageList()
        {
            this.Pages = new List<Page>();
        }
    }

}