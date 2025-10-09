using OnlineMusicPlayerApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.BLL
{
    public class LotteryGroupsRepository
    {
        readonly SQLiteAsyncConnection _database;

        public LotteryGroupsRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<LotteryGroups>().Wait();
        }

        public async Task<LotteryGroups> GetAsync(Expression<Func<LotteryGroups, bool>> predicate)
        {
            return await _database.Table<LotteryGroups>()
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<List<LotteryGroups>> GetAllAsync(Expression<Func<LotteryGroups, bool>> predicate)
        {
            return await _database.Table<LotteryGroups>()
                .Where(predicate).ToListAsync();
        }

        internal async Task<bool> Any(Expression<Func<LotteryGroups, bool>> predicate)
        {
            return await _database.Table<LotteryGroups>()
                .Where(predicate)
                .CountAsync() > 0 ? true : false;
        }

        public async Task<int> CreateAsync(LotteryGroups lotteryGroups)
        {
            var isExist = await Any(x => x.Name == lotteryGroups.Name && x.Enabled);
            if (!isExist)
                return await _database.InsertAsync(lotteryGroups);
            return 0;
        }

        public async Task<int> DeleteAsync(LotteryGroups lotteryGroups)
        {
            return await _database.DeleteAsync(lotteryGroups);
        }
    }
}
