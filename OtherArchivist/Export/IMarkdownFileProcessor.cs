using dRz.GPT_Utilities.Archivist.Files;
using dRz.GPT_Utilities.Archivist.Infrastructure;

namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Обрабатывает один Markdown-файл экспортного архива.</summary>
internal interface IMarkdownFileProcessor
{
    /// <summary>Обрабатывает Markdown-файл, используя его метаданные и имя.</summary>
    /// <param name="sourceFile">Путь к исходному Markdown-файлу.</param>
    /// <param name="destinationDirectory">Корневой каталог назначения.</param>
    /// <returns>Результат операции над файлом.</returns>
    FileOperationResult Process(string sourceFile, string destinationDirectory);
}

/// <summary>Читает метаданные, нормализует имя и синхронизирует Markdown-файл.</summary>
internal sealed class MarkdownFileProcessor : IMarkdownFileProcessor
{
    private readonly IExportPathBuilder _pathBuilder;
    private readonly IChatMetadataReader _metadataReader;
    private readonly IFileSynchronizer _fileSynchronizer;
    private readonly IChatMetadataWriter _metadataWriter;
    private readonly IArchivistLogger _logger;
    private readonly IFileNameNormalizer _fileNameNormalizer;

    /// <summary>Создаёт обработчик Markdown-файлов.</summary>
    /// <param name="pathBuilder">Построитель пути назначения.</param>
    /// <param name="metadataReader">Средство чтения метаданных.</param>
    /// <param name="fileSynchronizer">Средство синхронизации файлов.</param>
    /// <param name="logger">Журналировщик.</param>
    /// <param name="fileNameNormalizer">Нормализатор имён файлов.</param>
    /// <param name="metadataWriter">Средство записи метаданных; при отсутствии используется стандартная реализация.</param>
    /// <exception cref="ArgumentNullException">Обязательная зависимость равна <see langword="null"/>.</exception>
    public MarkdownFileProcessor(
        IExportPathBuilder pathBuilder,
        IChatMetadataReader metadataReader,
        IFileSynchronizer fileSynchronizer,
        IArchivistLogger logger,
        IFileNameNormalizer fileNameNormalizer,
        IChatMetadataWriter? metadataWriter = null)
    {
        _pathBuilder = pathBuilder ?? throw new ArgumentNullException(nameof(pathBuilder));
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
        _fileSynchronizer = fileSynchronizer ?? throw new ArgumentNullException(nameof(fileSynchronizer));
        _metadataWriter = metadataWriter ?? new ChatMetadataWriter(new LocalFileSystem());
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileNameNormalizer = fileNameNormalizer
            ?? throw new ArgumentNullException(nameof(fileNameNormalizer));
    }

    /// <summary>Обрабатывает Markdown-файл экспортного архива.</summary>
    /// <param name="sourceFile">Путь к исходному Markdown-файлу.</param>
    /// <param name="destinationDirectory">Корневой каталог назначения.</param>
    /// <returns>Результат синхронизации файла.</returns>
    public FileOperationResult Process(string sourceFile, string destinationDirectory)
    {
        ChatMetadata metadata = _metadataReader.Read(sourceFile);
        string originalName = Path.GetFileNameWithoutExtension(sourceFile);
        string extension = Path.GetExtension(sourceFile);
        string normalizedName = _fileNameNormalizer.Normalize(originalName);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            normalizedName = originalName;
            _logger.Warning($"КОПИРУЮ КАК ЕСТЬ: пустое имя файла: {sourceFile}");
        }

        string destinationFile = _pathBuilder.Build(
            destinationDirectory,
            metadata,
            normalizedName + extension);

        FileOperationResult result = _fileSynchronizer.Synchronize(
            sourceFile,
            destinationFile,
            metadata);

        if (result.Status is FileOperationStatus.Added or FileOperationStatus.Updated)
        {
            string actualDestination = result.DestinationPath
                ?? throw new InvalidOperationException("Путь назначения отсутствует.");
            _metadataWriter.Write(actualDestination, metadata);
        }

        return result;
    }
}
