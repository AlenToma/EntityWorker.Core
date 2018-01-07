using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityWorker.Core.InterFace;

namespace EntityWorker.Core.Interface
{
    /// <summary>
    /// quaryProvider for EntityWorker.Core
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISqlQueryable<T>
    {
        /// <summary>
        /// Result of LightDataTable LinqToSql
        /// </summary>
        string ParsedLinqToSql { get; }

        /// <summary>
        /// Add Items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        ISqlQueryable<T> AddRange(List<T> items);

        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ISqlQueryable<T> Add(T item);

        /// <summary>
        /// Ignore loading some children eg ignore Person
        /// IgnoreChildren(x=> x.User.Person)
        /// </summary>
        /// <param name="ignoreActions"></param>
        /// <returns></returns>
        ISqlQueryable<T> IgnoreChildren(params Expression<Func<T, object>>[] ignoreActions);

        /// <summary>
        /// LoadChildren, will load all children herarkie if onlyFirstLevel is not true
        /// </summary>
        /// <param name="onlyFirstLevel"></param>
        /// <returns></returns>
        ISqlQueryable<T> LoadChildren(bool onlyFirstLevel = false);


        /// <summary>
        /// LoadChildren, will load all selected children herarkie eg
        /// LoadChildren(x=> x.User, x=> x.Adress.Select(a=> a.Country)
        /// </summary>
        /// <returns></returns>
        ISqlQueryable<T> LoadChildren(params Expression<Func<T, object>>[] actions);

        /// <summary>
        /// Search 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        ISqlQueryable<T> Where(Expression<Predicate<T>> match);

        /// <summary>
        /// Take only the selected rows 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ISqlQueryable<T> Take(int value);

        /// <summary>
        /// Skip rows
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ISqlQueryable<T> Skip(int value);

        /// <summary>
        /// Order By Column
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISqlQueryable<T> OrderBy(Expression<Func<T, object>> exp);

        /// <summary>
        /// OrderByDescending Column
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISqlQueryable<T> OrderByDescending(Expression<Func<T, object>> exp);

        /// <summary>
        /// Execute the search Command
        /// </summary>
        /// <returns></returns>
        List<T> Execute();

        /// <summary>
        /// Execute the search Command
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ExecuteAsync();

        /// <summary>
        /// Select the top 1
        /// </summary>
        /// <returns></returns>
        T ExecuteFirstOrDefault();

        /// <summary>
        /// Return the count of the executed quary
        /// </summary>
        /// <returns></returns>
        int ExecuteCount();

        /// <summary>
        /// Return the Any of the executed quary
        /// </summary>
        /// <returns></returns>
        bool ExecuteAny();

        /// <summary>
        /// Save All Changes. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        ISqlQueryable<T> Save();

        /// <summary>
        /// Save All Changes. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        ISqlQueryable<T> SaveAll(Func<T, bool> match);

        /// <summary>
        /// Remove All objects herarkie. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        void Remove();

        /// <summary>
        /// Remove All objects herarkie. 
        /// You have to trigger SaveChanges() to commit
        /// </summary>
        /// <returns></returns>
        void RemoveAll(Func<T, bool> match);

        /// <summary>
        /// Commit Changes
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Convert to LightDataTable
        /// </summary>
        /// <returns></returns>
        ILightDataTable ToTable();

        /// <summary>
        /// Convert Object of type a to b 
        /// all properties of B have to be mapped using attribute PropertyName or the propertName of A = B eg a."UserId" = b."UserId" 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        List<TSource> ExecuteAndConvertToType<TSource>() where TSource : class;
    }
}
