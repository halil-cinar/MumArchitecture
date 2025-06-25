using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Services.Common;
using NLog.Filters;
using MumArchitecture.DataAccess;
using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Result
{
    public class Filter<TEntity> where TEntity : Entity, new()
    {

        public int Page { get; set; } = 0;
        public int Count { get; set; } = 50;
        protected List<Expression<Func<TEntity, bool>>>? EntityFilter { get; private set; }
        protected List<Expression<Func<TEntity, object?>>>? Includes { get; private set; }
        protected Expression<Func<TEntity, object>>? Order { get; private set; }
        protected Expression<Func<TEntity, TEntity>>? Select { get; private set; }
        public bool Descending { get; private set; } = false;
        public string? SearchKey { get; set; }


        public static Filter<TEntity> ConvertFilter(Dictionary<string, string> httpQuery)
        {
            var filter = new Filter<TEntity>();
            var filters = new List<Expression<Func<TEntity, bool>>>();
            var entityType = typeof(TEntity);
            var parameter = Expression.Parameter(entityType, "x");

            foreach (var kvp in httpQuery)
            {
                if (kvp.Key.Equals("page", StringComparison.OrdinalIgnoreCase) || kvp.Key.Equals("count", StringComparison.OrdinalIgnoreCase))
                {
                    if (kvp.Key.Equals("page", StringComparison.OrdinalIgnoreCase))
                    {
                        filter.Page = int.TryParse(kvp.Value, out int page) ? page : 0;
                    }
                    else if (kvp.Key.Equals("count", StringComparison.OrdinalIgnoreCase))
                    {
                        filter.Count = int.TryParse(kvp.Value, out int count) ? count : 50;
                    }
                }
                if (kvp.Key.Equals("search", StringComparison.OrdinalIgnoreCase))
                {
                    filter.SearchKey = kvp.Value;
                }
                if (kvp.Key.Equals("order", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        filter.Order = Expression.Lambda<Func<TEntity, object>>(Expression.Property(parameter, kvp.Value), parameter);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    continue;
                }
                var property = entityType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    try
                    {
                        object convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);

                        //x => x.Property == convertedValue
                        Expression left = Expression.Property(parameter, property);
                        Expression right = Expression.Constant(convertedValue, property.PropertyType);
                        Expression equality = Expression.Equal(left, right);
                        var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

                        filters.Add(lambda);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            if (filters.Count > 0)
            {
                filter.EntityFilter = filters;
            }
            return filter;
        }

        public static Filter<TEntity> CreateFilter(params Expression<Func<TEntity, bool>>[] filter)
        {
            var f = new Filter<TEntity>();
            f.EntityFilter = filter.ToList();
            return f;
        }
        public Filter<TEntity> AddFilter(params Expression<Func<TEntity, bool>>[] filter)
        {
            if (EntityFilter == null)
            {
                EntityFilter = filter.ToList();
            }
            else
            {
                EntityFilter.AddRange(filter);
            }
            return this;
        }
        public Filter<TEntity> AddFilter(bool reset, params Expression<Func<TEntity, bool>>[] filter)
        {
            if (EntityFilter == null || reset)
            {
                EntityFilter = filter.ToList();
            }
            else
            {
                EntityFilter.AddRange(filter);
            }
            return this;
        }

        public Filter<TEntity> AddFilterFromIds(params int[] ids)
        {
            var filter = new Expression<Func<TEntity, bool>>[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                filter.Append(x => x.Id == ids[i]);
            }
            if (EntityFilter == null)
            {
                EntityFilter = filter.ToList();
            }
            else
            {
                EntityFilter.AddRange(filter);
            }
            return this;
        }
        public Filter<TEntity> AddFilterFromIds(bool reset, params int[] ids)
        {
            var filter = new Expression<Func<TEntity, bool>>[ids.Length];
            var predicate = PredicateBuilder.New<TEntity>(false);
            foreach (var id in ids)
            {
                var capturedId = id;                            // capture loop variable
                predicate = predicate.Or(x => x.Id == capturedId);
            }
            if (EntityFilter == null || reset)
            {
                EntityFilter = new List<Expression<Func<TEntity, bool>>> { predicate };
            }
            else
            {
                EntityFilter.Add(predicate);
            }
            return this;
        }

        public Filter<TEntity> AddIncludes(params Expression<Func<TEntity, object?>>[]? includes)
        {
            if (Includes == null)
            {
                Includes = includes?.ToList();
            }
            else
            {
                if (includes != null)
                {
                    Includes.AddRange(includes);
                }
            }
            return this;
        }
        public Filter<TEntity> AddOrder(Expression<Func<TEntity, object>> order, bool force = true)
        {
            if (Order == null || force)
            {
                Order = order;
            }
            return this;
        }
        public Filter<TEntity> AddSelect(Expression<Func<TEntity, TEntity>> select)
        {
            Select = select;
            return this;
        }
        public Filter<TEntity> AddDesc()
        {
            Descending = true;
            return this;
        }

        public Filter<TEntity> All()
        {
            Page = 0;
            Count = int.MaxValue;
            return this;
        }
        public Filter<TEntity> GetPage(int page, int count = 50)
        {
            Page = page;
            Count = count;
            return this;
        }

        public Filter<TEntity> Copy()
        {
            return new Filter<TEntity>
            {
                Count = Count,
                Descending = Descending,
                EntityFilter = EntityFilter,
                Includes = Includes,
                Order = Order,
                Page = Page,
                Select = Select
            };
        }

        public Filter<TEntity> AddSearchKey(params string[] searchparams)
        {
            //if (!string.IsNullOrEmpty(searchKey))
            //{
            //    SearchKey = searchKey;
            //}
            if(searchparams == null || searchparams.Length == 0||string.IsNullOrEmpty(SearchKey))
            {
                return this;
            }
            var predicate = PredicateBuilder.New<TEntity>(true);
            if (SearchKey != null)
            {
                var searchKeyLower = SearchKey.ToLower();
                foreach(var search in searchparams)
                {
                    var property = typeof(TEntity).GetProperty(search, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null)
                    {
                        var parameter = Expression.Parameter(typeof(TEntity), "x");
                        var propertyAccess = Expression.Property(parameter, property);
                        var searchValue = Expression.Constant(searchKeyLower, typeof(string));
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var containsExpression = Expression.Call(propertyAccess, containsMethod, searchValue);
                        predicate = predicate.Or(Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter));
                    }
                }
//                predicate = predicate.And(x => search.Invoke(x).Where(x => !string.IsNullOrEmpty(x)).Aggregate((x, acc) => acc + " " + x)!.Contains(searchKeyLower));

            }
            if (EntityFilter == null)
            {
                EntityFilter = new Expression<Func<TEntity, bool>>[] { predicate }.ToList();
            }
            else
            {
                EntityFilter.Add(predicate);
            }
            return this;
        }

        public DBQuery<TEntity> ConvertDbQuery()
        {
            return new DBQuery<TEntity>
            {
                Descending = Descending,
                Includes = Includes?.ToArray(),
                Order = Order,
                Select = Select,
                Filter = EntityFilter?.ToArray(),
                Count = Count,
                Skip = Count * Page,
            };
        }
    }
}
