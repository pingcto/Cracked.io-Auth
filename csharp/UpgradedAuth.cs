using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrackedAuth
{
    internal static class Auth
    {
        private const string KeyFilePath = "key.dat";
        private const string AuthUrl = "https://cracked.to/auth.php";
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task LoginAsync()
        {
            var key = await ReadOrCreateKeyAsync();
            var hwid = GetHwid();
            var response = await AuthenticateAsync(key, hwid);

            if (response.ContainsKey("error"))
            {
                Console.WriteLine($"{DateTime.Now:[hh:mm:ss]} | {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(response["error"].ToString())}.\n\n");
                if (File.Exists(KeyFilePath))
                {
                    File.Delete(KeyFilePath);
                }
                Environment.Exit(0);
            }

            var userGroups = response["group"] as string;
            if (!IsUserPremiumPlus(userGroups))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You have to be at least Premium+ to be able to use this tool. Redirecting to upgrade page...");
                Process.Start(new ProcessStartInfo("http://cracked.to/upgrade.php") { UseShellExecute = true });
                if (File.Exists(KeyFilePath))
                {
                    File.Delete(KeyFilePath);
                }
                Environment.Exit(0);
            }

            Console.WriteLine($"Auth Granted. Welcome {response["username"]}.\n\n");
            await File.WriteAllTextAsync(KeyFilePath, key);
        }

        private static async Task<string> ReadOrCreateKeyAsync()
        {
            if (File.Exists(KeyFilePath))
            {
                return await File.ReadAllTextAsync(KeyFilePath);
            }

            Console.WriteLine("Cracked.to auth? ");
            return Console.ReadLine();
        }

        private static string GetHwid()
        {
            using var sha256 = SHA256.Create();
            var volumeSerialNumber = DriveInfo.GetDrives().First(drive => drive.IsReady).RootDirectory.ToString();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(volumeSerialNumber));
            return BitConverter.ToString(hash).Replace("-", "");
        }

        private static async Task<Dictionary<string, object>> AuthenticateAsync(string key, string hwid)
        {
            var values = new Dictionary<string, string>
            {
                {"a", "auth"},
                {"k", key},
                {"hwid", hwid}
            };

            var content = new FormUrlEncodedContent(values);
            var response = await HttpClient.PostAsync(AuthUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
        }

        private static bool IsUserPremiumPlus(string userGroups)
        {
            var premiumGroups = new HashSet<string> { "11", "12", "93", "96", "97", "99", "100", "101", "4", "3", "6", "94", "92" };
            return premiumGroups.Contains(userGroups);
        }
    }
}
