using System.Security.Cryptography;
using System.Text;

namespace TechNetworkControlApi.Services;

public class HashServiceSha256
{
    public string GetHash(string inputData)
    {
        using (SHA256 sha256 = new SHA256Managed())
        {
            var data = Encoding.UTF8.GetBytes(inputData);
            var hash = sha256.ComputeHash(data);

            StringBuilder builder = new StringBuilder(128);

            foreach (var b in hash)
            {
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString().ToLower();
        }
    }
}