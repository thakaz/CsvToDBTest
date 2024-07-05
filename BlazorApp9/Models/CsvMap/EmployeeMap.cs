using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace BlazorApp9.Models.CsvMap;

public class EmployeeMap : ClassMap<Employee>
{
    public EmployeeMap()
    {
        Map(e => e.EmployeeCode).Name("社員コード");
        Map(e => e.EmployeeName).Name("名前");
        Map(e => e.Age).Name("年齢").TypeConverter<ZeroIfEmptyIntConverter>();
    }
}

public class DepartmentMap : ClassMap<Department>
{
    public DepartmentMap()
    {
        Map(d => d.DepartmentCode).Name("部署コード");
        Map(d => d.DepartmentName).Name("部署名");
    }
}

public class ZeroIfEmptyIntConverter : Int32Converter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return base.ConvertFromString(text, row, memberMapData);
    }
}

public class FileModelMapping
{
    public string FileName { get; set; } = "";
    public string ModelType { get; set; } = "";
    public string MapModelType { get; set; } = "";
    public string UpdateMethod { get; set; } = "";//"add" or "replace"
}

