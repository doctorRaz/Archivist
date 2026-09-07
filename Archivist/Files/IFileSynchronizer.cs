using dRz.GPT_Utilities.Archivist.Export;
using dRz.GPT_Utilities.Archivist.Infrastructure;
using System.Text;

namespace dRz.GPT_Utilities.Archivist.Files;

/// <summary>
/// Применяет политику добавления, обновления и пропуска файла.
/// </summary>
internal interface IFileSynchronizer
{
    /// <summary>
    /// Синхронизирует исходный файл с целевым, применяя соответствующую политику (добавление, обновление или пропуск).
    /// </summary>
    /// <param name="sourceFilePath">Путь к исходному файлу.</param>
    /// <param name="destinationFilePath">Путь к целевому файлу.</param>
    /// <param name="sourceMetadata">Метаданные исходного файла.</param>
    /// <returns>Результат операции синхронизации.</returns>
    FileOperationResult Synchronize(string sourceFilePath, string destinationFilePath, ChatMetadata sourceMetadata);
}

/// <summary>Обновляет навигационный индекс каталога разговоров.</summary>
internal interface IConversationIndexWriter
{
    /// <summary>Обновляет файл индекса указанного каталога.</summary>
    /// <param name="directory">Каталог, индекс которого необходимо обновить.</param>
    void Refresh(string directory);
}

/// <summary>Создаёт файл <c>_index.md</c> по содержимому каталога.</summary>
internal sealed class ConversationIndexWriter : IConversationIndexWriter
{
    private const string IndexFileName = "_index.md";
    private readonly IFileSystem _fileSystem;

    /// <summary>Создаёт средство записи индекса разговоров.</summary>
    /// <param name="fileSystem">Файловая система.</param>
    /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> равен <see langword="null"/>.</exception>
    public ConversationIndexWriter(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>Перестраивает индекс разговоров указанного каталога.</summary>
    /// <param name="directory">Каталог, содержащий Markdown-файлы разговоров.</param>
    public void Refresh(string directory)
    {
        string year = Directory.GetParent(directory)?.Name ?? string.Empty;
        string month = new DirectoryInfo(directory).Name;
        int separator = month.IndexOf('-');
        string monthName = separator >= 0 ? month[(separator + 1)..] : month;

        IEnumerable<string> files = _fileSystem
            .EnumerateFiles(directory, "*.md", SearchOption.TopDirectoryOnly)
            .Where(path => !string.Equals(Path.GetFileName(path), IndexFileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase);

        StringBuilder contents = new();
        _ = contents.AppendLine($"# {monthName} {year}");
        _ = contents.AppendLine();
        _ = contents.AppendLine("## Conversations");
        _ = contents.AppendLine();

        foreach (string path in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string encodedFileName = Uri.EscapeDataString(Path.GetFileName(path));
            _ = contents.AppendLine($"- [{fileName}]({encodedFileName})");
        }

        _fileSystem.WriteAllText(Path.Combine(directory, IndexFileName), contents.ToString());
    }
}

/// <summary>Экземплярный сервис синхронизации Markdown-файлов.</summary>
internal sealed class FileSynchronizerService : IFileSynchronizer
{
    /// <summary>Средство чтения метаданных.</summary>
    private readonly IChatMetadataReader _metadataReader;
    /// <summary>Журналировщик.</summary>
    private readonly IArchivistLogger _logger;
    /// <summary>Провайдер уникальных имен.</summary>
    private readonly IUniqueFileNameProvider _uniqueFileNameProvider;
    /// <summary>Система файловых операций.</summary>
    private readonly IFileSystem _fileSystem;
    /// <summary>Индекс разговоров.</summary>
    private readonly IConversationIndex _conversationIndex;
    /// <summary>Средство обновления файла индекса каталога.</summary>
    private readonly IConversationIndexWriter _conversationIndexWriter;
    private readonly List<ExportError> _operationErrors = new();
    private static readonly object SynchronizationLock = new();

    /// <summary>Инициализирует новый экземпляр <see cref="FileSynchronizerService"/>.</summary>
    /// <param name="metadataReader">Средство чтения метаданных.</param>
    /// <param name="logger">Журналировщик.</param>
    /// <param name="uniqueFileNameProvider">Провайдер уникальных имён.</param>
    /// <param name="fileSystem">Система файловых операций.</param>
    /// <param name="conversationIndex">Индекс разговоров. Если не задан, создаётся стандартный индекс.</param>
    /// <param name="conversationIndexWriter">Средство записи индекса разговоров. Если не задано, создаётся стандартное средство записи.</param>
    /// <exception cref="ArgumentNullException">Один из обязательных параметров равен <see langword="null"/>.</exception>
    public FileSynchronizerService(
        IChatMetadataReader metadataReader,
        IArchivistLogger logger,
        IUniqueFileNameProvider uniqueFileNameProvider,
        IFileSystem fileSystem,
        IConversationIndex? conversationIndex = null,
        IConversationIndexWriter? conversationIndexWriter = null)
    {
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uniqueFileNameProvider = uniqueFileNameProvider ?? throw new ArgumentNullException(nameof(uniqueFileNameProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _conversationIndex = conversationIndex ?? new ConversationIndex(fileSystem, metadataReader, logger);
        _conversationIndexWriter = conversationIndexWriter ?? new ConversationIndexWriter(fileSystem);
    }

    // Остальная реализация сервиса без изменений.
}
