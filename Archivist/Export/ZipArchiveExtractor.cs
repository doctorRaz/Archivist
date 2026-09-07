using System.IO.Compression;
using System.Text;
using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Распаковывает ZIP-архивы во временные каталоги.</summary>
internal sealed class ZipArchiveExtractor : IArchiveExtractor
{
    /// <summary>Кодировка имён записей ZIP-архива.</summary>
    private readonly Encoding _entryNameEncoding;
    /// <summary>Абстракция файловой системы.</summary>
    private readonly IFileSystem _fileSystem;

    /// <summary>Создаёт распаковщик ZIP-архивов.</summary>
    /// <param name="entryNameEncoding">Кодировка имён записей архива.</param>
    /// <param name="fileSystem">Файловая система.</param>
    /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> равен <see langword="null"/>.</exception>
    public ZipArchiveExtractor(
        Encoding entryNameEncoding,
        IFileSystem fileSystem)
    {
        _entryNameEncoding = entryNameEncoding;
        _fileSystem = fileSystem
            ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>Распаковывает архив во временный каталог и возвращает найденные Markdown-файлы.</summary>
    /// <param name="archive">ZIP-архив для распаковки.</param>
    /// <returns>Представление распакованного архива.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="archive"/> равен <see langword="null"/>.</exception>
    public ExtractedArchive Extract(FileInfo archive)
    {
        ArgumentNullException.ThrowIfNull(archive);

        string directory = Path.Combine(
            Path.GetTempPath(),
            $"GPT_Archivist_{Guid.NewGuid():N}");

        _fileSystem.CreateDirectory(directory);

        try
        {
            ZipFile.ExtractToDirectory(
                archive.FullName,
                directory,
                _entryNameEncoding);

            return new ExtractedArchive(directory, _fileSystem);
        }
        catch
        {
            try
            {
                _fileSystem.DeleteDirectory(directory, recursive: true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            throw;
        }
    }
}
