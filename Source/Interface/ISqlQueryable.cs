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

        ISqlQueryable<T> Take(int value);

        ISqlQueryable<T> Skip(int value);

        ISqlQueryable<T> OrderBy(Expression<Func<T, object>> exp);

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
