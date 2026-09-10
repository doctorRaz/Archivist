using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Выбирает ZIP-файлы из локальной файловой системы.</summary>
internal sealed class FileSystemArchiveSelector : IArchiveSelector
{
    private readonly IFileSystem _fileSystem;

    /// <summary>Создаёт средство выбора архивов.</summary>
    /// <param name="fileSystem">Файловая система.</param>
    /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> равен <see langword="null"/>.</exception>
    public FileSystemArchiveSelector(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem
            ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>Выбирает архивы в соответствии с параметрами запроса.</summary>
    /// <param name="request">Параметры обработки экспорта.</param>
    /// <returns>Список выбранных ZIP-архивов.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> равен <see langword="null"/>.</exception>
    /// <exception cref="FileNotFoundException">В исходном каталоге не найден ни один подходящий архив.</exception>
    public IReadOnlyList<FileInfo> Select(ExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<FileInfo> archives = _fileSystem
            .EnumerateFiles(
                request.SourceDirectory,
                request.ZipFilePattern,
                SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.LastWriteTimeUtc)
            .ToList();

        if (archives.Count == 0)
        {
            throw new FileNotFoundException(
                $"В каталоге не найден ни один ZIP-архив: {request.SourceDirectory}");
        }

        // Обработка одного архива по умолчанию означает самый новый архив.
        return request.ProcessAllArchives
            ? archives
            : archives.TakeLast(1).ToList();
    }
}
