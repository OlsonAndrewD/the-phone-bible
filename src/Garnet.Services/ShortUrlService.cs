using Garnet.Domain.Repositories;
using Garnet.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garnet.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepository _repository;

        public ShortUrlService(IShortUrlRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> GetLongUrlAsync(string shortCode)
        {
            return await _repository.GetLongUrlAsync(shortCode);
        }

        public async Task<string> GetOrCreateShortCodeAsync(string longUrl)
        {
            var shortCode = await _repository.GetShortCodeAsync(longUrl);
            if (shortCode == null)
            {
                do
                {
                    shortCode = ShortIdGenerator.CreateId();
                }
                while ((await _repository.GetLongUrlAsync(shortCode)) != null);

                await _repository.SetShortCodeAsync(longUrl, shortCode);
            }
            return shortCode;
        }

        private static class ShortIdGenerator
        {
            private const string _characters = "bcdfghjklmnpqrstvwxyz123456789";
            private static Random _randomNumberGenerator = new Random();

            internal static string CreateId(int length = 5)
            {
                return string.Concat(GenerateRandomString(length));
            }

            private static IEnumerable<char> GenerateRandomString(int length)
            {
                var maxIndex = _characters.Length - 1;
                foreach (var i in Enumerable.Range(0, length))
                {
                    var index = _randomNumberGenerator.Next(maxIndex);
                    yield return _characters[index];
                }
            }
        }
    }
}
