using Microsoft.EntityFrameworkCore;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;

namespace SuttorLibrary.Core.Repositories
{
    public class GenericRepo<T>(AppDbContext context) 
        : IGenericRepo<T> where T : class
    {
        private readonly AppDbContext _context = context;
        private readonly DbSet<T> _set = context.Set<T>();

        public virtual async Task Add(T entity)
            => await _set.AddAsync(entity);

        public virtual async void Delete(string id)
        {
            var entity = await _set.FindAsync(id.ToString());
            if(entity is not null)
                _set.Remove(entity);
        }

        public virtual async Task<T?> GetById(string id)
            => await _set.FindAsync(Guid.Parse(id));

        public virtual async Task<IEnumerable<T>> GetAll()
        { return await _set.AsNoTracking().ToListAsync(); }

        public virtual void Update(T entity)
        {
            _set.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
