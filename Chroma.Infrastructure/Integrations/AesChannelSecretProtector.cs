using System.Security.Cryptography;
using System.Text;
using Chroma.Application.Abstractions;
using Chroma.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Chroma.Infrastructure.Integrations;

public sealed class AesChannelSecretProtector(IOptions<IntegrationsOptions> options) : IChannelSecretProtector
{
    private readonly byte[] _key = DeriveKey(options.Value.ChannelSecretKey);

    public string Protect(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var payload = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Unprotect(string protectedText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedText);

        var payload = Convert.FromBase64String(protectedText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        var cipherBytes = new byte[payload.Length - ivLength];

        Buffer.BlockCopy(payload, 0, iv, 0, ivLength);
        Buffer.BlockCopy(payload, ivLength, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] DeriveKey(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Integrations:ChannelSecretKey configuration is required.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));
    }
}
