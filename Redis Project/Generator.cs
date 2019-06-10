using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using StackExchange.Redis;
using GraphQL.Client;
using GraphQL.Common.Request;
using Newtonsoft.Json;

namespace Redis_Test
{
    class Generator
    {
        static int numGarbage = 900000;
        static int keySize = 10;
        static int valueSize = 100;
        static void Main(string[] args)
        {
            string filename = "redis_garbage.txt";
            using (System.IO.StreamWriter fs = new System.IO.StreamWriter(filename))
            {
                for (int i = 0; i < numGarbage; i++)
                {
                    string key = random_string(keySize);
                    string value = random_string(valueSize);
                    fs.WriteLine(key);
                    fs.WriteLine(value);
                }
            }
        }

        static string random_string(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}
