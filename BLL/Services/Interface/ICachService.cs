using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
  
        public interface ICacheService
        {
            Task<T?> GetAsync<T>(string key);
            Task SetAsync<T>(string key, T value, TimeSpan expirationTime);
            Task<bool> RemoveAsync(string key);
        }
    }

