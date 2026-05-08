using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Message
{
    public class PublishMsg
    {
        public int Num { get; set; }
    }

    public class Config
    {
        public bool Active { get; set; }
    }

    public class ReplyA
    {
        public string Sender { get; set; }
    }

    public class ReplyB
    {
        public string Sender { get; set; }
    }

    public class ReplyAErr
    {
        public string OriginalSender {  get; set; }
        public int AttemptNumber { get; set; }  
        public string ErrorMessage { get; set; }
    }

    public class ReplyBErr
    {
        public string OriginalSender { get; set; }
        public int AttemptNumber { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class EncryptedConfig
    {
        public byte[] Iv { get; set; }
        public byte[] CipherText { get; set; }
    }

    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("198263198263198263198263");

        public static EncryptedConfig Encrypt(Config config)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.GenerateIV();

                var json = JsonConvert.SerializeObject(config);
                var bytes = Encoding.UTF8.GetBytes(json);

                using (var encryptor = aes.CreateEncryptor())
                {
                    var cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                    return new EncryptedConfig
                    {
                        Iv = aes.IV,
                        CipherText = cipher
                    };
                }
            }
        }

        public static Config Decrypt(EncryptedConfig enc)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = enc.Iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var plain = decryptor.TransformFinalBlock(enc.CipherText, 0, enc.CipherText.Length);
                    var json = Encoding.UTF8.GetString(plain);
                    return JsonConvert.DeserializeObject<Config>(json);
                }
            }
        }
    }
}