using System.Linq.Expressions;

namespace FGR.Common.Interfaces
{
    public interface IRepository
    {
    }

    public interface IRepository<IEntity> : IRepository
        where IEntity : class
    {
        void Dispose();
        IEntity AddEntity(IEntity entity);
        ICollection<IEntity> AddEntities(ICollection<IEntity> collection);
        void DeleteEntity(IEntity entity);
        void DeleteEntities(ICollection<IEntity> collection);
        IEntity? GetByID(long id);
        Task<IEntity?> GetByIDAsync(long id);
        List<IEntity> GetEntities();
        List<IEntity> GetEntities(Expression<Func<IEntity, object>>? orderExpression);
        List<IEntity> GetEntities(Expression<Func<IEntity, bool>>? whereExpression);
        List<IEntity> GetEntities(Expression<Func<IEntity, bool>>? whereExpression, string includeExpression);
        List<IEntity> GetEntities(Expression<Func<IEntity, bool>>? whereExpression, Expression<Func<IEntity, object>>? orderExpression, bool sortAscending = true);
        List<IEntity> GetEntities(
            Expression<Func<IEntity, bool>>? whereExpression = null,
            string includeExpression = "",
            Expression<Func<IEntity, object>>? orderExpression = null,
            bool sortAscending = true,
            bool includeDeleted = false);
        IQueryable<IEntity> GetEntitiesQuery(
              Expression<Func<IEntity, bool>>? whereExpression = null,
              string includeExpression = "",
              Expression<Func<IEntity, object>>? orderExpression = null,
              bool sortAscending = true,
              bool noTracking = false,
              bool includeDeleted = false);

        int ExecuteWithStoreProcedure(string query, params object[] parameters);
        void Save();
        Task SaveAsync();
        void Reload<T1>(T1 entity) where T1 : class;
        List<IEntity> GetEntitiesNoTracking();
        List<IEntity> GetEntitiesNoTracking(Expression<Func<IEntity, bool>>? whereExpression);
        IEntity Update(IEntity entity);
        IEnumerable<IEntity> ExecuteQuery(string sql, params object[] parameters);
    }
}
