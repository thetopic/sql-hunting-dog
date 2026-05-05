using System;
using System.Windows;

namespace HuntingDog
{
    public static class Loc
    {
        private static ResourceDictionary _dict = new ResourceDictionary();

        // Fired after a new language is loaded so subscribers can update their own resources.
        public static event Action<string> LanguageChanged;

        // Loads a language, writes its keys into target, then notifies other subscribers.
        public static void Apply(ResourceDictionary target, string lang)
        {
            LoadDict(lang);
            WriteInto(target);
            LanguageChanged?.Invoke(lang);
        }

        // Loads a language and notifies subscribers without writing into any specific target.
        public static void Load(string lang)
        {
            LoadDict(lang);
            LanguageChanged?.Invoke(lang);
        }

        // Writes all translation keys from the current language directly into target.
        // Writing each key individually guarantees WPF fires a DynamicResource change
        // notification per key, which works correctly inside a WinForms ElementHost.
        public static void MergeInto(ResourceDictionary target)
        {
            WriteInto(target);
        }

        public static string T(string key)
        {
            return _dict.Contains(key) ? (string)_dict[key] : key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }

        private static void LoadDict(string lang)
        {
            var assemblyName = typeof(Loc).Assembly.GetName().Name;
            var uri = new Uri(
                $"pack://application:,,,/{assemblyName};component/DogFace/Localization/Strings.{lang}.xaml",
                UriKind.Absolute);
            try
            {
                _dict = new ResourceDictionary { Source = uri };
            }
            catch
            {
                var fallback = new Uri(
                    $"pack://application:,,,/{assemblyName};component/DogFace/Localization/Strings.en.xaml",
                    UriKind.Absolute);
                _dict = new ResourceDictionary { Source = fallback };
            }
        }

        private static void WriteInto(ResourceDictionary target)
        {
            if (_dict.Source == null) return;
            foreach (var key in _dict.Keys)
                target[key] = _dict[key];
        }
    }
}
