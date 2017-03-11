using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace UBS.FundManager.Common.Helpers
{
    /// <summary>
    /// Helper Utility for accessing configuration files (i.e web.config or app.config)
    /// </summary>
    public static class ConfigManager
    {
        private static Dictionary<string, object> configSettings = new Dictionary<string, object>();

        public static T GetSetting<T>(string settingName)
        {
            return GetSetting<T>(settingName, true);
        }

        public static T GetSetting<T>(string settingName, T defaultValue)
        {
            return GetSetting<T>(settingName, false, defaultValue);
        }

        public static T GetSetting<T>(string settingName, bool isRequired)
        {
            return GetSetting<T>(null, settingName, isRequired);
        }

        public static T GetSetting<T>(string settingName, bool isRequired, T defaultValue)
        {
            return GetSetting<T>(null, settingName, isRequired, defaultValue);
        }

        public static T GetSetting<T>(Type typeInSourceAssembly, string settingName, bool isRequired)
        {
            return GetSetting<T>(typeInSourceAssembly, settingName, isRequired, default(T));
        }

        public static T GetSetting<T>(Type typeInSourceAssembly, string settingName, bool isRequired, T defaultValue)
        {
            T settingValue = defaultValue;

            if (configSettings.ContainsKey(settingName))
            {
                // get from cached settings
                settingValue = (T)configSettings[settingName];
            }
            else
            {
                string settingStringValue;

                try
                {
                    settingStringValue = GetStringValueFromSetting(settingName, typeInSourceAssembly);
                }
                catch (ConfigurationErrorsException ex)
                {

                    throw new ConfigurationSettingsException(string.Format("Unable to read the setting '{0}' from the configuration file", settingName), ex);
                }

                if (settingStringValue == null && isRequired)
                {
                    throw new ConfigurationSettingsException(string.Format("The setting '{0}' is missing from the configuration file", settingName));
                }
                else if (settingStringValue != null)
                {
                    try
                    {
                        if (typeof(T).IsSubclassOf(typeof(Enum)))
                        {
                            settingValue = (T)Enum.Parse(typeof(T), settingStringValue);
                        }
                        else if (typeof(T) == typeof(DateTime))
                        {
                            settingValue = (T)(object)DateTime.Parse(settingStringValue);
                        }
                        else
                        {
                            settingValue = (T)Convert.ChangeType(settingStringValue, typeof(T));
                        }
                    }
                    catch (FormatException ex)
                    {
                        throw new ConfigurationSettingsException(string.Format("The configuration file setting '{0}' is in an invalid format", settingName), ex);
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new ConfigurationSettingsException(string.Format("The configuration file setting '{0}' is in an invalid format", settingName), ex);
                    }

                    // There's a slight chance of a race condition here. Catch and ignore the resulting exception
                    try
                    {
                        configSettings.Add(settingName, settingValue);
                    }
                    catch (ArgumentException)
                    { }
                }
            }

            return settingValue;
        }

        private static string GetStringValueFromSetting(string settingName, Type typeInSourceAssembly)
        {
            if (typeInSourceAssembly == null)
            {
                return ConfigurationManager.AppSettings[settingName];
            }
            else
            {
                // load config document for current assembly
                XmlDocument doc = LoadConfigDocument(typeInSourceAssembly);

                // retrieve appSettings node
                XmlNode node = doc.SelectSingleNode("//appSettings");

                if (node == null)
                {
                    throw new InvalidOperationException("appSettings section not found in config file.");
                }

                // select the 'add' element that contains the key
                XmlElement elem = (XmlElement)node.SelectSingleNode(string.Format("//add[@key='{0}']", settingName));

                if (elem != null)
                {
                    return elem.GetAttribute("value");
                }
            }

            // Could'nt find the setting
            return null;
        }

        private static XmlDocument LoadConfigDocument(Type typeInSourceAssembly)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(GetConfigFilePath(typeInSourceAssembly));

            return doc;
        }

        private static string GetConfigFilePath(Type typeInSourceAssembly)
        {
            return $"{ Assembly.GetAssembly(typeInSourceAssembly).Location }.config";
        }
    }

    [Serializable]
    public class ConfigurationSettingsException : Exception
    {
        public ConfigurationSettingsException() { }
        public ConfigurationSettingsException(string message) : base(message) { }
        public ConfigurationSettingsException(string message, Exception ex) : base(message, ex) { }
        protected ConfigurationSettingsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
