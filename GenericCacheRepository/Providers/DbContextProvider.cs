using GenericCacheRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Providers
{
    public class DbContextProvider : IDbContextProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbContext GetDbContext()
        {
            return _serviceProvider.GetRequiredService<DbContext>();
        }
    }

}
