using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrackedAuth
{
    internal class Auth
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string AuthUrl = "https://cracked.to/auth.php";
        private const string KeyFilePath = "key.dat";

        public static async Task LoginAsync()
        {
            string key = ReadOrCreateKey();
            string hwid = GetHwid();
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
            else
            {
                Console.WriteLine($"Auth Granted. Welcome {response["username"]}\n\n");
                await File.WriteAllTextAsync(KeyFilePath, key);
            }
        }

        private static string ReadOrCreateKey()
        {
            if (File.Exists(KeyFilePath))
            {
                return File.ReadAllText(KeyFilePath);
            }
            else
            {
                Console.Write("Cracked.to auth? ");
                return Console.ReadLine();
            }
        }

        private static string GetHwid()
        {
            using (var sha256 = SHA256.Create())
            {
                string volumeSerial = GetVolumeSerialNumber();
                byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(volumeSerial));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        private static string GetVolumeSerialNumber()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    return drive.RootDirectory.ToString();
                }
            }
            throw new InvalidOperationException("No ready drives found to determine HWID.");
        }

        private static async Task<Dictionary<string, object>> AuthenticateAsync(string key, string hwid)
        {
            var values = new Dictionary<string, string>
            {
                { "a", "auth" },
                { "k", key },
                { "hwid", hwid }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(AuthUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
        }
    }
}
