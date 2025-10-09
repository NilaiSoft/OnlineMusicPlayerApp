using OnlineMusicPlayerApp.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.BLL
{
    public class InstagramFollowersRepository
    {
        readonly SQLiteAsyncConnection _database;

        public InstagramFollowersRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<InstagramFollowers>().Wait();
        }

        public async Task<List<InstagramFollowers>> GetAllAsync()
        {
            return await _database.Table<InstagramFollowers>().ToListAsync();
        }

        public async Task<List<InstagramFollowers>> GetAllAsync(Expression<Func<InstagramFollowers, bool>> predicate)
        {
            return await _database.Table<InstagramFollowers>().Where(predicate).ToListAsync();
        }

        public async Task<bool> AnyAsync()
        {
            var result = await _database.Table<InstagramFollowers>().CountAsync();
            return result < 1 ? false : true;
        }

        public async Task<bool> AnyAsync(Expression<Func<InstagramFollowers, bool>> predicate)
        {
            var result = await _database.Table<InstagramFollowers>()
                .Where(predicate)
                .CountAsync();
            return result < 1 ? false : true;
        }

        public async Task<InstagramFollowers> GetItemByIdAsync(int id)
        {
            return await _database.Table<InstagramFollowers>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(InstagramFollowers follower)
        {
            return await _database.InsertAsync(follower);
        }

        public async Task<int> CreateRangeAsync(List<InstagramFollowers> followers)
        {
            return await _database.InsertAllAsync(followers);
        }

        public async Task<int> UpdateAsync(InstagramFollowers follower)
        {
            return await _database.UpdateAsync(follower);
        }

        public async Task<int> DeleteAsync(InstagramFollowers note)
        {
            return await _database.DeleteAsync<InstagramFollowers>(note);
        }

        public async Task<int> DeleteAsync(List<InstagramFollowers> followers)
        {
            int res = 0;
            foreach (var item in followers)
            {
                res = await _database.DeleteAsync<InstagramFollowers>(item.Id);
            }
            return res;
        }

        public async Task<int> DeleteAllAsync()
        {
            return await _database.DeleteAllAsync<InstagramFollowers>();
        }

        public async Task<int> IsGroupName()
        {
            return await _database.Table<InstagramFollowers>().CountAsync();
        }
    }
}
