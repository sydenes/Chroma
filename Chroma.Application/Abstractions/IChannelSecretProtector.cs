namespace Chroma.Application.Abstractions;

public interface IChannelSecretProtector
{
    string Protect(string plainText);

    string Unprotect(string protectedText);
}
