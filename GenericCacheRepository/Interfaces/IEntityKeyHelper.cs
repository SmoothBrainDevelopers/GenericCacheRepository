using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Interfaces
{
    public interface IEntityKeyService
    {
        IReadOnlyList<PropertyInfo> GetPrimaryKeys<T>(DbContext context) where T : class;
        IReadOnlyList<PropertyInfo> GetForeignKeys<T>(DbContext context) where T : class;
        string GenerateCompositeKey<T>(DbContext context, T entity) where T : class;
    }
}
