using dRz.GPT_Utilities.Archivist.CommandLine;
using dRz.GPT_Utilities.Archivist.Export;
using dRz.GPT_Utilities.Archivist.Infrastructure;
using dRz.GPT_Utilities.Archivist.Files;
using dRz.GPT_Utilities.Archivist.Localization;
using dRz.GPT_Utilities.Archivist.Maintenance;

namespace dRz.GPT_Utilities.Archivist
{
    /// <summary>Координирует запуск Archivist и выполнение выбранного режима работы.</summary>
    internal sealed class ArchivistApplication
    {
        private readonly CommandLineOptionsValidator _validator;
        private readonly IChatGptExportProcessor _processor;
        private readonly IFileSystem _fileSystem;
        private readonly ArchiveMaintenance _maintenance;

        /// <summary>Создаёт экземпляр приложения.</summary>
        /// <param name="validator">Валидатор параметров командной строки.</param>
        /// <param name="processor">Процессор экспорта ChatGPT.</param>
        public ArchivistApplication(CommandLineOptionsValidator validator, IChatGptExportProcessor processor)
            : this(validator, processor, new LocalFileSystem())
        {
        }

        /// <summary>Создаёт экземпляр приложения с заданной файловой системой.</summary>
        /// <param name="validator">Валидатор параметров командной строки.</param>
        /// <param name="processor">Процессор экспорта ChatGPT.</param>
        /// <param name="fileSystem">Файловая система приложения.</param>
        public ArchivistApplication(
            CommandLineOptionsValidator validator,
            IChatGptExportProcessor processor,
            IFileSystem fileSystem)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            ChatMetadataReader metadataReader = new(_fileSystem);
            _maintenance = new ArchiveMaintenance(
                _fileSystem,
                new FileNameNormalizer(),
                new DirectoryIndexWriter(
                    _fileSystem,
                    new ConversationDisplayNameProvider(metadataReader)));
        }

        /// <summary>Разбирает параметры и выполняет выбранный режим работы приложения.</summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>Код завершения приложения: 0 при успешном выполнении и 1 при ошибке.</returns>
        internal int Run(string[] args)
        {
            CommandLineOptions options = CommandLineParser.Parse(args);

            if (options.ShowHelp)
            {
                ShowHelp();
                return SuccessExitCode;
            }

            if (options.IsMaintenance)
            {
                ArchiveMaintenanceResult result = _maintenance.Run(options.MaintenanceDirectory);
                PrintMaintenanceStatistics(result);
                return result.Errors == 0 ? SuccessExitCode : ErrorExitCode;
            }

            options = _validator.Validate(options);
            ValidateDirectories(options);
            ExportRequest request = new(
                options.SourceDirectory,
                options.DestinationDirectory,
                options.ZipFilePattern,
                options.ExtractAll);

            ExportResult statistics = _processor.Process(request);
            PrintStatistics(statistics);
            ConsoleWriter.PressAnyKey();

            return statistics.Failed > 0 || statistics.ArchiveFailed > 0
                ? ErrorExitCode
                : SuccessExitCode;
        }

        /// <summary>Выводит статистику режима обслуживания.</summary>
        /// <param name="result">Результат обслуживания.</param>
        private static void PrintMaintenanceStatistics(ArchiveMaintenanceResult result)
        {
            ConsoleWriter.Success("================ MAINTENANCE =========================");
            ConsoleWriter.Trace($"Проверено файлов: {result.CheckedFiles}");
            ConsoleWriter.Update($"Переименовано файлов: {result.RenamedFiles}");
            ConsoleWriter.Warn($"Конфликтов: {result.Conflicts}");
            ConsoleWriter.Trace($"Обновлено индексов: {result.UpdatedIndexes}");
            ConsoleWriter.Error($"Ошибок: {result.Errors}");
            ConsoleWriter.Success("=======================================================");
        }

        /// <summary>Проверяет исходный каталог и создаёт каталог назначения при необходимости.</summary>
        /// <param name="options">Проверенные параметры приложения.</param>
        /// <exception cref="DirectoryNotFoundException">Исходный каталог не существует.</exception>
        private void ValidateDirectories(CommandLineOptions options)
        {
            if (!_fileSystem.DirectoryExists(options.SourceDirectory))
                throw new DirectoryNotFoundException($"Каталог с архивами не найден: {options.SourceDirectory}");

            _fileSystem.CreateDirectory(options.DestinationDirectory);
        }

        /// <summary>Выводит итоговую статистику обработки экспорта.</summary>
        /// <param name="statistics">Статистика обработки.</param>
        private static void PrintStatistics(ExportResult statistics)
        {
            ConsoleWriter.Success("================ TOTAL STATISTICS =====================");
            ConsoleWriter.Trace($"Обработано всего: {statistics.Total.Of(RussianWords.Files)}");
            ConsoleWriter.Trace("Из них:");
            ConsoleWriter.Success($"\tДобавлено {statistics.Added.Of(RussianWords.Files)}");
            ConsoleWriter.Update($"\tОбновлено {statistics.Updated.Of(RussianWords.Files)}");
            ConsoleWriter.Trace($"\tПропущено {statistics.Skipped.Of(RussianWords.Files)}");
            ConsoleWriter.Error($"\tОшибок {statistics.Failed.Of(RussianWords.Files)}");
            ConsoleWriter.Error($"\tОшибок архивов {statistics.ArchiveFailed.Of(RussianWords.Archives)}");
            int addedOrUpdated = statistics.Added + statistics.Updated;
            ConsoleWriter.Info($"Всего заменено и добавлено {addedOrUpdated.Of(RussianWords.Files)}");
            ConsoleWriter.Success("=======================================================");
        }

        /// <summary>Выводит справку по использованию приложения.</summary>
        private static void ShowHelp()
        {
            CommandLineHelp.Print();
            ConsoleWriter.PressAnyKey();
        }

        /// <summary>Код успешного завершения приложения.</summary>
        private const int ErrorExitCode = 1;
        /// <summary>Код завершения приложения при ошибке.</summary>
        private const int SuccessExitCode = 0;
    }
}