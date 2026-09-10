using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Строит путь назначения Markdown-файла по его метаданным.</summary>
internal interface IExportPathBuilder
{
    /// <summary>Формирует полный путь назначения файла.</summary>
    /// <param name="destinationDirectory">Корневой каталог назначения.</param>
    /// <param name="metadata">Метаданные разговора.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Полный путь к файлу назначения.</returns>
    string Build(string destinationDirectory, ChatMetadata metadata, string fileName);
}

/// <summary>Формирует структуру назначения <c>YYYY\MM-MMMM</c>.</summary>
/// <remarks>
/// Время нормализуется к UTC, а название месяца форматируется с использованием инвариантной культуры.
/// Например: <c>2024\03-March</c>.
/// </remarks>
internal sealed class ExportPathBuilder : IExportPathBuilder
{
    /// <summary>Формат года в имени каталога.</summary>
    private const string YearFormat = "yyyy";
    /// <summary>Формат месяца в имени каталога.</summary>
    private const string MonthFormat = "MM-MMMM";
    /// <summary>Файловая система для создания каталогов назначения.</summary>
    private readonly IFileSystem _fileSystem;

    /// <summary>Создаёт построитель путей назначения.</summary>
    /// <param name="fileSystem">Файловая система.</param>
    public ExportPathBuilder(IFileSystem fileSystem) =>
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

    /// <summary>Формирует каталог года и месяца и возвращает путь к файлу.</summary>
    /// <param name="destinationDirectory">Корневой каталог назначения.</param>
    /// <param name="metadata">Метаданные разговора.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Полный путь к файлу назначения.</returns>
    public string Build(string destinationDirectory, ChatMetadata metadata, string fileName)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        DateTimeOffset createTime = metadata.CreateTime.ToUniversalTime();
        string monthDirectory = Path.Combine(
            destinationDirectory,
            createTime.ToString(YearFormat, System.Globalization.CultureInfo.InvariantCulture),
            createTime.ToString(MonthFormat, System.Globalization.CultureInfo.InvariantCulture));
        _fileSystem.CreateDirectory(monthDirectory);
        return Path.Combine(monthDirectory, fileName);
    }
}