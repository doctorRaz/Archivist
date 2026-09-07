using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Maintenance;

/// <summary>Результат обслуживания существующего vault.</summary>
internal sealed class ArchiveMaintenanceResult
{
    /// <summary>Количество проверенных Markdown-файлов.</summary>
    public int CheckedFiles { get; internal set; }
    /// <summary>Количество переименованных файлов.</summary>
    public int RenamedFiles { get; internal set; }
    /// <summary>Количество обнаруженных конфликтов имён.</summary>
    public int Conflicts { get; internal set; }
    /// <summary>Количество обновлённых индексов каталогов.</summary>
    public int UpdatedIndexes { get; internal set; }
    /// <summary>Количество ошибок обслуживания.</summary>
    public int Errors { get; internal set; }
}

/// <summary>Нормализует существующий vault и перестраивает навигационные индексы.</summary>
internal sealed class ArchiveMaintenance
{
    private const string IndexFileName = "_index.md";
    private readonly IFileSystem _fileSystem;
    private readonly IFileNameNormalizer _normalizer;
    private readonly DirectoryIndexWriter _indexWriter;

    /// <summary>Создаёт средство обслуживания архива.</summary>
    /// <param name="fileSystem">Файловая система.</param>
    /// <param name="normalizer">Средство нормализации имён.</param>
    /// <param name="indexWriter">Средство перестроения индексов.</param>
    public ArchiveMaintenance(IFileSystem fileSystem, IFileNameNormalizer normalizer, DirectoryIndexWriter indexWriter)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        _indexWriter = indexWriter ?? throw new ArgumentNullException(nameof(indexWriter));
    }

    /// <summary>Выполняет нормализацию файлов и перестраивает индексы vault.</summary>
    /// <param name="rootDirectory">Корневой каталог vault.</param>
    /// <returns>Результат обслуживания.</returns>
    /// <exception cref="ArgumentException">Каталог не указан.</exception>
    /// <exception cref="DirectoryNotFoundException">Каталог не существует.</exception>
    public ArchiveMaintenanceResult Run(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Не указан каталог vault.", nameof(rootDirectory));
        if (!_fileSystem.DirectoryExists(rootDirectory))
            throw new DirectoryNotFoundException($"Каталог vault не найден: {rootDirectory}");

        ArchiveMaintenanceResult result = new();
        NormalizeFiles(Path.GetFullPath(rootDirectory), result);
        _indexWriter.Rebuild(Path.GetFullPath(rootDirectory), result);
        return result;
    }

    /// <summary>Переименовывает файлы в соответствии с правилами нормализации.</summary>
    /// <param name="root">Корневой каталог vault.</param>
    /// <param name="result">Аккумулятор результатов обслуживания.</param>
    private void NormalizeFiles(string root, ArchiveMaintenanceResult result)
    {
        string[] files = _fileSystem.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !string.Equals(Path.GetFileName(path), IndexFileName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        result.CheckedFiles = files.Length;

        var moves = new List<(string Source, string Target)>();
        var occupied = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string source in files)
        {
            string name = Path.GetFileNameWithoutExtension(source);
            string normalized = _normalizer.Normalize(name);
            if (string.IsNullOrWhiteSpace(normalized)) normalized = name;
            string target = Path.Combine(Path.GetDirectoryName(source)!, normalized + Path.GetExtension(source));
            if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase)) continue;

            if (occupied.Contains(target) || !targets.Add(target))
            {
                result.Conflicts++;
                try { target = GetUniqueTarget(target, occupied, targets); _ = targets.Add(target); }
                catch (Exception) { result.Errors++; continue; }
            }
            moves.Add((source, target));
        }

        var temporary = new List<(string Temp, string Target)>();
        foreach ((string source, string target) in moves)
        {
            try
            {
                string temp = source + $".archivist-maintenance-{Guid.NewGuid():N}.tmp";
                _fileSystem.MoveFile(source, temp);
                temporary.Add((temp, target));
            }
            catch (Exception) { result.Errors++; }
        }
        foreach ((string temp, string target) in temporary)
        {
            try { _fileSystem.MoveFile(temp, target); result.RenamedFiles++; }
            catch (Exception) { result.Errors++; }
        }
    }

    /// <summary>Подбирает свободное имя с числовым суффиксом.</summary>
    /// <param name="path">Исходное целевое имя.</param>
    /// <param name="occupied">Уже занятые пути.</param>
    /// <param name="targets">Уже зарезервированные целевые пути.</param>
    /// <returns>Свободный путь.</returns>
    /// <exception cref="IOException">Не удалось найти свободное имя за 100 попыток.</exception>
    private static string GetUniqueTarget(string path, ISet<string> occupied, ISet<string> targets)
    {
        string directory = Path.GetDirectoryName(path)!;
        string stem = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);
        for (int number = 1; number <= 100; number++)
        {
            string candidate = Path.Combine(directory, $"{stem} ({number}){extension}");
            if (!occupied.Contains(candidate) && !targets.Contains(candidate)) return candidate;
        }
        throw new IOException($"Не удалось подобрать свободное имя: {path}");
    }
}