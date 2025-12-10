using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly;
using Polly.Caching; // Aqui vive o IAsyncCacheProvider e o Ttl

namespace LauGardensApi
{
    // CORREÇÃO: Usamos IAsyncCacheProvider porque o Redis é Async
    // Removemos o <T> para simplificar a injeção de dependência
    public class PollyRedisAdapt : IAsyncCacheProvider
    {
        private readonly IDistributedCache _cache;

        public PollyRedisAdapt(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Método de Leitura (GET)
        // O Polly pede um 'object', nós vamos ao Redis, trazemos a string e convertemos
        public async Task<(bool, object)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
                    // Deserializamos para 'object' genérico
                    var result = JsonSerializer.Deserialize<object>(value, options);

                    return (true, result);
                }
                catch (Exception)
                {
                    await _cache.RemoveAsync(key, cancellationToken);
                }
            }

            return (false, null);
        }

        // Método de Escrita (PUT)
        public async Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            var options = new DistributedCacheEntryOptions
            {
                // Usamos o Ttl (Time To Live) que vem da política do Polly
                AbsoluteExpirationRelativeToNow = ttl.Timespan
            };

            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            var serialized = JsonSerializer.Serialize(value, jsonOptions);

            await _cache.SetStringAsync(key, serialized, options, cancellationToken);
        }
    }
}