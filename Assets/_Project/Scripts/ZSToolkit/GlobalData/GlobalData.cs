using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ZSToolkit.GlobalData.Extensions;

namespace ZSToolkit.GlobalData
{
    public static class GlobalData
    {
        public static readonly string dataPath = $"{Application.persistentDataPath}/data";

        private static void DeleteFile(string path)
        {
            File.Delete(path);

            var directory = Path.GetDirectoryName(path).Replace(@"\", "/");
            if (directory == Application.persistentDataPath || directory == dataPath) return;

            if (Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length == 0) Directory.Delete(directory);
        }

        /// <summary>
        /// creates a new .globaldata file
        /// </summary>
        /// <param name="path">local path of .globaldata file</param>
        /// <exception cref="ArgumentException"></exception>
        public static void New(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, "{}");
        }

        /// <summary>
        /// erases every .globaldata file
        /// </summary>
        public static void Erase()
        {
            foreach (var file in Directory.GetFiles(dataPath, "*.globaldata", SearchOption.AllDirectories))
            {
                DeleteFile(file);
            }
        }

        /// <summary>
        /// erases .globaldata file
        /// </summary>
        /// <param name="path">local path of .globaldata file</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Erase(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");

            if (!path.EndsWith("/"))
            {
                var jsonPath = $"{dataPath}/{path}.globaldata";
                if (!File.Exists(jsonPath)) throw new ArgumentException($"global data \"{path}\" not found");

                DeleteFile(jsonPath);
            }
            else
            {
                var directory = $"{dataPath}/{path}";
                foreach (var file in Directory.GetFiles(directory, "*.globaldata", SearchOption.AllDirectories))
                {
                    DeleteFile(file);
                }
            }
        }

        /// <summary>
        /// erases key from .globaldata file
        /// </summary>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Erase(string path, string key)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            if (!File.Exists(jsonPath)) throw new ArgumentException($"global data \"{path}\" not found");

            var json = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(json)) throw new ArgumentException($"globaldata at \"{path}\" is empty");

            var isEncrypted = json[0] != '{';
            if (isEncrypted) json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            jsonObject.Remove(key);

            var data = isEncrypted ? EncryptOrDecrypt(jsonObject.ToString()) : jsonObject.ToString();
            File.WriteAllText(jsonPath, data);
        }

        /// <summary>
        /// saves value to .globaldata file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <param name="value">value to save</param>
        /// <param name="encrypt">should .globaldata file be encrypted</param>
        /// <exception cref="ArgumentException"></exception>
        // представь
        // зима
        // ты гуляешь в девочкой которая тебе нравится
        // и тут ты видишь
        // прям пиздец какой ахуенный лед
        // ну и просто не можешь пройти мимо
        // решаешь проехать на нем
        // и падаешь нахуй
        // в сугроб
        // лежишь блять
        // настолько в снегу
        // что даже если бы был черножопым то стал бы человеком
        // ну и она думает
        // ну а хули
        // пойду тоже в сугроб ебнусь
        // она падает
        // И ТЫ РЕЗКО ВСТАЕШЬ
        // ХВАТАЕШЬ АРМАТУРУ
        // И ХУЯРИШЬ
        // потом она сдохла
        public static void Save<T>(string path, string key, T value, bool encrypt = true)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            var json = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "{}";
            if (string.IsNullOrEmpty(json)) json = "{}";
            else if (json[0] != '{') json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            jsonObject[key] = JToken.FromObject(value);

            var data = encrypt ? EncryptOrDecrypt(jsonObject.ToString()) : jsonObject.ToString();

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, data);
        }

        /// <summary>
        /// loads value from .globaldata file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <returns>value of json property</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T Load<T>(string path, string key)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            if (!File.Exists(jsonPath)) throw new ArgumentException($"global data \"{path}\" not found");

            var json = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(json)) throw new ArgumentException($"globaldata at \"{path}\" is empty");
            else if (json[0] != '{') json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            if (!jsonObject.ContainsKey(key))
            {
                throw new ArgumentException($"cant load \"{key}\" at \"{path}\" because it does not exist");
            }

            try
            {
                var obj = jsonObject[key].ToObject<T>();
                return obj;
            }
            catch
            {
                throw new ArgumentException($"cant load {jsonObject[key].Type} \"{key}\" as {typeof(T).GenericToString()}");
            }
        }

        /// <summary>
        /// loads value from .globaldata file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <param name="defaultValue">value to return if property was not found</param>
        /// <returns>value of json property</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T Load<T>(string path, string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            if (!File.Exists(jsonPath)) return defaultValue;

            var json = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(json)) throw new ArgumentException($"globaldata at \"{path}\" is empty");
            else if (json[0] != '{') json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            if (!jsonObject.ContainsKey(key))
            {
                return defaultValue;
            }

            try
            {
                var obj = jsonObject[key].ToObject<T>();
                return obj;
            }
            catch
            {
                throw new ArgumentException($"cant load {jsonObject[key].Type} \"{key}\" as {typeof(T).GenericToString()}");
            }
        }

        private static string EncryptOrDecrypt(string data)
        {
            var output = "";
            for (int i = 0; i < data.Length; i++)
            {
                output += (char)(data[i] ^ ZSToolkitGlobalDataEncryptionKey.encryptionKey[i % ZSToolkitGlobalDataEncryptionKey.encryptionKey.Length]);
            }

            return output;
        }
    }
}