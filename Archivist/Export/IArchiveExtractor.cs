using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Распаковывает один экспортный архив во временное хранилище.</summary>
internal interface IArchiveExtractor
{
    /// <summary>Распаковывает указанный архив.</summary>
    /// <param name="archive">ZIP-архив для распаковки.</param>
    /// <returns>Объект с путём временного хранилища и найденными Markdown-файлами.</returns>
    ExtractedArchive Extract(FileInfo archive);
}

/// <summary>Распакованный архив и список содержащихся в нём Markdown-файлов.</summary>
internal sealed class ExtractedArchive : IDisposable
{
    /// <summary>Путь к временному каталогу распакованного архива.</summary>
    private readonly string _directory;
    /// <summary>Абстракция файловой системы.</summary>
    private readonly IFileSystem _fileSystem;

    /// <summary>Создаёт представление распакованного архива.</summary>
    /// <param name="directory">Каталог распакованного архива.</param>
    /// <param name="fileSystem">Файловая система.</param>
    public ExtractedArchive(string directory, IFileSystem fileSystem)
    {
        _directory = directory;
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        MarkdownFiles = _fileSystem.EnumerateFiles(directory, "*.md", SearchOption.AllDirectories).ToArray();
    }

    /// <summary>Markdown-файлы, найденные в распакованном архиве.</summary>
    public IReadOnlyList<string> MarkdownFiles { get; }

    /// <summary>Удаляет временный каталог распакованного архива.</summary>
    /// <remarks>Ошибки очистки не скрывают результат основной обработки.</remarks>
    public void Dispose()
    {
        if (!_fileSystem.DirectoryExists(_directory)) return;
        try { _fileSystem.DeleteDirectory(_directory, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}