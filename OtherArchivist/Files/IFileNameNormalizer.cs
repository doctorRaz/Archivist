using System.Text.RegularExpressions;

namespace dRz.GPT_Utilities.Archivist.Files;

/// <summary>Нормализует имя экспортируемого файла.</summary>
internal interface IFileNameNormalizer
{
    /// <summary>Нормализует имя файла.</summary>
    /// <param name="fileName">Имя файла для нормализации.</param>
    /// <returns>Нормализованное имя файла.</returns>
    string Normalize(string fileName);
}

/// <summary>Нормализует имена файлов по правилам Archivist.</summary>
internal sealed class FileNameNormalizer : IFileNameNormalizer
{
    /// <summary>Регулярное выражение для поиска последовательностей пробельных символов.</summary>
    private static readonly Regex MultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Нормализует имя файла, заменяя символ <c>#</c> пробелом и убирая лишние пробелы.
    /// Символ подчёркивания сохраняется без изменений.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Нормализованное имя файла.</returns>
    public string Normalize(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        string normalized = fileName.Replace('#', ' ');
        normalized = MultipleSpacesRegex.Replace(normalized, " ");
        return normalized.Trim();
    }
}
