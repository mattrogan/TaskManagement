using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Data;
using TaskManagement.Shared;

namespace TaskManagement.Server.Services
{
    public class ServiceProvider<T> : IServiceProvider<T> 
        where T : BaseEntity
    {
        private readonly TaskContext ctx;

        public ServiceProvider(TaskContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<IEnumerable<T>> GetAsync() 
            => await this.ctx.Set<T>().ToListAsync();

        public async Task<T?> GetSingleAsync(int id)
        {
            var item = await this.ctx.Set<T>().SingleOrDefaultAsync(x => x.Id == id);
            return item;
        }

        public Task<bool> AddAsync(T item)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(T task)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(T task)
        {
            throw new NotImplementedException();
        }
    }
}
