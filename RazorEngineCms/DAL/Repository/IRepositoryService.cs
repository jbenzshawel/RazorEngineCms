﻿using RazorEngineCms.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngineCms.DAL.Repository
{
    public interface IRepositoryService
    {
        ApplicationContext db { get; set; }

        Task SavePage(Page page, ConcurrentBag<string> errors);

        Task SaveInclude(Include include, ConcurrentBag<string> errors);

        Page FindPage(string section, string name, DateTime? updated = null);

        Include FindInclude(int Id);

        List<Page> AllPages();

        List<Include> AllIncludes();

        Task<Page> CopyPage(Page page, ConcurrentBag<string> errors);

        Task DeletePage(Page page, ConcurrentBag<string> errors);

        Task DeleteInclude(Include page, ConcurrentBag<string> errors);
    }
}