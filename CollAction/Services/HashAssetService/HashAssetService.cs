﻿using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;

namespace CollAction.Services.HashAssetService
{
    public class HashAssetService : IHashAssetService
    {
        private readonly ConcurrentDictionary<string, byte[]> _hashes;
        private readonly bool _useCaching;

        public HashAssetService(bool useCaching)
        {
            _hashes = new ConcurrentDictionary<string, byte[]>();
            _useCaching = useCaching;
        }

        public byte[] HashAsset(string[] location)
        {
            string path = Path.Combine(location);
            if (_useCaching)
            {
                return _hashes.GetOrAdd(path, Hash);
            }
            else
            {
                return Hash(path);
            }
        }

        public string HashAssetBase64(string[] location)
            => Base64UrlEncoder.Encode(HashAsset(location));

        private byte[] Hash(string location)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(location))
                {
                    return sha256.ComputeHash(stream);
                }
            }
        }
    }
}