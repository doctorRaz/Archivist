using dRz.GPT_Utilities.Archivist.Files;
using dRz.GPT_Utilities.Archivist.Infrastructure;
using dRz.GPT_Utilities.Archivist.Localization;

namespace dRz.GPT_Utilities.Archivist.Export
{
    /// <summary>Координирует обработку ZIP-архивов экспорта ChatGPT.</summary>
    /// <remarks>Класс делегирует выбор архивов, распаковку и обработку Markdown отдельным компонентам.</remarks>
    internal sealed class ChatGptExportProcessor : IChatGptExportProcessor
    {
        /// <summary>Средство выбора экспортных архивов.</summary>
        private readonly IArchiveSelector _archiveSelector;
        /// <summary>Средство распаковки архивов.</summary>
        private readonly IArchiveExtractor _archiveExtractor;
        /// <summary>Средство обработки Markdown-файлов.</summary>
        private readonly IMarkdownFileProcessor _markdownProcessor;
        /// <summary>Журналировщик процесса обработки.</summary>
        private readonly IArchivistLogger _logger;

        /// <summary>Создаёт процессор с заданными зависимостями.</summary>
        /// <param name="archiveSelector">Средство выбора архивов.</param>
        /// <param name="archiveExtractor">Средство распаковки архивов.</param>
        /// <param name="markdownProcessor">Средство обработки Markdown-файлов.</param>
        /// <param name="logger">Журналировщик.</param>
        public ChatGptExportProcessor(IArchiveSelector archiveSelector, IArchiveExtractor archiveExtractor, IMarkdownFileProcessor markdownProcessor, IArchivistLogger logger)
        {
            _archiveSelector = archiveSelector ?? throw new ArgumentNullException(nameof(archiveSelector));
            _archiveExtractor = archiveExtractor ?? throw new ArgumentNullException(nameof(archiveExtractor));
            _markdownProcessor = markdownProcessor ?? throw new ArgumentNullException(nameof(markdownProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Обрабатывает выбранные экспортные архивы.</summary>
        /// <param name="request">Параметры обработки экспорта.</param>
        /// <returns>Итоговая статистика обработки.</returns>
        public ExportResult Process(ExportRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            IReadOnlyList<FileInfo> archives = _archiveSelector.Select(request);
            _logger.Trace($"Найдено {archives.Count.Of(RussianWords.Archives)} для обработки");
            ExportStatistics statistics = new();
            foreach (FileInfo archive in archives)
            {
                _logger.Trace($"ZIP: {archive.FullName}");
                _logger.Trace($"\tДата изменения ZIP: {archive.LastWriteTime}");
                try
                {
                    ExportStatistics archiveStatistics = ProcessArchive(archive, request.DestinationDirectory);
                    statistics.Add(archiveStatistics);
                }
                catch (InvalidDataException exception)
                {
                    statistics.AddArchiveError(CreateError(archive.FullName, "Архив", exception));
                    _logger.Error($"Ошибка чтения ZIP-архива: {archive.FullName}", exception);
                }
                catch (IOException exception)
                {
                    statistics.AddArchiveError(CreateError(archive.FullName, "Архив", exception));
                    _logger.Error($"Ошибка доступа к ZIP-архиву: {archive.FullName}", exception);
                }
                catch (UnauthorizedAccessException exception)
                {
                    statistics.AddArchiveError(CreateError(archive.FullName, "Архив", exception));
                    _logger.Error($"Нет доступа к ZIP-архиву: {archive.FullName}", exception);
                }
                catch (Exception exception)
                {
                    statistics.AddArchiveError(CreateError(archive.FullName, "Архив", exception));
                    _logger.Error($"Непредвиденная ошибка обработки ZIP-архива: {archive.FullName}", exception);
                }
            }
            return statistics.ToResult();
        }

        /// <summary>Распаковывает архив и передаёт Markdown-файлы обработчику.</summary>
        /// <param name="archive">Обрабатываемый архив.</param>
        /// <param name="destinationDirectory">Каталог назначения.</param>
        /// <returns>Статистика обработки файлов архива.</returns>
        private ExportStatistics ProcessArchive(FileInfo archive, string destinationDirectory)
        {
            ExportStatistics statistics = new();
            using ExtractedArchive extractedArchive = _archiveExtractor.Extract(archive);
            _logger.Trace($"\tНайдено {extractedArchive.MarkdownFiles.Count.Of(RussianWords.Files)} Markdown");
            foreach (string sourceFile in extractedArchive.MarkdownFiles)
            {
                try
                {
                    FileOperationResult result = _markdownProcessor.Process(sourceFile, destinationDirectory);
                    statistics.Add(result);
                }
                catch (FormatException ex)
                {
                    statistics.AddMarkdownError(CreateError(sourceFile, "Markdown", ex));
                    _logger.Error($"Ошибка при обработке файла: {sourceFile}", ex);
                }
                catch (IOException ex)
                {
                    statistics.AddMarkdownError(CreateError(sourceFile, "Markdown", ex));
                    _logger.Error($"Ошибка при обработке файла: {sourceFile}", ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    statistics.AddMarkdownError(CreateError(sourceFile, "Markdown", ex));
                    _logger.Error($"Ошибка при обработке файла: {sourceFile}", ex);
                }
                catch (Exception ex)
                {
                    statistics.AddMarkdownError(CreateError(sourceFile, "Markdown", ex));
                    _logger.Error($"Непредвиденная ошибка обработки Markdown-файла: {sourceFile}", ex);
                }
            }
            return statistics;
        }

        /// <summary>Создаёт структурированное описание ошибки обработки.</summary>
        /// <param name="path">Путь к объекту, при обработке которого произошла ошибка.</param>
        /// <param name="stage">Этап обработки.</param>
        /// <param name="exception">Возникшее исключение.</param>
        /// <returns>Структурированное описание ошибки.</returns>
        private static ExportError CreateError(string path, string stage, Exception exception) =>
            new(path, exception.GetType().Name, exception.Message, stage);
    }
}