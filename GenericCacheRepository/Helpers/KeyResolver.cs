using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GenericCacheRepository.Helpers
{

    public static class KeyResolver
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo> _keyPropertyCache = new();

        public static PropertyInfo? GetKeyProperty<T>() where T : class
        {
            var type = typeof(T);

            // Check cache first
            if (_keyPropertyCache.TryGetValue(type, out var cachedProperty))
                return cachedProperty;

            // Look for [Key] attribute
            var keyProperty = type.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            // If no [Key] found, use the first declared property
            keyProperty ??= type.GetProperties().FirstOrDefault();

            // Store in cache for future use
            if (keyProperty != null)
                _keyPropertyCache[type] = keyProperty;

            return keyProperty;
        }

        public static Expression<Func<T, bool>> GetFetchByKeysLambda<T>(List<object> missingKeys) where T : class
        {
            // Get the primary key property info
            var keyProperty = KeyResolver.GetKeyProperty<T>();
            if (keyProperty == null)
            {
                throw new InvalidOperationException($"No key property found for type {typeof(T).Name}");
            }

            // Convert `missingKeys` to the correct strongly-typed list
            var keyType = keyProperty.PropertyType;
            var typedListType = typeof(List<>).MakeGenericType(keyType);
            var typedKeysList = Activator.CreateInstance(typedListType);

            var addMethod = typedListType.GetMethod("Add");
            foreach (var key in missingKeys)
            {
                var convertedKey = Convert.ChangeType(key, keyType);
                addMethod.Invoke(typedKeysList, new object[] { convertedKey });
            }

            // Use EF.Property<T>() to allow dynamic property access inside LINQ queries
            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyAccess = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                new[] { keyType },
                parameter,
                Expression.Constant(keyProperty.Name)
            );

            var containsMethod = typedListType.GetMethod("Contains", new[] { keyType });
            var containsExpression = Expression.Call(Expression.Constant(typedKeysList), containsMethod, propertyAccess);
            var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
            return lambda;
        }

        public static object? GetPrimaryKey<T>(T entity) where T : class
        {
            var keyProperty = GetKeyProperty<T>();
            return keyProperty?.GetValue(entity);
        }
    }

}
