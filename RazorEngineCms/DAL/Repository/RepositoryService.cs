using RazorEngineCms.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RazorEngineCms.DAL.Repository
{
    public interface IRepositoryService
    {
        Task<ConcurrentBag<string>> SavePage(Page page, ConcurrentBag<string> errors);

        Page FindPage(string section, string name);

        Include FindInclude(int Id);

        Task<Page> CopyPage(Page page, ConcurrentBag<string> errors);

        Task<ConcurrentBag<string>> DeletePage(Page page, ConcurrentBag<string> errors);
    }

    public class RepositoryService : IRepositoryService
    {
        internal ApplicationContext _db { get; set; }
        internal IRepository<Page> _PageRepository { get; set; }

        internal IRepository<Include> _IncludeRepository {get; set; }

        public RepositoryService(ApplicationContext db)
        {
            this._db = db;
            this._PageRepository = new Repository<Page>(db);
            this._IncludeRepository = new Repository<Include>(db);
        }

        public Page FindPage(string section, string name)
        {
            return this._PageRepository.Find(section, name);
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