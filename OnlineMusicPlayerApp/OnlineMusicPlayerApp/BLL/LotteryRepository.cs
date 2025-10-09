using OnlineMusicPlayerApp.Model;
using OnlineMusicPlayerApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.BLL
{
    public class LotteryRepository
    {
        readonly SQLiteAsyncConnection _database;

        public LotteryRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<LotteryItem>().Wait();
        }

        public async Task<List<LotteryItem>> GetAsync()
        {
            return await _database.Table<LotteryItem>()
                .OrderByDescending(x => x.DateTime).ToListAsync();
        }

        public async Task<List<LotteryItem>> GetAllAsync(Expression<Func<LotteryItem,bool>> predicate)
        {
            return await _database.Table<LotteryItem>()
                .Where(predicate)
                .OrderByDescending(x => x.DateTime).ToListAsync();
        }

        public async Task<LotteryItem> GetAsync(int id)
        {
            return await _database.Table<LotteryItem>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(LotteryItem note)
        {
            //if (note.Id != 0)
            //{
            //    return _database.UpdateAsync(note);
            //}
            //else
            //{
            return await _database.InsertAsync(note);
            //}
        }

        public async Task<int> DeleteAsync(LotteryItem note)
        {
            return await _database.DeleteAsync(note);
        }

        public async Task<int> IsGroupName()
        {
            return await _database.Table<LotteryItem>().CountAsync();
        }

        internal async Task<bool> Any(Expression<Func<LotteryItem, bool>> predicate)
        {
            return await _database.Table<LotteryItem>()
                .Where(predicate)
                .CountAsync() > 0 ? true : false;
        }
    }
}
