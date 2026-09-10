
namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Обрабатывает экспорт ChatGPT.</summary>
internal interface IChatGptExportProcessor
{
    /// <summary>Запускает сценарий обработки экспорта.</summary>
    /// <param name="request">Параметры обработки.</param>
    /// <returns>Итоговая статистика обработки.</returns>
    ExportResult Process(ExportRequest request);
}