using dRz.GPT_Utilities.Archivist.Infrastructure;

namespace dRz.GPT_Utilities.Archivist.CommandLine
{
    /// <summary>Выводит справочную информацию по использованию Archivist.</summary>
    internal class CommandLineHelp
    {
        /// <summary>Выводит справку по использованию программы.</summary>
        public static void Print()
        {
            ConsoleWriter.Info("""
    Archivist — обработка архивов экспорта ChatGPT

    Использование:
      Archivist -s <каталог> -d <каталог> [опции]

    Параметры:

      -s, --source <каталог>
          Каталог с ZIP-архивами экспорта ChatGPT.
          Каталог должен существовать.

      -d, --destination <каталог>
          Каталог для распаковки архивов.
          Если каталог отсутствует, он будет создан.

      -p, --pattern <маска>
          Маска ZIP-файлов для обработки.
          По умолчанию: *.zip.
          Поддерживается стандартная маска Directory.EnumerateFiles.

      -m, --maintenance <каталог>
          Нормализовать существующий vault и перестроить _index.md.
          Архивы не обрабатываются.

    Опции:

      -a, --all
          Обработать все ZIP-архивы.
          По умолчанию обрабатывается только последний архив.

      -h, --help, /?
          Показать эту справку.

    Примеры:

      Archivist -s "D:\GPT\Archives" -d "D:\GPT\Unpacked"

      Archivist -s "D:\GPT\Archives" -d "D:\GPT\Unpacked" -a

      Archivist --source "D:\GPT\Archives" --destination "D:\GPT\Unpacked" --all

      Archivist -s "D:\GPT\Archives" -d "D:\GPT\Unpacked" -p "*.zip"

      Archivist --maintenance "D:\GPT\Unpacked"

      Archivist -m "D:\GPT\Unpacked"

    """);
        }
    }
}
