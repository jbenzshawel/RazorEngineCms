using RazorEngineCms.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorEngineCms.DAL.Repository
{
    public class RepositoryService : IRepositoryService
    {
        public ApplicationContext db { get; set; }

        internal IRepository<Page> _PageRepository { get; set; }

        internal IRepository<Include> _IncludeRepository {get; set; }

        public RepositoryService(ApplicationContext db)
        {
            this.db = db;
            this._PageRepository = new Repository<Page>(db);
            this._IncludeRepository = new Repository<Include>(db);
        }

        public Page FindPage(string section, string name, DateTime? updated = null)
        {
            return this._PageRepository.Find(section, name, updated);
        }

        public Include FindInclude(int Id)
        {
            return this._IncludeRepository.Find(Id);
        }


        public List<Page> AllPages()
        {
            return this._PageRepository.All();
        }

        public List<Include> AllIncludes()
        {
            return this._IncludeRepository.All();
        }

        public async Task SavePage(Page page, ConcurrentBag<string> errors)
        {
            if (!this._UpdateTimesDiffer<Page>(page))
            {
                await this._PageRepository.Save(page, errors);
            }
            else
            {
                errors.Add("Someone else has updated this page before you");
            }
        }

        public async Task SaveInclude(Include include, ConcurrentBag<string> errors)
        {
            if(!this._UpdateTimesDiffer<Include>(include))
            {
                await this._IncludeRepository.Save(include, errors);
            }
            else
            {
                errors.Add("Someone else has updated this include before you");
            }
        }

        public async Task<Page> CopyPage(Page page, ConcurrentBag<string> errors)
        {
            return await this._PageRepository.Copy(page, errors);
        }

        public async Task DeletePage(Page page, ConcurrentBag<string> errors)
        {
            await this._PageRepository.Delete(page, errors);
        }

        public async Task DeleteInclude(Include include, ConcurrentBag<string> errors)
        {
            await this._IncludeRepository.Delete(include, errors);
        }

        private bool _UpdateTimesDiffer<T>(T obj)
        {
            bool status = true;
            if (obj.GetType() == typeof(Page))
            {
                var pageToSave = obj as Page;
                var pageInDb = this.FindPage(pageToSave.Section, pageToSave.Name);
                if (pageToSave.Updated.ToString() == pageInDb.Updated.ToString())
                {
                    status = false;
                }
            }
            else if (obj.GetType() == typeof(Include))
            {
                var includeToSave = obj as Include;
                var includeInDb = this.FindInclude(includeToSave.Id);
                if (includeInDb.Updated.ToString() == includeToSave.Updated.ToString())
                {
                    status = false;
                }
            }
            return status;
        }
    }
}