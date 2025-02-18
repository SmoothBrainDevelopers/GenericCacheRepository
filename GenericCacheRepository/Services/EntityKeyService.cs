using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GenericCacheRepository.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class EntityKeyService: IEntityKeyService
    {
        /// <summary>
        /// Retrieves the primary key properties of the given entity type from the DbContext.
        /// </summary>
        public IReadOnlyList<PropertyInfo> GetPrimaryKeys<T>(DbContext context) where T : class
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} is not mapped in the DbContext.");

            return entityType.FindPrimaryKey()?.Properties
                .Select(p => p.PropertyInfo)
                .Where(p => p != null)
                .ToList() ?? new List<PropertyInfo>();
        }

        /// <summary>
        /// Retrieves foreign key properties of the given entity type from the DbContext.
        /// </summary>
        public IReadOnlyList<PropertyInfo> GetForeignKeys<T>(DbContext context) where T : class
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} is not mapped in the DbContext.");

            return entityType.GetForeignKeys()
                .SelectMany(fk => fk.Properties)
                .Select(p => p.PropertyInfo)
                .Where(p => p != null)
                .ToList() ?? new List<PropertyInfo>();
        }

        /// <summary>
        /// Constructs a composite key string for an entity using its primary key values.
        /// </summary>
        public string GenerateCompositeKey<T>(DbContext context, T entity) where T : class
        {
            var keyProperties = GetPrimaryKeys<T>(context);
            if (!keyProperties.Any())
                throw new InvalidOperationException($"No primary key found for entity {typeof(T).Name}");

            var keyValues = keyProperties.Select(p => p.GetValue(entity)?.ToString() ?? "NULL");
            return $"{typeof(T).Name}:{string.Join(":", keyValues)}";
        }
    }

}
