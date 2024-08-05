namespace FGR.Common.Interfaces
{
    public interface IRepoStore
    {
        TEntity Get<TEntity, TTag>(Func<TEntity> createAction);
    }
}