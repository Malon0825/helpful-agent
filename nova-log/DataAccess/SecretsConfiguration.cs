using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nova_log.DataAccess
{
    public static class SecretsConfiguration
    {
        private static string _jsonFilePath = "secrets.json";
        private static string _jsonContent = File.ReadAllText(_jsonFilePath);
        private static JObject _config = JObject.Parse(_jsonContent);

        public static string? GetOpenAIKey()
        {
            return _config["OpenAIApiKey"] != null
                ? _config["OpenAIApiKey"].ToString()
                : throw new NullReferenceException("Cannot find OpenAI Key");
        }

        public static string GetGithubToken()
        {
            return _config["GitHubToken"] != null
                ? _config["GitHubToken"].ToString()
                : throw new NullReferenceException("Cannot find GitHub Token");
        }

        public static string GetAppName()
        {
            return _config["AppName"] != null
                ? _config["AppName"].ToString()
                : throw new NullReferenceException("Cannot find App Name");
        }

        public static string GetAppVersion()
        {
            return _config["AppVersion"] != null
                ? _config["AppVersion"].ToString()
                : throw new NullReferenceException("Cannot find App Version");
        }

        public static string GetOpenAIModel()
        {
            return _config["OpenAIModel"] != null
                ? _config["OpenAIModel"].ToString()
                : throw new NullReferenceException("Cannot find OpenAI Model");
        }
    }
}
