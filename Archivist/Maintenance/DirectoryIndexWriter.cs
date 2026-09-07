using System.Globalization;
using System.Text;
using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Maintenance;

/// <summary>Перестраивает пользовательские навигационные индексы vault.</summary>
internal sealed class DirectoryIndexWriter
{
    private const string IndexFileName = "_index.md";
    private readonly IFileSystem _fileSystem;
    private readonly ConversationDisplayNameProvider _displayNameProvider;

    /// <summary>Создаёт средство перестроения индексов.</summary>
    /// <param name="fileSystem">Файловая система.</param>
    /// <param name="displayNameProvider">Источник отображаемых названий разговоров.</param>
    public DirectoryIndexWriter(IFileSystem fileSystem, ConversationDisplayNameProvider displayNameProvider)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _displayNameProvider = displayNameProvider ?? throw new ArgumentNullException(nameof(displayNameProvider));
    }

    /// <summary>Перестраивает индексы корневого каталога, годов и месяцев.</summary>
    /// <param name="root">Корневой каталог vault.</param>
    /// <param name="result">Результат обслуживания, в котором учитываются изменённые индексы.</param>
    public void Rebuild(string root, ArchiveMaintenanceResult result)
    {
        string[] years = _fileSystem.EnumerateDirectories(root, SearchOption.TopDirectoryOnly)
            .Where(IsYearDirectory).OrderByDescending(Path.GetFileName, StringComparer.Ordinal).ToArray();
        foreach (string year in years)
        {
            string[] months = _fileSystem.EnumerateDirectories(year, SearchOption.TopDirectoryOnly)
                .Where(IsMonthDirectory).OrderBy(Path.GetFileName, StringComparer.Ordinal).ToArray();
            foreach (string month in months)
            {
                string[] conversations = ConversationFiles(month);
                string index = Path.Combine(month, IndexFileName);
                if (conversations.Length > 0 || _fileSystem.FileExists(index))
                    WriteIfChanged(index, BuildMonth(month, year, conversations), result);
            }
            WriteIfChanged(Path.Combine(year, IndexFileName), BuildYear(year, months), result);
        }
        WriteIfChanged(Path.Combine(root, IndexFileName), BuildRoot(years), result);
    }

    /// <summary>Возвращает Markdown-файлы разговоров в каталоге без файла индекса.</summary>
    /// <param name="directory">Каталог месяца.</param>
    /// <returns>Отсортированные пути к файлам разговоров.</returns>
    private string[] ConversationFiles(string directory) => _fileSystem
        .EnumerateFiles(directory, "*.md", SearchOption.TopDirectoryOnly)
        .Where(path => !string.Equals(Path.GetFileName(path), IndexFileName, StringComparison.OrdinalIgnoreCase))
        .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>Формирует содержимое индекса месяца.</summary>
    /// <param name="monthDirectory">Каталог месяца.</param><param name="yearDirectory">Каталог года.</param><param name="files">Файлы разговоров.</param>
    /// <returns>Содержимое Markdown-индекса.</returns>
    private string BuildMonth(string monthDirectory, string yearDirectory, string[] files)
    {
        string month = Path.GetFileName(monthDirectory);
        int separator = month.IndexOf('-');
        string monthName = separator >= 0 ? month[(separator + 1)..] : month;
        StringBuilder text = new();
        _ = text.AppendLine($"# {monthName} {Path.GetFileName(yearDirectory)}");
        _ = text.AppendLine(); _ = text.AppendLine("## Conversations"); _ = text.AppendLine();
        foreach (string file in files)
        {
            string fallback = Path.GetFileNameWithoutExtension(file);
            string visibleName = _displayNameProvider.Get(file, fallback);
            _ = text.AppendLine($"- [{visibleName}]({Uri.EscapeDataString(Path.GetFileName(file))})");
        }
        return text.ToString();
    }

    /// <summary>Формирует содержимое индекса года.</summary>
    /// <param name="year">Каталог года.</param><param name="months">Каталоги месяцев.</param>
    /// <returns>Содержимое Markdown-индекса.</returns>
    private static string BuildYear(string year, string[] months)
    {
        StringBuilder text = new(); _ = text.AppendLine($"# {Path.GetFileName(year)}"); _ = text.AppendLine(); _ = text.AppendLine("## Months"); _ = text.AppendLine();
        foreach (string month in months)
        {
            string name = Path.GetFileName(month); int separator = name.IndexOf('-'); string label = separator >= 0 ? name[(separator + 1)..] : name;
            _ = text.AppendLine($"- [{label}]({name}/{IndexFileName})");
        }
        return text.ToString();
    }

    /// <summary>Формирует корневой индекс со ссылками на годы.</summary>
    /// <param name="years">Каталоги годов.</param>
    /// <returns>Содержимое корневого Markdown-индекса.</returns>
    private static string BuildRoot(string[] years)
    {
        StringBuilder text = new(); _ = text.AppendLine("# ChatGPT Conversations"); _ = text.AppendLine(); _ = text.AppendLine("## Years"); _ = text.AppendLine();
        foreach (string year in years) { string name = Path.GetFileName(year); _ = text.AppendLine($"- [{name}]({name}/{IndexFileName})"); }
        return text.ToString();
    }

    /// <summary>Записывает индекс только при изменении его содержимого.</summary>
    /// <param name="path">Путь к индексу.</param><param name="contents">Новое содержимое.</param><param name="result">Результат обслуживания.</param>
    private void WriteIfChanged(string path, string contents, ArchiveMaintenanceResult result)
    {
        if (_fileSystem.FileExists(path) && string.Equals(_fileSystem.ReadAllText(path), contents, StringComparison.Ordinal)) return;
        _fileSystem.WriteAllText(path, contents); result.UpdatedIndexes++;
    }

    /// <summary>Проверяет, является ли каталог каталогом года.</summary>
    /// <param name="path">Путь к каталогу.</param><returns><see langword="true"/>, если имя каталога представляет год.</returns>
    private static bool IsYearDirectory(string path) => int.TryParse(Path.GetFileName(path), NumberStyles.None, CultureInfo.InvariantCulture, out int year) && year is >= 1000 and <= 9999;

    /// <summary>Проверяет, является ли каталог каталогом месяца.</summary>
    /// <param name="path">Путь к каталогу.</param><returns><see langword="true"/>, если имя соответствует формату <c>MM-...</c>.</returns>
    private static bool IsMonthDirectory(string path)
    {
        string name = Path.GetFileName(path); return name.Length >= 4 && name[2] == '-' && int.TryParse(name[..2], out int month) && month is >= 1 and <= 12;
    }
}