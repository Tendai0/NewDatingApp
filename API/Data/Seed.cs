using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if(await context.users.AnyAsync()) return;
            var userdata = await File.ReadAllTextAsync("Data/UserSeed.json");
            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
            var users =JsonSerializer.Deserialize<List<AppUser>>(userdata,options);

            foreach(var user in users){
                using var hmac = new HMACSHA512();
                user.UserName=user.UserName.ToLower();
                user.PassWordHarsh = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PassWordSalt = hmac.Key;

                context.users.Add(user);
            }
            await context.SaveChangesAsync();
        }
    }
}