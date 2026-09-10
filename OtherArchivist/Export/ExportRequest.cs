namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Независимый от командной строки запрос на обработку экспорта.</summary>
/// <param name="SourceDirectory">Каталог с исходными ZIP-архивами.</param>
/// <param name="DestinationDirectory">Каталог назначения для обработанных файлов.</param>
/// <param name="ZipFilePattern">Маска ZIP-файлов, участвующих в обработке.</param>
/// <param name="ProcessAllArchives"><see langword="true"/> для обработки всех подходящих архивов; иначе обрабатывается выбранный архив.</param>
/// <remarks>Отделяет сценарий экспорта от способа передачи параметров пользователем.</remarks>
internal sealed record ExportRequest(
    string SourceDirectory,
    string DestinationDirectory,
    string ZipFilePattern,
    bool ProcessAllArchives);
