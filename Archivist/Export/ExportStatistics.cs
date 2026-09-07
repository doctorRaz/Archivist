using dRz.GPT_Utilities.Archivist.Files;

namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Внутренний аккумулятор статистики обработки.</summary>
/// <remarks>Изменяется во время обработки; наружу преобразуется в неизменяемый <see cref="ExportResult"/>.</remarks>
internal sealed class ExportStatistics
{
    /// <summary>Ошибки обработки архивов.</summary>
    private readonly List<ExportError> _archiveErrors = new();
    /// <summary>Ошибки обработки Markdown-файлов.</summary>
    private readonly List<ExportError> _markdownErrors = new();

    /// <summary>Общее количество обработанных файлов и зарегистрированных ошибок файлов.</summary>
    public int Total { get; private set; }
    /// <summary>Количество пропущенных файлов.</summary>
    public int Skipped { get; private set; }
    /// <summary>Количество добавленных файлов.</summary>
    public int Added { get; private set; }
    /// <summary>Количество обновлённых файлов.</summary>
    public int Updated { get; private set; }
    /// <summary>Количество ошибок обработки Markdown-файлов.</summary>
    public int Failed { get; private set; }
    /// <summary>Количество архивов, обработка которых завершилась ошибкой.</summary>
    public int ArchiveFailed { get; private set; }
    /// <summary>Ошибки обработки архивов.</summary>
    public IReadOnlyList<ExportError> ArchiveErrors => _archiveErrors;
    /// <summary>Ошибки обработки Markdown-файлов.</summary>
    public IReadOnlyList<ExportError> MarkdownErrors => _markdownErrors;

    /// <summary>Добавляет результат обработки одного файла в статистику.</summary>
    /// <param name="result">Результат операции над файлом.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> равен <see langword="null"/>.</exception>
    public void Add(FileOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Status == FileOperationStatus.Failed)
        {
            AddFailure();
            Total += result.IndexReadErrors;
            Failed += result.IndexReadErrors;
            AddErrors(result.Errors);
            return;
        }
        Total++;
        Total += result.IndexReadErrors;
        Failed += result.IndexReadErrors;
        AddErrors(result.Errors);
        switch (result.Status)
        {
            case FileOperationStatus.Skipped: Skipped++; break;
            case FileOperationStatus.Added: Added++; break;
            case FileOperationStatus.Updated: Updated++; break;
            default: throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }
    }

    /// <summary>Добавляет ошибки обработки Markdown-файла.</summary>
    /// <param name="errors">Список ошибок.</param>
    private void AddErrors(IReadOnlyList<ExportError>? errors)
    {
        if (errors is null) return;
        foreach (ExportError error in errors) AddMarkdownError(error);
    }

    /// <summary>Регистрирует ошибку обработки файла.</summary>
    public void AddFailure() { Total++; Failed++; }

    /// <summary>Регистрирует ошибку архива без добавления ошибки файла.</summary>
    public void AddArchiveFailure() { ArchiveFailed++; }

    /// <summary>Регистрирует структурированную ошибку архива.</summary>
    /// <param name="error">Описание ошибки.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> равен <see langword="null"/>.</exception>
    public void AddArchiveError(ExportError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArchiveFailed++;
        _archiveErrors.Add(error);
    }

    /// <summary>Регистрирует структурированную ошибку Markdown-файла.</summary>
    /// <param name="error">Описание ошибки.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> равен <see langword="null"/>.</exception>
    public void AddMarkdownError(ExportError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        Failed++;
        Total++;
        _markdownErrors.Add(error);
    }

    /// <summary>Добавляет статистику другого аккумулятора.</summary>
    /// <param name="statistics">Статистика отдельного архива или этапа обработки.</param>
    public void Add(ExportStatistics statistics)
    {
        Total += statistics.Total;
        Skipped += statistics.Skipped;
        Added += statistics.Added;
        Updated += statistics.Updated;
        Failed += statistics.Failed;
        ArchiveFailed += statistics.ArchiveFailed;
        _archiveErrors.AddRange(statistics.ArchiveErrors);
        _markdownErrors.AddRange(statistics.MarkdownErrors);
    }

    /// <summary>Создаёт неизменяемый результат на основе накопленной статистики.</summary>
    /// <returns>Результат обработки экспорта.</returns>
    public ExportResult ToResult() => new(Total, Skipped, Added, Updated, Failed, ArchiveFailed, ArchiveErrors.ToArray(), MarkdownErrors.ToArray());
}