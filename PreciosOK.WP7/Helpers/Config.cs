using System.IO.IsolatedStorage;

namespace PreciosOK.WP7.Helpers
{
    public static class Config
    {
        public static T Get<T>(string key) where T : class
        {
            T applicationSetting = default(T);
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                applicationSetting = (T)IsolatedStorageSettings.ApplicationSettings[key];
            }
            return applicationSetting;
        }
        
        public static T Get<T>() where T : class
        {
            return Get<T>(typeof(T).ToString());
        }

        public static void Set<T>(T value) where T : class
        {
            Set(typeof(T).ToString(), value);
        }

        public static void Set<T>(object key, T value)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key.ToString()))
            {
                IsolatedStorageSettings.ApplicationSettings[key.ToString()] = value;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(key.ToString(), value);
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
