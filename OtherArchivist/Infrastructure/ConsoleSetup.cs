namespace dRz.GPT_Utilities.Archivist.Infrastructure
{
    /// <summary>Настраивает параметры консоли Archivist.</summary>
    public static class ConsoleSetup
    {
        /// <summary>Настраивает заголовок и фон консольного окна.</summary>
        /// <param name="title">Необязательный заголовок консольного окна.</param>
        public static void Configure(string? title = null)
        {
            if (title != null)
                Console.Title = title;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }
    }
}

//Console.OutputEncoding = Encoding.UTF8;
//Console.InputEncoding = Encoding.UTF8;