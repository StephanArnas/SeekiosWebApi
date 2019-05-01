using System.Globalization;
using System.Linq;
using WCFServiceWebRole.Enum;

namespace WCFServiceWebRole.Helper
{
    public static class ResourcesHelper
    {
        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        /// <summary>
        /// Get the value from the resource files (.resx) depend on the langage
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="language">langage</param>
        /// <returns>the value associate with the key with the correct langage</returns>
        public static string GetLocalizedString(string key, string language)
        {
            CultureInfo culture = null;

            switch (language)
            {
                default:
                    culture = new CultureInfo("en-GB"); //by default culture = en-GB
                    break;
                case "fr":
                    culture = new CultureInfo("fr-FR");
                    break;
                case "en":
                    culture = new CultureInfo("en-GB");
                    break;
            }
            return Resources.ResourceManager.GetString(key, culture);
        }

        /// <summary>
        /// Get the value from the resource files (.resx) depend on the langage
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="countryCode">country code</param>
        /// <returns>the value associate with the key with the correct langage</returns>
        public static string GetLocalizedString(string key, int countryCode)
        {
            CultureInfo culture = null;

            switch (countryCode)
            {
                default:
                    culture = new CultureInfo("en-GB"); //by default culture = en-GB
                    break;
                case 1:
                    culture = new CultureInfo("fr-FR");
                    break;
                case 2:
                    culture = new CultureInfo("en-GB");
                    break;
            }
            return Resources.ResourceManager.GetString(key, culture);
        }

        /// <summary>
        /// Return the id langage matching with the string langage
        /// </summary>
        /// <param name="language">string langage</param>
        /// <returns>id langage</returns>
        public static int GetCountryResources(string language)
        {
            switch (language)
            {
                default:
                    //by default return english code
                    return (int)CountryCodeEnum.en;
                case "fr":
                    return (int)CountryCodeEnum.fr;
                case "en":
                    return (int)CountryCodeEnum.en;
            }
        }

        /// <summary>
        /// Return the langage of a user depends on the langage device associate
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="idUser">id user</param>
        public static string GetPreferredLanguage(seekios_dbEntities seekiosEntities, int idUser)
        {
            var deviceList = (from d in seekiosEntities.device
                              where d.user_iduser == idUser
                              orderby d.lastUseDate descending
                              select d).ToList();
            if (deviceList?.Count > 0)
            {
                foreach (var device in deviceList)
                {
                    if (device.plateform.Contains("iOS") || device.plateform.Contains("Android"))
                    {
                        return device.countryCode;
                    }
                }
            }
            return "en";
        }

        #endregion
    }
}