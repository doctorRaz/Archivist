namespace dRz.GPT_Utilities.Archivist.CommandLine
{
    /// <summary>Проверяет и нормализует параметры командной строки.</summary>
    internal sealed class CommandLineOptionsValidator
    {
        /// <summary>Проверяет параметры и нормализует маску ZIP-файлов.</summary>
        /// <param name="options">Параметры, подлежащие проверке.</param>
        /// <returns>Проверенные параметры с нормализованной маской ZIP-файлов.</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="options"/> равен <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Не указан обязательный каталог или маска содержит недопустимые символы.</exception>
        public CommandLineOptions Validate(CommandLineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.SourceDirectory))
            {
                throw new ArgumentException(
                    "Не указан каталог с ZIP-архивами.",
                    nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.DestinationDirectory))
            {
                throw new ArgumentException(
                    "Не указан каталог назначения.",
                    nameof(options));
            }

            string pattern = NormalizeZipPattern(options.ZipFilePattern);

            return new CommandLineOptions
            {
                SourceDirectory = options.SourceDirectory,
                DestinationDirectory = options.DestinationDirectory,
                ExtractAll = options.ExtractAll,
                ShowHelp = options.ShowHelp,
                ZipFilePattern = pattern
            };
        }

        /// <summary>Маска ZIP-файлов экспорта ChatGPT по умолчанию.</summary>
        private const string _defaultZipFilePattern = "*.zip";

        /// <summary>Проверяет маску ZIP-файлов на наличие недопустимых символов.</summary>
        /// <param name="pattern">Проверяемая маска.</param>
        /// <returns><see langword="true"/>, если маска содержит недопустимые символы.</returns>
        private static bool ContainsInvalidZipPatternCharacters(string pattern)
        {
            char[] invalidCharacters =
                { '\\', '/', ':', '"', '<', '>', '|' };

            return pattern.IndexOfAny(invalidCharacters) >= 0;
        }

        /// <summary>Нормализует маску ZIP-файлов и добавляет расширение <c>.zip</c> при необходимости.</summary>
        /// <param name="pattern">Исходная маска.</param>
        /// <returns>Нормализованная маска.</returns>
        /// <exception cref="ArgumentException">Маска содержит недопустимые символы.</exception>
        private static string NormalizeZipPattern(string? pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return _defaultZipFilePattern;
            }

            pattern = pattern.Trim();

            if (ContainsInvalidZipPatternCharacters(pattern))
            {
                throw new ArgumentException(
                    $"Недопустимая маска ZIP-файлов: {pattern}",
                    nameof(pattern));
            }

            return pattern.EndsWith(
                ".zip",
                StringComparison.OrdinalIgnoreCase)
                ? pattern
                : $"{pattern}.zip";
        }
    }
}