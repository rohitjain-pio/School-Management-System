using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SMSServices.Services
{
    public interface IMessageEncryptionService
    {
        string Encrypt(string plainText, string roomId);
        string Decrypt(string cipherText, string roomId);
        string GenerateKeyForRoom(string roomId, string roomPassword);
    }

    public class MessageEncryptionService : IMessageEncryptionService
    {
        // Master key should be stored securely (e.g., Azure Key Vault, AWS KMS)
        // For now, using configuration
        private readonly byte[] _masterKey;

        public MessageEncryptionService(IConfiguration configuration)
        {
            var masterKeyString = configuration["Encryption:MasterKey"] 
                ?? throw new InvalidOperationException("Encryption master key not configured");
            
            _masterKey = Convert.FromBase64String(masterKeyString);
        }

        public string GenerateKeyForRoom(string roomId, string roomPassword)
        {
            // Derive a unique key for each room using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(
                roomPassword,
                Encoding.UTF8.GetBytes(roomId),
                100000, // iterations
                HashAlgorithmName.SHA256
            );

            return Convert.ToBase64String(pbkdf2.GetBytes(32)); // 256-bit key
        }

        public string Encrypt(string plainText, string roomId)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = DeriveKeyFromRoomId(roomId);
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                
                // Write IV first
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Encryption failed", ex);
            }
        }

        public string Decrypt(string cipherText, string roomId)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = DeriveKeyFromRoomId(roomId);

                // Extract IV
                var iv = new byte[aes.IV.Length];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Decryption failed", ex);
            }
        }

        private byte[] DeriveKeyFromRoomId(string roomId)
        {
            // Derive room-specific key from master key and room ID
            using var hmac = new HMACSHA256(_masterKey);
            var roomIdBytes = Encoding.UTF8.GetBytes(roomId);
            return hmac.ComputeHash(roomIdBytes);
        }
    }
}
