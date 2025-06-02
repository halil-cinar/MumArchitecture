using Microsoft.EntityFrameworkCore;
using MumArchitecture.DataAccess.Repository.EntityFramework;
using MumArchitecture.Domain.Abstract;
using System.Linq.Expressions;

namespace MumArchitecture.DataAccess.Abstract
{
    public class EfGenericRepositoryBase<TEntity> : IRepository<TEntity>
        where TEntity : Entity, new()
    {
        private readonly DatabaseContext _context;
        public EfGenericRepositoryBase(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<TEntity> Add(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Added;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<int> Count(Expression<Func<TEntity, bool>>? filter = null)
        {
            var entities = _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false);
            return filter == null
                 ? await entities.AsNoTracking().CountAsync()
                 : await entities.AsNoTracking().CountAsync(filter);
        }

        public async Task<int> Count(DBQuery<TEntity> query)
        {
            var entities = _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false);
            if (query == null)
            {
                return await entities.CountAsync();
            }
            query.Count = int.MaxValue;
            query.Skip = 0;
            return await GetAllWithQuery(query, entities)!.CountAsync();
        }

        public async Task<TEntity?> Delete(Expression<Func<TEntity, bool>> filter)
        {
            var entity = await _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false).FirstOrDefaultAsync(filter);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdateDate = DateTime.UtcNow;
                await Update(entity);
            }
            return entity;
        }
        public async Task<List<TEntity>?> DeleteAll(Expression<Func<TEntity, bool>> filter)
        {
            var entities = await _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false).Where(filter).ToListAsync();
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdateDate = DateTime.UtcNow;
                await Update(entity);
            }

            return entities;
        }

        public Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>>? select = null)
        {
            var entities = _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false);
            return select == null
            ? entities.FirstOrDefaultAsync(filter)
            : entities.Where(filter).Select(select).FirstOrDefaultAsync();

        }
        public async Task<TEntity?> Get(DBQuery<TEntity> query)
        {
            var entities = _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false);
            var data = GetAllWithQuery(query, entities);
            return query == null ? null : await data!.FirstOrDefaultAsync();

        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>>? filter = null)
        {
            var entities = _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false);
            return filter == null
                ? await entities.ToListAsync()
                : await entities.Where(filter).ToListAsync();

        }
        public async Task<List<TEntity>> GetAll(DBQuery<TEntity> query)
        {
            var entities = _context.Set<TEntity>().AsNoTracking().Where(x => x.IsDeleted == false);
            if (query == null)
            {
                return await entities.ToListAsync();
            }
            return await GetAllWithQuery(query, entities)!.ToListAsync();
        }

        public async Task<TEntity> Update(TEntity entity)
        {
            entity.UpdateDate = DateTime.UtcNow;
            var entry = _context.Entry(entity);
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return entity;
        }

        private IQueryable<TEntity>? GetAllWithQuery(DBQuery<TEntity> query, IQueryable<TEntity> entities)
        {
            IQueryable<TEntity> data = entities;
            if (query == null)
            {
                return null;
            }
            if (query.Filter != null && query.Filter.Count() > 0)
            {
                foreach (var filter in query.Filter)
                {
                    data = data.Where(filter);
                }
            }
            if (query.Includes != null)
            {
                foreach (var include in query.Includes)
                {
                    data = data.Include(include!);
                }
            }
            if (query.Order != null)
            {
                if (query.Descending)
                {
                    data = data.OrderByDescending(query.Order);
                }
                else
                {
                    data = data.OrderBy(query.Order);
                }
            }
            if (query.Select != null)
            {
                data = data.Select(query.Select);
            }
            data = data.Skip(query.Skip);
            data = data.Take(query.Count);
            return data;
        }

    }
}
