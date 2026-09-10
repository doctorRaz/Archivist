namespace dRz.GPT_Utilities.Archivist.CommandLine
{
    /// <summary>
    /// Параметры командной строки GPT_Archivist.
    /// </summary>
    internal sealed class CommandLineOptions
    {
        /// <summary>Каталог, содержащий ZIP-архивы экспорта ChatGPT.</summary>
        public string SourceDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Каталог назначения для распаковки архивов.
        /// Если каталог отсутствует, приложение должно его создать.
        /// </summary>
        public string DestinationDirectory { get; init; } = string.Empty;

        /// <summary>Каталог vault для обслуживания.</summary>
        public string MaintenanceDirectory { get; init; } = string.Empty;

        /// <summary>Возвращает признак запуска режима обслуживания vault.</summary>
        public bool IsMaintenance => !string.IsNullOrWhiteSpace(MaintenanceDirectory);

        /// <summary>
        /// Маска файлов ZIP-архивов, которые следует обрабатывать.
        /// Например: <c>chatgpt-export-markdown*.zip</c>.
        /// </summary>
        public string ZipFilePattern { get; init; } = string.Empty;

        /// <summary>
        /// Признак обработки всех найденных архивов.
        /// <c>false</c> — обрабатывается только последний архив; <c>true</c> — все архивы.
        /// </summary>
        public bool ExtractAll { get; init; }

        /// <summary>Признак запроса справки.</summary>
        public bool ShowHelp { get; init; }
    }
}