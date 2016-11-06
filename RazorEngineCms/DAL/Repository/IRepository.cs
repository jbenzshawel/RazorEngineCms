using RazorEngineCms.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngineCms.DAL.Repository
{
    public interface IRepository<T>
    {
        Page Find(string section, string name, DateTime? updated = null);

        T Find(int Id);

        List<T> All();

        Task Save(T obj, ConcurrentBag<string> errors);

        Task<T> Copy(T page, ConcurrentBag<string> errors);

        Task Delete(T obj, ConcurrentBag<string> errors);

    }
}
