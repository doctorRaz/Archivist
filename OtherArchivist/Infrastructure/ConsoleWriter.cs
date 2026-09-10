namespace dRz.GPT_Utilities.Archivist.Infrastructure
{
    /// <summary>Выводит диагностические сообщения в консоль.</summary>
    public static class ConsoleWriter
    {
        /// <summary>Выводит сообщение об ошибке.</summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="ex">Необязательное исключение.</param>
        public static void Error(string message, Exception? ex = null) =>
            WriteLine(Format(message, ex), ConsoleColor.Red);

        /// <summary>Выводит критическую ошибку с контрастным фоном.</summary>
        /// <param name="ex">Исключение, описывающее критическую ошибку.</param>
        /// <param name="message">Необязательное сообщение.</param>
        public static void Fatal(Exception ex, string? message = null) => WriteLine(
            Format(message, ex, "Fatal error"), ConsoleColor.White, ConsoleColor.DarkRed);

        /// <summary>Выводит информационное сообщение.</summary>
        /// <param name="message">Текст сообщения.</param>
        public static void Info(string message) => WriteLine(message, ConsoleColor.Gray);

        /// <summary>Ожидает нажатия клавиши в интерактивной консоли.</summary>
        public static void PressAnyKey()
        {
            if (Console.IsInputRedirected || Console.IsOutputRedirected)
                return;

            Info("");
            Info("Press any key...");
            _ = Console.ReadKey();
        }

        /// <summary>Выводит трассировочное сообщение.</summary>
        /// <param name="message">Текст сообщения.</param>
        public static void Trace(string message) => WriteLine(message, ConsoleColor.DarkGray);

        /// <summary>Выводит сообщение об успешном выполнении.</summary>
        /// <param name="message">Текст сообщения.</param>
        public static void Success(string message) => WriteLine(message, ConsoleColor.Green);

        /// <summary>Выводит сообщение об обновлении.</summary>
        /// <param name="message">Текст сообщения.</param>
        public static void Update(string message) => WriteLine(message, ConsoleColor.Cyan);

        /// <summary>Выводит предупреждение.</summary>
        /// <param name="message">Текст предупреждения.</param>
        public static void Warn(string message) => WriteLine(message, ConsoleColor.Magenta);

#if DEBUG
        /// <summary>Форматирует сообщение и исключение для диагностического вывода.</summary>
        /// <param name="userMessage">Сообщение пользователя.</param>
        /// <param name="ex">Исключение.</param>
        /// <param name="defaultMessage">Сообщение по умолчанию.</param>
        /// <returns>Отформатированное сообщение.</returns>
        internal static string Format(string? userMessage, Exception? ex, string defaultMessage = "Error")
#else
        private static string Format(string? userMessage, Exception? ex, string defaultMessage = "Error")
#endif
        {
            if (ex == null && string.IsNullOrEmpty(userMessage))
                return defaultMessage;

            List<string> parts = new();
            if (!string.IsNullOrEmpty(userMessage))
                parts.Add(userMessage);
            if (ex != null)
            {
                parts.Add($"Exception: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    parts.Add($"StackTrace: {ex.StackTrace}");
            }
            return string.Join(Environment.NewLine, parts);
        }

#if DEBUG
        /// <summary>Возвращает контрастный цвет текста для заданного цвета фона.</summary>
        /// <param name="background">Цвет фона.</param>
        /// <returns>Цвет текста, контрастный фону.</returns>
        internal static ConsoleColor GetContrastColor(ConsoleColor background)
#else
        private static ConsoleColor GetContrastColor(ConsoleColor background)
#endif
        {
            return background switch
            {
                ConsoleColor.Gray or ConsoleColor.White or ConsoleColor.Yellow => ConsoleColor.Black,
                _ => ConsoleColor.White
            };
        }

#if DEBUG
        /// <summary>Выводит строку с указанными цветами.</summary>
        /// <param name="message">Текст строки.</param>
        /// <param name="foreground">Необязательный цвет текста.</param>
        /// <param name="background">Необязательный цвет фона.</param>
        internal static void WriteLine(string message, ConsoleColor? foreground = null, ConsoleColor? background = null)
#else
        private static void WriteLine(string message, ConsoleColor? foreground = null, ConsoleColor? background = null)
#endif
        {
            ConsoleColor previousForeground = Console.ForegroundColor;
            ConsoleColor previousBackground = Console.BackgroundColor;
            try
            {
                if (foreground.HasValue)
                    Console.ForegroundColor = foreground.Value;
                if (background.HasValue)
                    Console.BackgroundColor = background.Value;
                if (Console.ForegroundColor == Console.BackgroundColor)
                    Console.ForegroundColor = GetContrastColor(Console.BackgroundColor);
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previousForeground;
                Console.BackgroundColor = previousBackground;
            }
        }
    }
}