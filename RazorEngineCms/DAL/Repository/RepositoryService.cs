using RazorEngineCms.Models;
using System;
using System.Collections.Concurrent;
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

        public async Task<ConcurrentBag<string>> SavePage(Page page, ConcurrentBag<string> errors)
        {
            if (!this._NotUpToDate<Page>(page))
            {
                return await this._PageRepository.Save(page, errors);
            }
            errors.Add("Someone else has updated this page before you");
            return errors;
        }

        public async Task<ConcurrentBag<string>> SaveInclude(Include include, ConcurrentBag<string> errors)
        {
            if(!this._NotUpToDate<Include>(include))
            {
                return await this._IncludeRepository.Save(include, errors);
            }
            errors.Add("Someone else has updated this include before you");
            return errors;

        }

        public async Task<Page> CopyPage(Page page, ConcurrentBag<string> errors)
        {
            return await this._PageRepository.Copy(page, errors);
        }

        public async Task<ConcurrentBag<string>> DeletePage(Page page, ConcurrentBag<string> errors)
        {
            return await this._PageRepository.Delete(page, errors);
        }

        private bool _NotUpToDate<T>(T obj)
        {
            bool status = false;
            if (obj.GetType() == typeof(Page))
            {
                var pageToSave = obj as Page;
                var pageInDb = this.FindPage(pageToSave.Section, pageToSave.Name);
                if (pageToSave.Updated == pageInDb.Updated)
                {
                    status = true;
                }
            }
            else if (obj.GetType() == typeof(Include))
            {
                var includeToSave = obj as Include;
                ///var includeInDb = 
            }
            return status;
        }
    }
}