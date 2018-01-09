using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityWorker.Core.Interface;
using EntityWorker.Core.SqlQuerys;
using EntityWorker.Core.Helper;

namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// quaryProvider for EntityWorker.Core
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SqlQueryable<T> : List<T>, ISqlQueryable<T>, IOrderedQueryable<T>
    {
        private readonly Transaction.Transaction _repository;
        private readonly List<string> _ignoreActions = new List<string>();
        private LightDataLinqToNoSql _expression = new LightDataLinqToNoSql(typeof(T));
        private bool _executed = false;
        private readonly bool _partExecuted = false;
        private readonly List<Expression<Func<T, object>>> _childrenToLoad = new List<Expression<Func<T, object>>>();
        private bool? _landholderOnlyFirstLevel;
        private readonly List<Expression> _matches = new List<Expression>();

        public IQueryProvider Provider => _repository;

        internal SqlQueryable(Transaction.Transaction repository, List<T> items)
        {
            _repository = repository;
            if (items == null)
                return;
            _partExecuted = true;
            items.RemoveAll(x => x == null);
            base.AddRange(items);
        }


        internal SqlQueryable(Expression exp, Transaction.Transaction repository)
        {
            _repository = repository;
            if (exp != null)
                _matches.Add(exp);
        }


        /// <summary>
        /// Result of LightDataTable LinqToSql
        /// </summary>
        public string ParsedLinqToSql { get; private set; }

        public Type ElementType => typeof(T);

        public Expression Expression => throw new NotImplementedException();


        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public new ISqlQueryable<T> Add(T item)
        {
            if (item != null)
                base.Add(item);
            return this;
        }

        /// <summary>
        /// Add Items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public ISqlQueryable<T> AddRange(List<T> items)
        {
            if (items == null)
                return this;
            base.AddRange(items.Where(x => x != null));
            return this;
        }

        /// <summary>
        /// Ignore loading some children eg ignore Person
        /// IgnoreChildren(x=> x.User.Person)
        /// </summary>
        /// <param name="ignoreActions"></param>
        /// <returns></returns>
        public ISqlQueryable<T> IgnoreChildren(params Expression<Func<T, object>>[] ignoreActions)
        {
            if (ignoreActions != null)
                _ignoreActions.AddRange(ignoreActions.ConvertExpressionToIncludeList(true));

            return this;
        }
        /// <summary>
        /// LoadChildren, will load all children herarkie if onlyFirstLevel is not true
        /// </summary>
        /// <param name="onlyFirstLevel"></param>
        /// <returns></returns>
        public ISqlQueryable<T> LoadChildren(bool onlyFirstLevel = false)
        {
            _landholderOnlyFirstLevel = onlyFirstLevel;
            return this;
        }

        /// <summary>
        /// LoadChildren, will load all selected children herarkie eg
        /// LoadChildren(x=> x.User, x=> x.Adress.Select(a=> a.Country)
        /// </summary>
        /// <returns></returns>
        public ISqlQueryable<T> LoadChildren(params Expression<Func<T, object>>[] actions)
        {
            if (actions != null)
                _childrenToLoad.AddRange(actions);
            return this;
        }

        /// <summary>
        /// Search 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public ISqlQueryable<T> Where(Expression<Predicate<T>> match)
        {
            _matches.Add(match);
            return this;
        }

        /// <summary>
        /// Take only the selected rows 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISqlQueryable<T> Take(int value)
        {
            _expression.Take = value;
            return this;
        }

        /// <summary>
        /// Skip rows
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISqlQueryable<T> Skip(int value)
        {
            _expression.Skip = value;
            return this;
        }

        /// <summary>
        /// Order By Column
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public ISqlQueryable<T> OrderBy(Expression<Func<T, object>> exp)
        {
            var list = Expression.Parameter(typeof(IEnumerable<T>), "list");
            var orderByExp = Expression.Call(typeof(Enumerable), "OrderBy", new Type[] { typeof(T), typeof(object) }, list, exp);
            _matches.Add(orderByExp);
            return this;
        }

        /// <summary>
        /// OrderByDescending Column
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public ISqlQueryable<T> OrderByDescending(Expression<Func<T, object>> exp)
        {
            var list = Expression.Parameter(typeof(IEnumerable<T>), "list");
            var orderByExp = Expression.Call(typeof(Enumerable), "OrderByDescending", new Type[] { typeof(T), typeof(object) }, list, exp);
            _matches.Add(orderByExp);
            return this;
        }

        /// <summary>
        /// Execute the search Command
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> ExecuteAsync()
        {
            if (_executed)
                return this.ToList<T>();
            else
            {
                var result = new List<T>();
                _expression.DataBaseTypes = _repository.DataBaseTypes;
                foreach (var exp in _matches)
                    _expression.Translate(exp);

                ParsedLinqToSql = _expression.Quary;
                if (!_partExecuted)
                    result.AddRange(await _repository.SelectAsync<T>(ParsedLinqToSql));
                if (_childrenToLoad.Any() || _landholderOnlyFirstLevel.HasValue)
                {
                    foreach (var item in result)
                    {
                        if (_childrenToLoad.Any())
                            await _repository.LoadChildrenAsync(item, false, _ignoreActions, _childrenToLoad.ToArray());
                        else await _repository.LoadChildrenAsync(item, _landholderOnlyFirstLevel.Value, null, _ignoreActions);
                    }
                }
                _executed = true;
                this.AddRange(result);
            }

            return this.ToList<T>();
        }


        public new IEnumerator<T> GetEnumerator()
        {
            return Execute().GetEnumerator();
        }

        /// <summary>
        /// Execute the search Command
        /// </summary>
        /// <returns></returns>
        public List<T> Execute()
        {
            if (_executed)
                return this.ToList<T>();
            else
            {
                var result = new List<T>();

                _expression.DataBaseTypes = _repository.DataBaseTypes;

                foreach (var exp in _matches)
                    _expression.Translate(exp);
                ParsedLinqToSql = _expression.Quary;
                if (!_partExecuted)
                    result.AddRange(_repository.Select<T>(ParsedLinqToSql));
                if (_childrenToLoad.Any() || _landholderOnlyFirstLevel.HasValue)
                {
                    foreach (var item in result)
                    {
                        if (_childrenToLoad.Any())
                            _repository.LoadChildren(item, false, _ignoreActions, _childrenToLoad.ToArray());
                        else _repository.LoadChildren(item, _landholderOnlyFirstLevel.Value, null, _ignoreActions);
                    }
                }
                _executed = true;
                this.AddRange(result);
            }

            return this.ToList<T>();
        }

        /// <summary>
        /// Get Only the top 1
        /// </summary>
        /// <returns></returns>
        public T ExecuteFirstOrDefault()
        {
            if (_executed)
                return this.ToList<T>().FirstOrDefault();
            else
            {
                var result = new List<T>();
                _expression.DataBaseTypes = _repository.DataBaseTypes;
                foreach (var exp in _matches)
                    _expression.Translate(exp);
                ParsedLinqToSql = _expression.QuaryFirst;
                if (!_partExecuted)
                    result.AddRange(_repository.Select<T>(ParsedLinqToSql));
                if (_childrenToLoad.Any() || _landholderOnlyFirstLevel.HasValue)
                {
                    foreach (var item in result)
                    {
                        if (_childrenToLoad.Any())
                            _repository.LoadChildren(item, false, _ignoreActions, _childrenToLoad.ToArray());
                        else _repository.LoadChildren(item, _landholderOnlyFirstLevel.Value, null, _ignoreActions);
                    }
                }
                _executed = true;
                this.AddRange(result);
            }
            return this.ToList<T>().FirstOrDefault();
        }

        /// <summary>
        /// Return the count of the executed quary
        /// </summary>
        /// <returns></returns>
        public int ExecuteCount()
        {
            _expression.DataBaseTypes = _repository.DataBaseTypes;
            foreach (var exp in _matches)
                _expression.Translate(exp);
            ParsedLinqToSql = _expression.Count;
            _expression = new LightDataLinqToNoSql(typeof(T));// reset
            var cmd = _repository.GetSqlCommand(ParsedLinqToSql);
            return _repository.ExecuteScalar(cmd).ConvertValue<int>();
        }

        /// <summary>
        /// Return the Any of the executed quary
        /// </summary>
        /// <returns></returns>
        public bool ExecuteAny()
        {
            return ExecuteCount() > 0;
        }

        /// <summary>
        /// Save All Changes. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        public ISqlQueryable<T> Save()
        {
            foreach (var item in Execute())
                _repository.Save(item);
            return this;
        }

        /// <summary>
        /// Save All Changes. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        public ISqlQueryable<T> SaveAll(Func<T, bool> match)
        {

            foreach (var item in Execute().Where(match))
                _repository.Save(item);
            return this;
        }

        /// <summary>
        /// Remove All objects herarkie. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        public ISqlQueryable<T> Remove()
        {
            foreach (var item in Execute())
                _repository.Delete(item);
            this.Clear();
            return this;
        }

        /// <summary>
        /// Remove All objects herarkie. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        public ISqlQueryable<T> RemoveAll(Func<T, bool> match)
        {
            foreach (var item in Execute().Where(match))
            {
                _repository.Delete(item);
                base.Remove(item);
            }
            foreach (var item in Execute().Where(match))
                base.Remove(item);

            return this;
        }

        /// <summary>
        /// Commit Changes
        /// </summary>
        public void SaveChanges()
        {
            _repository.SaveChanges();
        }

        /// <summary>
        /// Convert to LightDataTable
        /// </summary>
        /// <returns></returns>
        public ILightDataTable ToTable()
        {
            return new LightDataTable(Execute());
        }


        /// <summary>
        /// Convert Object of type a to b 
        /// all properties of B have to be mapped using attribute PropertyName or the propertName of A = B eg a."UserId" = b."UserId" 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public List<TSource> ExecuteAndConvertToType<TSource>() where TSource : class
        {
            return Execute().ToType<List<TSource>>();
        }

        /// <summary>
        /// Dispose the repository
        /// </summary>
        public void Dispose()
        {
            _repository?.Dispose();
        }
    }
}
