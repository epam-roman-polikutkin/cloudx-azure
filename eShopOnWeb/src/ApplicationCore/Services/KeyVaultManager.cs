using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class KeyVaultManager : IKeyVaultManager
{
    private readonly SecretClient _secretClient;

    public KeyVaultManager(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            KeyVaultSecret keyValueSecret = await
            _secretClient.GetSecretAsync(secretName);

            return keyValueSecret.Value;
        }
        catch
        {
            throw;
        }

    }
}
