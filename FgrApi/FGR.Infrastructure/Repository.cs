using System.Linq.Expressions;
using FGR.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGR.Infrastructure
{
    public class Repository<IEntity, T, TContext>(TContext context) : IRepository<IEntity>, IDisposable
        where T : class, IEntity, new()
        where IEntity : class
        where TContext : DbContext
    {
        protected TContext _context = context;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context?.Dispose();
        }

        public IEntity AddEntity(IEntity entity)
        {
            var entityTrack = _context.Set<T>().Add((T)entity);
            return entityTrack.Entity;
        }

        public ICollection<IEntity> AddEntities(ICollection<IEntity> collection)
        {
            _context.Set<T>().AddRange(collection.Select(e => (T)e));

            return collection;
        }

        public void DeleteEntity(IEntity entity)
        {
            _context.Set<T>().Remove((T)entity);
        }

        public void DeleteEntities(ICollection<IEntity> collection)
        {
            _context.Set<T>().RemoveRange(collection.Select(e => (T)e));
        }

        public IEntity? GetByID(long id)
        {
            return _context.Set<T>().Find(id);
        }

        public List<IEntity> GetEntities()
        {
            return GetEntities(null, string.Empty, null, true);
        }

        public List<IEntity> GetEntities(Expression<Func<IEntity, object>>? orderExpression)
        {
            return GetEntities(null, string.Empty, orderExpression, true);
        }

        public List<IEntity> GetEntities(Expression<Func<IEntity, bool>>? whereExpression)
        {
            return GetEntities(whereExpression, string.Empty, null);
        }

        public List<IEntity> GetEntities(Expression<Func<IEntity, bool>>? whereExpression, string includeExpression)
        {
            return GetEntities(whereExpression, includeExpression, null, true);
        }

        public List<IEntity> GetEntities(Expression<Func<IEntity, bool>>? whereExpression, Expression<Func<IEntity, object>>? orderExpression, bool sortAscending = true)
        {
            return GetEntities(whereExpression, string.Empty, orderExpression, sortAscending);
        }

        public List<IEntity> GetEntities(
            Expression<Func<IEntity, bool>>? whereExpression = null,
            string includeExpression = "",
            Expression<Func<IEntity, object>>? orderExpression = null,
            bool sortAscending = true,
            bool noTracking = false,
            bool includeDeleted = false)
        {
            var query = GetEntitiesQuery(whereExpression, includeExpression, orderExpression, sortAscending, noTracking, includeDeleted);
            List<IEntity> result = [.. query];

            return result;
        }

        public IQueryable<IEntity> GetEntitiesQuery
            (Expression<Func<IEntity, bool>>? whereExpression = null,
            string includeExpression = "",
            Expression<Func<IEntity, object>>? orderExpression = null,
            bool sortAscending = true,
            bool noTracking = false,
            bool includeDeleted = false)
        {
            var query = _context.Set<T>().AsQueryable();
            IQueryable<IEntity> q;

            if (!string.IsNullOrEmpty(includeExpression))
            {
                foreach (var property in includeExpression.Split(','))
                    query = query.Include(property.Trim()).AsQueryable();
            }

            q = noTracking ? query.AsNoTracking() : query;

            if (whereExpression != null)
                q = query.Where(whereExpression).AsQueryable();

            if (orderExpression != null)
                q = sortAscending ? q.OrderBy(orderExpression).AsQueryable() : q.OrderByDescending(orderExpression).AsQueryable();

            return q;
        }

        public int ExecuteWithStoreProcedure(string query, params object[] parameters)
        {
            int res = _context.Database.ExecuteSqlRaw(query, parameters);

            return res;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Reload<T1>(T1 entity) where T1 : class
        {
            _context.Entry(entity).Reload();
        }

        public List<IEntity> GetEntitiesNoTracking()
        {
            var result = _context.Set<T>().AsNoTracking();
            var convertedResult = result?.ToList<IEntity>() ?? [];
            return convertedResult;
        }

        public List<IEntity> GetEntitiesNoTracking(Expression<Func<IEntity, bool>>? whereExpression)
        {
            return [.. GetEntitiesQuery(whereExpression: whereExpression, noTracking: true)];
        }

        public IEntity Update(IEntity entity)
        {
            var entityTrack = _context.Set<T>().Update((T)entity);
            return entityTrack.Entity;
        }

        public IEnumerable<IEntity> ExecuteQuery(string sql, params object[] parameters)
        {
            return _context.Set<T>().FromSqlRaw(sql, parameters);
        }

        public async Task Transaction(Func<IRepository<IEntity>, Action<string>?, Task> func)
        {
            using var transaction = _context.Database.BeginTransaction();
            string savePoint = string.Empty;
            try
            {
                await func(this, sp => { transaction.CreateSavepoint(sp); savePoint = sp; });
                transaction.Commit();
            }
            catch (Exception)
            {
                if (string.IsNullOrEmpty(savePoint))
                {
                    transaction.RollbackToSavepoint(savePoint);
                }
                else
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }


        public async Task<IEntity?> GetByIDAsync(long id, CancellationToken token)
        {
            return await _context.Set<T>().FindAsync([id], cancellationToken: token);
        }

        public async Task<string> SaveAsync(CancellationToken token)
        {
            await _context.SaveChangesAsync(token);
            var savePoint = $"savePoint_{Guid.NewGuid()}";
            return savePoint;
        }

        public async Task<IEntity?> AddEntityAsync(IEntity entity, CancellationToken token)
        {
            var entityTrack = await _context.Set<T>().AddAsync((T)entity, token);
            return entityTrack.Entity;
        }

        public async Task<ICollection<IEntity>?> AddEntitiesAsync(ICollection<IEntity> collection, CancellationToken token)
        {
            await _context.Set<T>().AddRangeAsync(collection.Select(e => (T)e), token);
            return collection;
        }

        public async Task<ICollection<IEntity>?> GetEntitiesAsync(Expression<Func<IEntity, bool>>? whereExpression = null,
            string includeExpression = "",
            Expression<Func<IEntity, object>>? orderExpression = null,
            bool sortAscending = true,
            bool noTracking = false,
            bool includeDeleted = false,
            CancellationToken token = default)
        {
            var query = GetEntitiesQuery(whereExpression, includeExpression, orderExpression, sortAscending, noTracking, includeDeleted);
            List<IEntity> result = await query.ToListAsync(token);
            return result;
        }
    }
}

