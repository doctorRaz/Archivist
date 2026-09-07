namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Выбирает ZIP-архивы, участвующие в обработке.</summary>
internal interface IArchiveSelector
{
    /// <summary>Выбирает архивы согласно параметрам запроса.</summary>
    /// <param name="request">Запрос на обработку экспорта.</param>
    /// <returns>Список выбранных ZIP-архивов.</returns>
    IReadOnlyList<FileInfo> Select(ExportRequest request);
}
