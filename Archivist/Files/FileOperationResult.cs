using dRz.GPT_Utilities.Archivist.Export;

namespace dRz.GPT_Utilities.Archivist.Files;

/// <summary>Результат обработки одного файла.</summary>
/// <param name="Status">Статус операции.</param>
/// <param name="SourcePath">Путь к исходному файлу.</param>
/// <param name="DestinationPath">Фактический путь назначения, если файл был добавлен или обновлён.</param>
/// <param name="Reason">Дополнительное пояснение результата.</param>
/// <param name="Error">Исключение, вызвавшее ошибку, если оно произошло.</param>
/// <param name="IndexReadErrors">Количество ошибок чтения индекса, обнаруженных при операции.</param>
/// <param name="Errors">Структурированные ошибки, возникшие при обработке.</param>
internal sealed record FileOperationResult(
    FileOperationStatus Status,
    string SourcePath,
    string? DestinationPath = null,
    string? Reason = null,
    Exception? Error = null,
    int IndexReadErrors = 0,
    IReadOnlyList<ExportError>? Errors = null);

/// <summary>Возможные результаты обработки файла.</summary>
internal enum FileOperationStatus
{
    /// <summary>Файл не требует изменения.</summary>
    Skipped,
    /// <summary>Файл добавлен в каталог назначения.</summary>
    Added,
    /// <summary>Существующий файл обновлён.</summary>
    Updated,
    /// <summary>Обработка файла завершилась ошибкой.</summary>
    Failed
}
