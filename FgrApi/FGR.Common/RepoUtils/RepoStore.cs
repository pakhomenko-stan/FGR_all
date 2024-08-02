namespace FGR.Common.RepoUtils
{
    public class RepoStore 
    {
        private readonly Dictionary<int, object> store;

        public RepoStore()
        {
            store = [];
        }

        public TEntity Get<TEntity, TTag>(Func<TEntity> createAction)
        {
            var tag = typeof(TTag).GetHashCode();
            if (!store.TryGetValue(tag, out object? value))
            {
                var newEntry = createAction();
                if (newEntry != null) 
                    store.Add(tag, newEntry);
                return newEntry;
            }
            return (TEntity)value;
        }
    }
}
