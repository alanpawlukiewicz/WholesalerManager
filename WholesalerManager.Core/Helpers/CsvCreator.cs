using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Reflection;

namespace WholesalerManager.Core.Helpers
{
    /// <summary>
    /// Static helper class for creating CSV files from a list of objects of type T, with the ability to exclude specified properties.
    /// </summary>
    public static class CsvCreator
    {
        /// <summary>
        /// Creates a CSV file from a list of objects of type T, excluding specified properties.
        /// </summary>
        /// <typeparam name="T">Type of objects in the list</typeparam>
        /// <param name="data">List of objects to include in the CSV</param>
        /// <param name="exclude">An array of property names to exclude from the CSV</param>
        /// <returns>MemoryStream containing the CSV data</returns>
        public static async Task<MemoryStream> CreateCsv<T>(IEnumerable<T> data, string[] exclude) where T : class
        {
            MemoryStream memoryStream = new MemoryStream();
            await using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
            await using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, true))
            {
                CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (exclude.Contains(property.Name))
                    {
                        continue;
                    }
                    csvWriter.WriteField(property.Name);
                }
                csvWriter.NextRecord();

                foreach (T item in data)
                {
                    foreach (PropertyInfo property in properties)
                    {
                        if (exclude.Contains(property.Name))
                        {
                            continue;
                        }
                        csvWriter.WriteField(property.GetValue(item)?.ToString());
                    }
                    csvWriter.NextRecord();
                }
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
