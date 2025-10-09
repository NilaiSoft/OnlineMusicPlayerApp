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
    public class PostMediasRepository
    {
        readonly SQLiteAsyncConnection _database;

        public PostMediasRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<PostMedias>().Wait();
        }

        public async Task<List<PostMedias>> GetsAsync(Expression<Func<PostMedias, bool>> predicate)
        {
            return await _database.Table<PostMedias>().Where(predicate).ToListAsync();
        }

        public async Task<PostMedias> GetAsync(Expression<Func<PostMedias, bool>> predicate)
        {
            return await _database.Table<PostMedias>()
                            .Where(predicate)
                            .FirstOrDefaultAsync();
        }

        internal async Task<bool> Any(Expression<Func<PostMedias, bool>> predicate)
        {
            return await _database.Table<PostMedias>()
                .Where(predicate)
                .CountAsync() > 0 ? true : false;
        }

        public async Task<int> CreateAsync(PostMedias postMedias)
        {
            return await _database.InsertAsync(postMedias);
        }

        public async Task<int> DeleteNoteAsync(PostMedias postMedias)
        {
            return await _database.DeleteAsync(postMedias);
        }
    }
}
