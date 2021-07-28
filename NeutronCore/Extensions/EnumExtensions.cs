using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace NeutronCore.Extensions
{
    public static class EnumExtensions
    {
        private static CultureInfo _cultureInfo;
        private static ResourceManager _enumResourceManager;

        static EnumExtensions()
        {
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
        }

        public static string GetEnumDescription<TEnum>(this TEnum value)
            where TEnum : struct
        {
            var type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException(message: "Enumeration Value must be of the Enum Type"
                    , paramName: nameof(value));
            }

            var fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), inherit: false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Gets all items for an enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetAllItems<T>(this Enum value)
        {
            foreach (object item in Enum.GetValues(typeof(T)))
            {
                yield return (T) item;
            }
        }

        //public static Dictionary<int, string> ToDictionary(this Enum value)
        //{
        //    return Enum.GetValues(typeof(value))
        //        .Cast<value>()
        //        .ToDictionary( t => (int)t, t => Enum.GetName(typeof(value), t));

        //}

        public static Dictionary<int, string> EnumToDictionary<T>() where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T is not an Enum type");

            return Enum.GetValues(typeof(T))
                .Cast<object>()
                .ToDictionary(k => (int) k, v => _enumResourceManager.GetString(v.ToString()));
        }

        private static void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _enumResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "EnumDescriptions",
                    resourceDir: languageDirectory, usingResourceSet: null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Enum language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }
    }
}
