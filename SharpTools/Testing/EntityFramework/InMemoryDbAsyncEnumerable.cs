using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace SharpTools.Testing.EntityFramework
{
    internal class InMemoryDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public InMemoryDbAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public InMemoryDbAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new InMemoryDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new InMemoryDbAsyncQueryProvider<T>(this); }
        }
    }
}
