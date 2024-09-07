using CsvHelper;
using IPassM.Entities;
using System.Globalization;
using System.Security.Cryptography;

namespace IPassM.Services
{
    public class CsvService
    {
        private readonly string _filePath;
        private readonly byte[] _encryptionKey;

        public CsvService(string filePath, Guid userGuid)
        {
            _filePath = "UserCredential/";
            _encryptionKey = userGuid.ToByteArray();

            if (!Directory.Exists(_filePath))
            {
                Directory.CreateDirectory(_filePath);
            }

            _filePath += filePath + ".csv";
            // Ensure the file is hidden
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
                File.SetAttributes(_filePath, FileAttributes.Hidden);
            }
        }

        public void AddEntry(Guid credentialId, string websiteName, string userName, string password)
        {
            var entries = ReadEntries();
            var newEntry = new Credential
            {
                CredentialId = credentialId,
                WebsiteName = Encrypt(websiteName),
                UserName = Encrypt(userName),
                Password = Encrypt(password)
            };
            entries.Add(newEntry);
            WriteEntries(entries);
        }

        public void UpdateEntry(string websiteName, string newUserName, string newPassword)
        {
            var entries = ReadEntries();
            var entry = entries.FirstOrDefault(e => Decrypt(e.WebsiteName) == websiteName);
            if (entry != null)
            {
                entry.UserName = Encrypt(newUserName);
                entry.Password = Encrypt(newPassword);
                WriteEntries(entries);
            }
        }

        public void DeleteEntry(Guid CredentialId)
        {
            var entries = ReadEntries();
            var entry = entries.FirstOrDefault(e => e.CredentialId == CredentialId);
            if (entry != null)
            {
                entries.Remove(entry);
                WriteEntries(entries);
            }
        }

        public List<Credential> ViewEntries()
        {
            var entries = ReadEntries();
            return entries.Select(e => new Credential
            {
                CredentialId = e.CredentialId == null ? Guid.NewGuid() : e.CredentialId,
                WebsiteName = Decrypt(e.WebsiteName),
                UserName = Decrypt(e.UserName),
                Password = "****"
                //Decrypt(e.Password)
            }).ToList();

        }

        public List<Credential> FilterEntries(string websiteName, string username)
        {
            var entries = ReadEntries();
            return entries
                .Select(e => new Credential
                {
                    CredentialId = e.CredentialId == null ? Guid.NewGuid() : e.CredentialId,
                    WebsiteName = Decrypt(e.WebsiteName),
                    UserName = Decrypt(e.UserName),
                    Password = "****"
                    //Decrypt(e.Password)
                })
                .Where(e =>
                    (string.IsNullOrEmpty(username) || e.UserName.Contains(username, StringComparison.CurrentCultureIgnoreCase)) &&
                    (string.IsNullOrEmpty(websiteName) || e.WebsiteName.Contains(websiteName, StringComparison.CurrentCultureIgnoreCase))
                )
                .ToList();

        }

        public Credential? ViewEntry(Guid CredentialId)
        {
            var entries = ReadEntries();
            return entries
                .Where(a => a.CredentialId == CredentialId)
                .Select(e => new Credential
                {
                    WebsiteName = Decrypt(e.WebsiteName),
                    UserName = Decrypt(e.UserName),
                    Password = "****"
                    //Decrypt(e.Password)
                }).FirstOrDefault();
        }

        public string GetPassword(Guid CredentialId)
        {
            var entries = ReadEntries();
            string password = entries
                .First(a => a.CredentialId == CredentialId)
                .Password;
            return Decrypt(password);
        }

        private List<Credential> ReadEntries()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Credential>();
            }

            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<Credential>().ToList();
            }
        }

        private void WriteEntries(List<Credential> entries)
        {
            Console.Write("_filePath", _filePath);
            if (File.Exists(_filePath))
            {
                FileAttributes attributes = File.GetAttributes(_filePath);

                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    File.SetAttributes(_filePath, attributes & ~FileAttributes.Hidden);
                }

                try
                {

                    using (var writer = new StreamWriter(_filePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(entries);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    File.SetAttributes(_filePath, attributes | FileAttributes.Hidden);
                }
            }
        }

        private string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

}
