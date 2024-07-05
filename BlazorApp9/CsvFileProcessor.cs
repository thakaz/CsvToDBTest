using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace BlazorApp9.Helper
{
    /// <summary>
    /// CSVの読み込みを行うクラスのインターフェース
    /// </summary>
    public interface ICsvFileProcessor
    {
        IEnumerable<T> ReadCsvFile<T, U>(string filePath) where U : ClassMap<T>;
    }
    /// <summary>
    /// CSVの読み込みを行うクラス 
    /// </summary>
    public class CsvFileProcessor : ICsvFileProcessor
    {
        public IEnumerable<T> ReadCsvFile<T, U>(string filePath) where U : ClassMap<T>
        {
            using (var reader = new StreamReader(filePath, System.Text.Encoding.GetEncoding("shift_jis"))
) using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<U>();
                return csv.GetRecords<T>().ToList();
            }
        }
    }
}
