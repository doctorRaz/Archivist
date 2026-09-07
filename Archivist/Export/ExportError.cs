namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Структурированная ошибка обработки экспорта.</summary>
/// <param name="Path">Путь к файлу или архиву, при обработке которого произошла ошибка.</param>
/// <param name="ExceptionType">Полное имя типа исключения.</param>
/// <param name="Message">Сообщение исключения.</param>
/// <param name="Stage">Этап обработки, на котором произошла ошибка.</param>
internal sealed record ExportError(
    string Path,
    string ExceptionType,
    string Message,
    string Stage);
