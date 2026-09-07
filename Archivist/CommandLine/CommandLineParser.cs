namespace dRz.GPT_Utilities.Archivist.CommandLine
{
    /// <summary>Разбирает параметры командной строки GPT_Archivist.</summary>
    internal static class CommandLineParser
    {
        /// <summary>Разбирает параметры командной строки.</summary>
        /// <param name="args">Аргументы командной строки, переданные в <c>Program.Main</c>.</param>
        /// <returns>Объект с разобранными параметрами.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> равен <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Отсутствует обязательное значение или указан неизвестный параметр.</exception>
        public static CommandLineOptions Parse(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            if (args.Length == 0)
                return new CommandLineOptions { ShowHelp = true };

            string? sourceDirectory = null;
            string? destinationDirectory = null;
            string? zipFilePattern = null;
            string? maintenanceDirectory = null;
            bool extractAll = false;

            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i].Trim().ToLowerInvariant();
                switch (argument)
                {
                    case "-h":
                    case "--help":
                    case "/?":
                        return new CommandLineOptions { ShowHelp = true };
                    case "-s":
                    case "--source":
                        sourceDirectory = ReadValue(args, ref i, argument);
                        break;
                    case "--maintenance":
                    case "-m":
                        maintenanceDirectory = ReadValue(args, ref i, argument);
                        break;
                    case "-d":
                    case "--destination":
                        destinationDirectory = ReadValue(args, ref i, argument);
                        break;
                    case "-p":
                    case "--pattern":
                        zipFilePattern = ReadValue(args, ref i, argument);
                        break;
                    case "-a":
                    case "--all":
                        extractAll = true;
                        break;
                    default:
                        throw new ArgumentException($"Неизвестный параметр: {argument}");
                }
            }

            if (!string.IsNullOrWhiteSpace(maintenanceDirectory))
                return new CommandLineOptions { MaintenanceDirectory = maintenanceDirectory };

            if (string.IsNullOrWhiteSpace(sourceDirectory))
                throw new ArgumentException("Не указан каталог с ZIP-архивами. Используй параметр -s или --source.");
            if (string.IsNullOrWhiteSpace(destinationDirectory))
                throw new ArgumentException("Не указан каталог назначения. Используй параметр -d или --destination.");

            return new CommandLineOptions
            {
                SourceDirectory = sourceDirectory,
                DestinationDirectory = destinationDirectory,
                MaintenanceDirectory = maintenanceDirectory ?? string.Empty,
                ExtractAll = extractAll,
                ZipFilePattern = zipFilePattern ?? string.Empty
            };
        }

        /// <summary>Читает значение параметра, расположенное следующим аргументом.</summary>
        /// <param name="args">Все аргументы командной строки.</param>
        /// <param name="index">Индекс текущего параметра; после чтения значения увеличивается на один.</param>
        /// <param name="parameterName">Имя параметра, для которого требуется значение.</param>
        /// <returns>Значение параметра.</returns>
        /// <exception cref="ArgumentException">Значение отсутствует или вместо него указан другой параметр.</exception>
        private static string ReadValue(string[] args, ref int index, string parameterName)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException($"Для параметра {parameterName} не указано значение.");

            string value = args[++index];
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith("-", StringComparison.Ordinal))
                throw new ArgumentException($"Для параметра {parameterName} не указано значение.");

            return value;
        }
    }
}