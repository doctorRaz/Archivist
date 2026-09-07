namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Неизменяемый результат обработки экспортных архивов.</summary>
/// <param name="Total">Общее количество обработанных файлов.</param>
/// <param name="Skipped">Количество пропущенных файлов.</param>
/// <param name="Added">Количество добавленных файлов.</param>
/// <param name="Updated">Количество обновлённых файлов.</param>
/// <param name="Failed">Количество ошибок обработки файлов.</param>
/// <param name="ArchiveFailed">Количество архивов, обработка которых завершилась ошибкой.</param>
/// <param name="ArchiveErrors">Ошибки обработки архивов.</param>
/// <param name="MarkdownErrors">Ошибки обработки Markdown-файлов.</param>
internal sealed record ExportResult(
    int Total,
    int Skipped,
    int Added,
    int Updated,
    int Failed,
    int ArchiveFailed,
    IReadOnlyList<ExportError> ArchiveErrors,
    IReadOnlyList<ExportError> MarkdownErrors)
{
    /// <summary>Общее количество успешно добавленных или обновлённых файлов.</summary>
    public int AddedOrUpdated => Added + Updated;

    /// <summary>Возвращает объединённый список ошибок архивов и Markdown-файлов.</summary>
    public IReadOnlyList<ExportError> Errors => ArchiveErrors.Concat(MarkdownErrors).ToArray();
}
