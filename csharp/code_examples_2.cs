using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Reflection;

namespace CrackedAuth
{
	internal class Auth
	{
		public static void Login()
		{
			if (File.Exists("key.dat"))
			{
				string key = File.ReadAllText("key.dat");
				LoginInternal(key);
			}
			else
			{
				Console.WriteLine("Enter key:");
				string key = Console.ReadLine();
				LoginInternal(key);
			}
		}

		private static void LoginInternal(string key)
		{
			string rootDirectory = GetRootDirectory();
			string volumeSerialNumber = GetVolumeSerialNumber(rootDirectory);
			HttpWebRequest request = CreateRequest(key, volumeSerialNumber);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Dictionary<string, object> responseDictionary = Json.Deserialize(response.GetResponseStream());
			if (responseDictionary.ContainsKey("error"))
			{
				Console.WriteLine("Error: " + responseDictionary["error"]);
				return;
			}
			Console.WriteLine("Welcome " + responseDictionary["username"]);
			File.WriteAllText("key.dat", key);
		}

		private static string GetRootDirectory()
		{
			foreach (DriveInfo drive in DriveInfo.GetDrives())
			{
				if (drive.IsReady)
				{
					return drive.RootDirectory.ToString();
				}
			}
			return null;
		}

		private static string GetVolumeSerialNumber(string rootDirectory)
		{
			using (ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"" + rootDirectory + ":\""))
			{
				disk.Get();
				return disk["VolumeSerialNumber"].ToString();
			}
		}

		private static HttpWebRequest CreateRequest(string key, string volumeSerialNumber)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://cracked.to/auth.php");
			request.Proxy = null;
			request.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			string postData = "a=auth&k=" + key + "&hwid=" + volumeSerialNumber;
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			request.ContentLength = byteArray.Length;
			Stream requestStream = request.GetRequestStream();
			requestStream.Write(byteArray, 0, byteArray.Length);
			return request;
		}
	}

	public static class Json
	{
		public static object Deserialize(string json)
		{
			return Parser.Parse(json);
		}

		private sealed class Parser
		{
			public static object Parse(string json)
			{
				return Parse(json, 0, json.Length);
			}

			private static object Parse(string json, int start, int length)
			{
				if (json[start] == '{')
				{
					return ParseObject(json, start, length);
				}
				else if (json[start] == '[')
				{
					return ParseArray(json, start, length);
				}
				else if (json[start] == '"')
				{
					return ParseString(json, start, length);
				}
				else if (char.IsDigit(json[start]) || json[start] == '-')
				{
					return ParseNumber(json, start, length);
				}
				else if (json[start] == 't')
				{
					return ParseBoolean(json, start, length);
				}
				else
				{
					throw new Exception("Unexpected token");
				}
			}

			private static Dictionary<string, object> ParseObject(string json, int start, int length)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				int index = start + 1;
				while (index < start + length)
				{
					if (json[index] == '}')
					{
						return dictionary;
					}
					string name = ParseString(json, index, length).ToString();
					index += name.Length + 2;
					object value = Parse(json, index, length);
					dictionary.Add(name, value);
					index += 1;
				}
				return dictionary;
			}

			private static List<object> ParseArray(string json, int start, int length)
			{
				List<object> list = new List<object>();
				int index = start + 1;
				while (index < start + length)
				{
					if (json[index] == ']')
					{
						return list;
					}
					object value = Parse(json, index, length);
					list.Add(value);
					index += 1;
				}
									if (json[index] == ',')
					{
						index += 1;
					}
				}
				return list;
			}

			private static string ParseString(string json, int start, int length)
			{
				int index = start + 1;
				while (index < start + length)
				{
					if (json[index] == '"')
					{
						return json.Substring(start + 1, index - start - 1);
					}
					index += 1;
				}
				return null;
			}
			private static double ParseNumber(string json, int start, int length)
			{
				int index = start;
				while (index < start + length)
				{
					if (!char.IsDigit(json[index]) && json[index] != '.' && json[index] != '-')
					{
						return double.Parse(json.Substring(start, index - start));
					}
					index += 1;
				}
				return double.Parse(json.Substring(start, length));
			}

			private static bool ParseBoolean(string json, int start, int length)
			{
				if (json.Substring(start, 4) == "true")
				{
					return true;
				}
				else if (json.Substring(start, 5) == "false")
				{
					return false;
				}
				else
				{
					throw new Exception("Unexpected token");
				}
			}
		}
	}
	}
