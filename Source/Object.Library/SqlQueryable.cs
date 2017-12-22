using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityWorker.Core.Interface;
using EntityWorker.Core.InterFace;
using EntityWorker.Core.SqlQuerys;
using EntityWorker.Core.Helper;

namespace EntityWorker.Core.Object.Library
{
    public class SqlQueryable<T> : List<T>, ISqlQueryable<T> where T : class, IDbEntity
    {
        private readonly IRepository _repository;
        private readonly List<string> _ignoreActions = new List<string>();
        private readonly LightDataLinqToNoSql<T> _expression = new LightDataLinqToNoSql<T>();
        private bool _executed = false;
        private readonly bool _partExecuted = false;
        private readonly List<Expression<Func<T, object>>> _childrenToLoad = new List<Expression<Func<T, object>>>();
        private bool? _landholderOnlyFirstLevel;
        private readonly List<Expression> _matches = new List<Expression>();


        internal SqlQueryable(List<T> items, IRepository repository)
        {
            _repository = repository;
            if (items == null)
                return;
            _partExecuted = true;
            items.RemoveAll(x => x == null);
            base.AddRange(items);
        }

        public string ParsedLinqToSql { get; private set; }

        public new ISqlQueryable<T> Add(T item)
        {
            if (item != null)
                base.Add(item);
            return this;
        }

        public ISqlQueryable<T> AddRange(List<T> items)
        {
            if (items == null)
                return this;
            base.AddRange(items.Where(x => x != null));
            return this;
        }

        public ISqlQueryable<T> IgnoreChildren(params Expression<Func<T, object>>[] ignoreActions)
        {
            if (ignoreActions != null)
                _ignoreActions.AddRange(ignoreActions.ConvertExpressionToIncludeList(true));

            return this;
        }

        public ISqlQueryable<T> LoadChildren(bool onlyFirstLevel = false)
        {
            _landholderOnlyFirstLevel = onlyFirstLevel;
            return this;
        }

        public ISqlQueryable<T> LoadChildren(params Expression<Func<T, object>>[] actions)
        {
            if (actions != null)
                _childrenToLoad.AddRange(actions);
            return this;
        }

        public ISqlQueryable<T> Where(Expression<Predicate<T>> match)
        {
            _matches.Add(match);
            return this;
        }

        public ISqlQueryable<T> Take(int value)
        {
            _expression.Take = value;
            return this;
        }

        public ISqlQueryable<T> Skip(int value)
        {
            _expression.Skip = value;
            return this;
        }

        public ISqlQueryable<T> OrderBy(Expression<Func<T, object>> exp)
        {
            var list = Expression.Parameter(typeof(IEnumerable<T>), "list");
            var orderByExp = Expression.Call(typeof(Enumerable), "OrderBy", new Type[] { typeof(T), typeof(object) }, list, exp);
            _matches.Add(orderByExp);
            return this;
        }

        public ISqlQueryable<T> OrderByDescending(Expression<Func<T, object>> exp)
        {
            var list = Expression.Parameter(typeof(IEnumerable<T>), "list");
            var orderByExp = Expression.Call(typeof(Enumerable), "OrderByDescending", new Type[] { typeof(T), typeof(object) }, list, exp);
            _matches.Add(orderByExp);
            return this;
        }

        public async Task<List<T>> ExecuteAsync()
        {
            if (_executed)
                return this.ToList<T>();
            else
            {
                _expression.DataBaseTypes = _repository.DataBaseTypes;
                foreach (var exp in _matches)
                    _expression.Translate(exp);

                ParsedLinqToSql = _expression.Quary;
                if (!_partExecuted)
                    this.AddRange(!string.IsNullOrEmpty(_expression.Quary) ? await _repository.SelectAsync<T>(_expression?.Quary) : await _repository.GetAllAsync<T>());
                if (_childrenToLoad.Any() || _landholderOnlyFirstLevel.HasValue)
                {
                    foreach (var item in this)
                    {
                        if (_childrenToLoad.Any())
                            await _repository.LoadChildrenAsync(item, false, _ignoreActions, _childrenToLoad.ToArray());
                        else await _repository.LoadChildrenAsync(item, _landholderOnlyFirstLevel.Value, null, _ignoreActions);
                    }
                }
                _executed = true;
            }

            return this.ToList<T>();
        }

        public List<T> Execute()
        {
            if (_executed)
                return this.ToList<T>();
            else
            {
                _expression.DataBaseTypes = _repository.DataBaseTypes;
                foreach (var exp in _matches)
                    _expression.Translate(exp);

                ParsedLinqToSql = _expression.Quary;
                if (!_partExecuted)
                    this.AddRange(!string.IsNullOrEmpty(_expression.Quary) ? _repository.Select<T>(_expression?.Quary) : _repository.GetAll<T>());
                if (_childrenToLoad.Any() || _landholderOnlyFirstLevel.HasValue)
                {
                    foreach (var item in this)
                    {
                        if (_childrenToLoad.Any())
                            _repository.LoadChildren(item, false, _ignoreActions, _childrenToLoad.ToArray());
                        else _repository.LoadChildren(item, _landholderOnlyFirstLevel.Value, null, _ignoreActions);
                    }
                }
                _executed = true;
            }

            return this.ToList<T>();
        }


        public ISqlQueryable<T> Save()
        {

            foreach (var item in Execute())
                _repository.Save(item);
            return this;
        }



        public ISqlQueryable<T> SaveAll(Func<T, bool> match)
        {

            foreach (var item in Execute().Where(match))
                _repository.Save(item);
            return this;
        }

        public void Remove()
        {
            foreach (var item in Execute())
                _repository.Delete(item);
            this.Clear();
        }


        public void RemoveAll(Func<T, bool> match)
        {
            foreach (var item in Execute().Where(match))
            {
                _repository.Delete(item);
                base.Remove(item);
            }
            foreach (var item in Execute().Where(match))
                base.Remove(item);
        }

        public void SaveChanges()
        {
            _repository.SaveChanges();
        }

        public ILightDataTable ToTable()
        {
            return new LightDataTable(Execute());
        }

        public List<TSource> ExecuteAndConvertToType<TSource>() where TSource : class
        {
            return Execute().ToType<List<TSource>>();
        }

        public void Dispose()
        {
            _repository?.Dispose();
        }
    }
}
