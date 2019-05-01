using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WCFServiceWebRole.Data.LOCAL;
using WCFServiceWebRole.Extension;

namespace WCFServiceWebRole.Helper
{
    public static class TriangulationHelper
    {
        #region ----- VARIABLES -------------------------------------------------------------------------------

        private const string _URL_GOOGLE_API_TRIANGULATION = "https://www.googleapis.com/geolocation/v1/geolocate?key=AIzaSyBFvNFSlAybsqwgYdw4I9Jk9JW74M5p8x8";

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        /// <summary>
        /// Get Triangulation location from the google API
        /// </summary>
        /// <param name="cellsData">cells data</param>
        /// <returns></returns>
        public async static Task<TriangulationLocation> GetTriangulationLocation(CellsData cellsData)
        {
            var json = JsonConvert.SerializeObject(cellsData);
            var resultJson = await HttpRequestHelper.PostRequestAsync(_URL_GOOGLE_API_TRIANGULATION, json);
            if (string.IsNullOrEmpty(resultJson) || !resultJson.IsJson()) return null;
            return JsonConvert.DeserializeObject<TriangulationLocation>(resultJson);
        }
        
        /// <summary>
        ///  Deserialize the incomming cells data
        /// </summary>
        /// <param name="cellsDataStr">cells data</param>
        /// <returns></returns>
        public static CellsData DeserializeCellsData(string cellsDataStr)
        {
            var splittedCellsData = cellsDataStr.Split(';');
            if (splittedCellsData.Length < 3) return null;
            try
            {
                var cellsData = new CellsData
                {
                    HomeMobileCountryCode = int.Parse(splittedCellsData[0]),
                    HomeMobileNetworkCode = int.Parse(splittedCellsData[1]),
                    RadioType = "gsm"
                };
                for (int i = 2; i < splittedCellsData.Length; i++)
                {
                    var splittedCellTower = splittedCellsData[i].Split(',');
                    var cellTower = new CellTower()
                    {
                        CellId = long.Parse(splittedCellTower[0], System.Globalization.NumberStyles.HexNumber),
                        LocationAreaCode = int.Parse(splittedCellTower[1], System.Globalization.NumberStyles.HexNumber),
                        MobileCountryCode = int.Parse(splittedCellTower[2]),
                        MobileNetworkCode = int.Parse(splittedCellTower[3])
                    };
                    cellsData.CellTowers.Add(cellTower);
                }
                return cellsData;
            }
            catch (Exception) { }
            return null;
        }

        #endregion
    }
}