using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace MiniMart.Services
{
    public class RsaService
    {
        private readonly ILogger<RsaService> _logger;
        private readonly string _privateKey;
        private readonly RSA _rsa;

        public RsaService(IConfiguration configuration, ILogger<RsaService> logger)
        {
            _logger = logger;

            // Load and cache private key
            //var privateKeyPath = configuration["SecuritySettings:PrivateKey"];
            var keysDirectory = Path.Combine(AppContext.BaseDirectory, "Keys");
            var privateKeyPath = Path.Combine(keysDirectory, "frontend_private_key.pem");
            if (File.Exists(privateKeyPath))
            {
                _privateKey = File.ReadAllText(privateKeyPath);
                _rsa = RSA.Create();
                _rsa.ImportFromPem(_privateKey);
                _logger.LogInformation("Private Key Loaded Successfully.");
            }
            else
            {
                throw new FileNotFoundException("Private key file not found!", privateKeyPath);
            }
        }

        public string Decrypt(byte[] encryptedData)
        {
            try
            {
                var decryptedBytes = _rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "RSA decryption failed");
                throw new SecurityException("Credential decryption failed", ex);
            }
        }
    }
}
