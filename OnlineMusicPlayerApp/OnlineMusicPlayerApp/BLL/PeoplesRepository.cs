using OnlineMusicPlayerApp.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.BLL
{
    public class PeoplesRepository
    {
        readonly SQLiteAsyncConnection _database;

        public PeoplesRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<PeoplesItem>().Wait();
        }

        public async Task<List<PeoplesItem>> GetPeopelsAsync(Expression<Func<PeoplesItem, bool>> predicate)
        {
            //await _database.DeleteAllAsync<PeoplesItem>();
            return await _database.Table<PeoplesItem>().Where(predicate).ToListAsync();
        }

        public async Task<int> UpdateAsync(PeoplesItem note)
        {
            return await _database.UpdateAsync(note);
        }

        public async Task<PeoplesItem> GetPeopelAsync(Expression<Func<PeoplesItem, bool>> predExpr)
        {
            return await _database.Table<PeoplesItem>()
                            .Where(predExpr)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(PeoplesItem note)
        {
            if (note.Id != 0)
            {
                return await _database.UpdateAsync(note);
            }
            else
            {
                return await _database.InsertAsync(note);
            }
        }

        public async Task<int> CreateAsync(IList<PeoplesItem> peoples)
        {
            return await _database.InsertAllAsync(peoples);
        }

        public async Task<int> DeleteAsync(PeoplesItem note)
        {
            return await _database.DeleteAsync(note);
        }

        public async Task<int> IsGroupName()
        {
            return await _database.Table<PeoplesItem>().CountAsync();
        }

        internal async Task<bool> Any(Expression<Func<PeoplesItem, bool>> predicate)
        {
            return await _database.Table<PeoplesItem>()
                .Where(predicate)
                .CountAsync() > 0 ? true : false;
        }

        public async Task<PeoplesItem> GetAsync(Expression<Func<PeoplesItem, bool>> predicate)
        {
            return await _database.Table<PeoplesItem>()
                .FirstOrDefaultAsync(predicate);
        }
    }
}
