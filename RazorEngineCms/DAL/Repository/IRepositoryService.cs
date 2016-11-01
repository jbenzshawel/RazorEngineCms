using RazorEngineCms.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngineCms.DAL.Repository
{
    internal interface IRepositoryService
    {
        ApplicationContext db { get; set; }

        Task<ConcurrentBag<string>> SavePage(Page page, ConcurrentBag<string> errors);

        Task<ConcurrentBag<string>> SaveInclude(Include include, ConcurrentBag<string> errors);

        Page FindPage(string section, string name, DateTime? updated = null);

        Include FindInclude(int Id);

        Task<Page> CopyPage(Page page, ConcurrentBag<string> errors);

        Task<ConcurrentBag<string>> DeletePage(Page page, ConcurrentBag<string> errors);
    }
}
