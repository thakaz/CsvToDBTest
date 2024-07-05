using BlazorApp9.Data;
using BlazorApp9.Helper;
using BlazorApp9.Models.CsvMap;

namespace BlazorApp9.Services;

/// <summary>
/// csvファイルをDBにぶち込むためのバッチ処理サービス
/// </summary>
public class CsvToDBService : IHostedService, IDisposable
{
    private readonly ILogger<CsvToDBService> _logger;
    private Timer? _timer;

    private readonly string _csvFolderPath;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<FileModelMapping> _fileModelMappings;

    public CsvToDBService(ILogger<CsvToDBService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _logger = logger;
        _csvFolderPath = configuration.GetValue<string>("CsvFolderPath") ?? throw new ArgumentNullException(nameof(configuration));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(configuration));
        _fileModelMappings = configuration.GetSection("FileModelMappings").Get<List<FileModelMapping>>() ?? throw new ArgumentNullException(nameof(configuration));

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CsvToDBService is starting.");

        //とりあえず30秒ごとに処理を行う
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("CsvToDBService is working.");

        //csvファイルをDBに登録する処理
        foreach (var filePath in Directory.GetFiles(_csvFolderPath, "*.csv"))
        {

            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var mapping = GetMappingFromFileName(fileName);
                    if (mapping == null)
                    {
                        _logger.LogWarning($"No mapping found for file {fileName}");
                        continue;
                    }

                    var csvFileProcessor = scope.ServiceProvider.GetRequiredService<ICsvFileProcessor>();
                    var csvImportRepository = scope.ServiceProvider.GetRequiredService<ICsvImportRepository>();

                    Type modelType = Type.GetType(mapping.ModelType) ?? throw new ArgumentNullException(nameof(mapping.ModelType));
                    Type mapModelType = Type.GetType(mapping.MapModelType) ?? throw new ArgumentNullException(nameof(mapping.MapModelType));

                    var method = typeof(CsvFileProcessor).GetMethod("ReadCsvFile")?.MakeGenericMethod(modelType, mapModelType);
                    if (method == null)
                    {
                        _logger.LogError($"The generic method 'ReadCsvFile<{modelType.Name}, {mapModelType.Name}>' could not be found.");
                        continue;
                    }
                    var records = method.Invoke(csvFileProcessor, [filePath]) as IEnumerable<object>;

                    if (records == null)
                    {
                        _logger.LogError($"Failed to read CSV file '{filePath}' with model type '{modelType.Name}' and map model type '{mapModelType.Name}'.");
                        continue;
                    }

                    if (mapping.UpdateMethod == "add")
                    {
                        csvImportRepository.AddRecordsWithIgnore(modelType, records);
                    }
                    else if (mapping.UpdateMethod == "replace")
                    {
                        csvImportRepository.ReplaceRecords(modelType, records);
                    }
                    _logger.LogInformation($"Successfully processed {mapping.UpdateMethod} for file: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CsvToDBService");
                }
            }
        }
    }
    private FileModelMapping GetMappingFromFileName(string fileName)
    {
        return _fileModelMappings.FirstOrDefault(m => fileName.StartsWith(m.FileName));
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CsvToDBService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }


    public void Dispose()
    {
        _timer?.Dispose();
    }
}

