﻿using System.Collections.Generic;
using System.Linq;
using System.Device.Location;
using Microsoft.Win32;
using System.Diagnostics;

namespace Project1_Group_16.Classes
{
    class Statistics
    {
        // Properties
        internal Dictionary<string, CityInfo> CityCatalogue = new Dictionary<string, CityInfo>();

        // Constructor
        public Statistics(string fileName, string fileType)
        {
            CityCatalogue = DataModeler.ParseFile(fileName, fileType);
        }

        /// <summary>
        /// Get city information based on the city.
        /// </summary>
        /// <param name="city">A string with the name of the city.</param>
        /// <returns>A list with the city information</returns>
        public List<CityInfo> DisplayCityInformation(string city)
        {
            return CityCatalogue.Where(c => c.Key.Contains(city)).Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Gets the largest populous city in a given province.
        /// </summary>
        /// <param name="province">A string with the name of the province</param>
        /// <returns>A CityInfo object with the city information.</returns>
        public CityInfo DisplayLargestPopulationCity(string province)
        {
            return CityCatalogue.Where(city => city.Value.Province == province).OrderByDescending(city => city.Value.Population).First().Value;
        }

        /// <summary>
        /// Gets the smallest populous city in a given province.
        /// </summary>
        /// <param name="province">A string with the name of the province</param>
        /// <returns>A CityInfo object with the city information.</returns>
        public CityInfo DisplaySmallestPopulationCity(string province)
        {
            return CityCatalogue.Where(city => city.Value.Province == province).OrderBy(city => city.Value.Population).First().Value;
        }

        /// <summary>
        /// Compares 2 cities population
        /// </summary>
        /// <param name="city1">A string with the name of city 1.</param>
        /// <param name="province1">A string with the name of province 1.</param>
        /// <param name="city2">A string with the name of city 2.</param>
        /// <param name="province2">A string with the name of province 2.</param>
        /// <returns></returns>
        public (CityInfo, ulong, ulong) CompareCitiesPopulation(string city1, string province1, string city2, string province2)
        {
            return (CityCatalogue[$"{city1}, {province1}"].Population > CityCatalogue[$"{city2}, {province2}"].Population ? CityCatalogue[$"{city1}, {province1}"] : CityCatalogue[$"{city2}, {province2}"], CityCatalogue[city2].Population, CityCatalogue[$"{city1}, {province1}"].Population);
        }

        /// <summary>
        /// Shows city on the map
        /// </summary>
        /// <param name="city">A string with the name of the city.</param>
        /// <param name="province">A string with the name of the province.</param>
        public void ShowCityOnMap(string city, string province)
        {
            Process process = new Process();
            process.StartInfo.FileName = GetStandardBrowserPath();
            process.StartInfo.Arguments = "https://www.google.com/maps/place/" + city + "," + province;
            process.Start();
        }

        /// <summary>
        /// Gets the distance between cities.
        /// </summary>
        /// <param name="city1">A string with the name of city 1.</param>
        /// <param name="province1">A string with the name of province 1.</param>
        /// <param name="city2">A string with the name of city 2.</param>
        /// <param name="province2">A string with the name of province 2.</param>
        /// <returns>A double with the distance between two cities.</returns>
        public double CalculateDistanceBetweenCities(string city1, string province1, string city2, string province2)
        {
            GeoCoordinate city1Coord = new GeoCoordinate(CityCatalogue[$"{city1}, {province1}"].Latitude, CityCatalogue[$"{city1}, {province1}"].Longitude);
            GeoCoordinate city2Coord = new GeoCoordinate(CityCatalogue[$"{city2}, {province2}"].Latitude, CityCatalogue[$"{city2}, {province2}"].Longitude);

            return city1Coord.GetDistanceTo(city2Coord);
        }

        /// <summary>
        /// Gets the province population.
        /// </summary>
        /// <param name="province">A string with the province name.</param>
        /// <returns>A ulong with the population of the province.</returns>
        public ulong DisplayProvincePopulation(string province)
        {
            return (ulong)CityCatalogue.Where(city => city.Value.Province == province.ToLower()).Sum(city => (decimal)city.Value.Population);
        }

        /// <summary>
        /// Get all cities in a certain province.
        /// </summary>
        /// <param name="province">A string with the name of the province.</param>
        /// <returns>A List with the cities information</returns>
        public List<CityInfo> DisplayProvinceCities(string province)
        {
            return CityCatalogue.Values.Where(city => city.Province == province).ToList();
        }

        /// <summary>
        /// Ranks all provinces by population.
        /// </summary>
        /// <returns>A Dictionary with the city name and population.</returns>
        public Dictionary<string, ulong> RankProvincesByPopulation()
        {
            return (from city in CityCatalogue.Values group city by new { city.Province } into province select new { province.Key.Province, Population = (ulong)province.Sum(prov => (decimal)prov.Population) }).OrderBy(prov => prov.Population).ToDictionary(x => x.Province, y => y.Population);
        }

        /// <summary>
        /// Ranks provinces on how many cities in each province.
        /// </summary>
        /// <returns>A Dictionary with the province name and count of cities.</returns>
        public Dictionary<string, int> RankProvincesByCities()
        {
            return (from city in CityCatalogue.Values group city by new { city.Province } into province select new { province.Key.Province, Count = province.Count() }).OrderBy(prov => prov.Count).ToDictionary(x => x.Province, y => y.Count);
        }

        /// <summary>
        /// Gets capital of a province.
        /// </summary>
        /// <param name="province">A string with the name of the province.</param>
        /// <returns>A string with the capital name, a double with latitude and a double with longitude.</returns>
        public (string, double, double) GetCapital(string province)
        {
            CityInfo city = CityCatalogue.Values.Where(city => city.Capital == province).First();
            return (city.CityName, city.Latitude, city.Longitude);
        }

        /// <summary>
        /// Gets the path to the default browser.
        /// </summary>
        /// <returns>A string with the path.</returns>
        private string GetStandardBrowserPath()
        {
            string browserPath = string.Empty;
            RegistryKey browserKey = null;

            try
            {
                //Read default browser path from Win XP registry key
                browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                //If browser path wasn't found, try Win Vista (and newer) registry key
                if (browserKey == null)
                {
                    browserKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http", false); ;
                }

                //If browser path was found, clean it
                if (browserKey != null)
                {
                    //Remove quotation marks
                    browserPath = (browserKey.GetValue(null) as string).ToLower().Replace("\"", "");

                    //Cut off optional parameters
                    if (!browserPath.EndsWith("exe"))
                    {
                        browserPath = browserPath.Substring(0, browserPath.LastIndexOf(".exe") + 4);
                    }

                    //Close registry key
                    browserKey.Close();
                }
            }
            catch
            {
                //Return empty string, if no path was found
                return "";
            }
            //Return default browsers path
            return browserPath;
        }
    }
}