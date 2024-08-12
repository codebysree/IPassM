using IPassM.Entities;
using System.Text.Json;

namespace IPassM.Services
{
    public class UserService
    {
        private readonly string filePath = "Credentials/UserCredential.json";

        public async Task AppendUserAsync(UserCredentials user)
        {
            List<UserCredentials> users = await ReadUsersAsync();
            users.Add(user);
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<UserCredentials>> ReadUsersAsync()
        {
            if (!File.Exists(filePath))
            {
                return new List<UserCredentials>();
            }
            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<UserCredentials>>(json) ?? new List<UserCredentials>();
        }

        public async Task<UserCredentials> FindUserAsync(string username, string? password = null)
        {
            List<UserCredentials> users = await ReadUsersAsync();
            if (password is not null)
                return users.FirstOrDefault(user => user.Username == username && user.Password == password);
            else
                return users.FirstOrDefault(user => user.Username == username);
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            List<UserCredentials> users = await ReadUsersAsync();
            var userToDelete = users.FirstOrDefault(user => user.Username == username);
            if (userToDelete != null)
            {
                users.Remove(userToDelete);
                string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            return false;
        }
    }
}
