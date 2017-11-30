using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityWorker.Core.InterFace;

namespace EntityWorker.Core.Interface
{
    public interface ISqlQueryable<T> where T : class, IDbEntity
    {

        /// <summary>
        /// Result of LightDataTable LinqToSql
        /// </summary>
        string ParsedLinqToSql { get; }

        ISqlQueryable<T> AddRange(List<T> items);

        ISqlQueryable<T> Add(T item);

        ISqlQueryable<T> IgnoreChildren(params Expression<Func<T, object>>[] ignoreActions);

        ISqlQueryable<T> LoadChildren(bool onlyFirstLevel = false);

        ISqlQueryable<T> LoadChildren(params Expression<Func<T, object>>[] actions);

        ISqlQueryable<T> Where(Expression<Predicate<T>> match);

        ISqlQueryable<T> Take(int value);

        ISqlQueryable<T> Skip(int value);

        ISqlQueryable<T> OrderBy(Expression<Func<T, object>> exp);

        ISqlQueryable<T> OrderByDescending(Expression<Func<T, object>> exp);

        List<T> Execute();

        Task<List<T>> ExecuteAsync();

        ISqlQueryable<T> Save();

        ISqlQueryable<T> SaveAll(Func<T, bool> match);

        void Remove();

        void RemoveAll(Func<T, bool> match);

        ILightDataTable ToTable();

        List<TSource> ExecuteAndConvertToType<TSource>() where TSource : class;
    }
}
