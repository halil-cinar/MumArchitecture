using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Services.Common;
using MumArchitecture.DataAccess;
using MumArchitecture.Domain.Abstract;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MumArchitecture.Domain.Converters;

namespace MumArchitecture.Business.Result
{
    public class Filter<TEntity> where TEntity : Entity, new()
    {

        public int Page { get; set; } = 0;
        public int Count { get; set; } = 20;
        protected List<Expression<Func<TEntity, bool>>>? EntityFilter { get; private set; }
        protected List<Expression<Func<TEntity, object?>>>? Includes { get; private set; }
        protected Expression<Func<TEntity, object>>? Order { get; private set; } = x => x.Id;
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
                if (kvp.Key.Equals("page", StringComparison.OrdinalIgnoreCase))
                {
                    filter.Page = int.TryParse(kvp.Value, out var page) ? page : 0;
                    continue;
                }

                if (kvp.Key.Equals("count", StringComparison.OrdinalIgnoreCase))
                {
                    filter.Count = int.TryParse(kvp.Value, out var count) ? count : 50;
                    continue;
                }

                if (kvp.Key.Equals("search", StringComparison.OrdinalIgnoreCase))
                {
                    filter.SearchKey = kvp.Value;
                    continue;
                }

                if (kvp.Key.Equals("format", StringComparison.OrdinalIgnoreCase) && kvp.Value.Equals("select", StringComparison.OrdinalIgnoreCase))
                {
                    filter.All();
                    continue;
                }


                if (kvp.Key.Equals("order", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        filter.Order = Expression.Lambda<Func<TEntity, object>>(
                            Expression.Convert(Expression.Property(parameter, kvp.Value), typeof(object)),
                            parameter);
                    }
                    catch
                    {
                        /* alan bulunamazsa sıralamayı yoksay */
                    }

                    continue;
                }

                if (string.IsNullOrEmpty(kvp.Value))
                    continue;
                var operatorkey = "equal";
                PropertyInfo? property;
                if (kvp.Key.Contains(":"))
                {
                    var parts = kvp.Key.Split(":");
                    if (parts.FirstOrDefault()?.ToLowerInvariant() == "min")
                    {
                        operatorkey = "min";
                    }
                    else if (parts.FirstOrDefault()?.ToLowerInvariant() == "max")
                    {
                        operatorkey = "max";
                    }
                    property = entityType.GetProperty(parts.Count() > 1 ? parts[1] : kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                }
                else
                {
                    property = entityType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                }
                if (property is null)
                    continue;
                var value= kvp.Value;
                if (property.Name.Contains("Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(kvp.Value, out var id))
                    {
                        value= id.ToDatabaseId().ToString();
                    }
                    else
                    {
                        continue; // id alanı integer değilse yoksay
                    }
                }
                try
                {
                    Expression left = Expression.Property(parameter, property);
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    object converted = targetType.IsEnum
                            ? Enum.Parse(targetType, value, true)
                            : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                    var constant = Expression.Constant(converted, targetType);

                    Expression right = targetType != property.PropertyType
                        ? Expression.Convert(constant, property.PropertyType)
                        : constant;


                    Expression body = property.PropertyType == typeof(string)
                        ? Expression.Call(left, nameof(string.Contains), Type.EmptyTypes, right)
                        : operatorkey == "equal" ? Expression.Equal(left, right)
                        : operatorkey == "min" && property.GetType().IsValueType ? Expression.GreaterThanOrEqual(left, right)
                        : operatorkey == "max" && property.GetType().IsValueType ? Expression.LessThanOrEqual(left, right)
                        : Expression.Equal(left, right); // default to equal if no operator matched

                    var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
                    filters.Add(lambda);
                }
                catch
                {
                    /* dönüştürülemeyen değerleri yoksay */
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
        public Filter<TEntity> CopyFrom(Filter<TEntity> filter)
        {
            Count = filter.Count;
            Descending = filter.Descending;
            EntityFilter = filter.EntityFilter;
            Includes = filter.Includes;
            Order = filter.Order;
            Page = filter.Page;
            Select = filter.Select;
            return this;
        }

        public Filter<TEntity> AddSearchKey(params string[] searchparams)
        {
            //if (!string.IsNullOrEmpty(searchKey))
            //{
            //    SearchKey = searchKey;
            //}
            if (searchparams == null || searchparams.Length == 0 || string.IsNullOrEmpty(SearchKey))
            {
                return this;
            }
            var predicate = PredicateBuilder.New<TEntity>(true);
            if (SearchKey != null)
            {
                var searchKeyLower = SearchKey.ToLower();
                foreach (var search in searchparams)
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
