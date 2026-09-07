namespace dRz.GPT_Utilities.Archivist.Files;

/// <summary>Минимальная абстракция файловых операций, необходимых Archivist.</summary>
internal interface IFileSystem
{
    /// <summary>Проверяет существование файла.</summary><param name="path">Путь к файлу.</param><returns><see langword="true"/>, если файл существует.</returns>
    bool FileExists(string path);
    /// <summary>Читает весь текст файла.</summary><param name="path">Путь к файлу.</param><returns>Содержимое файла.</returns>
    string ReadAllText(string path);
    /// <summary>Записывает текст в файл.</summary><param name="path">Путь к файлу.</param><param name="contents">Содержимое файла.</param>
    void WriteAllText(string path, string contents);
    /// <summary>Читает файл построчно.</summary><param name="path">Путь к файлу.</param><returns>Перечисление строк файла.</returns>
    IEnumerable<string> ReadLines(string path);
    /// <summary>Копирует файл.</summary><param name="sourcePath">Путь к исходному файлу.</param><param name="destinationPath">Путь к целевому файлу.</param><param name="overwrite">Разрешить перезапись.</param>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);
    /// <summary>Перемещает файл.</summary><param name="sourcePath">Путь к исходному файлу.</param><param name="destinationPath">Путь к целевому файлу.</param>
    void MoveFile(string sourcePath, string destinationPath);
    /// <summary>Удаляет файл.</summary><param name="path">Путь к файлу.</param>
    void DeleteFile(string path);
    /// <summary>Пытается скопировать файл без перезаписи.</summary><param name="sourcePath">Путь к исходному файлу.</param><param name="destinationPath">Путь к целевому файлу.</param><returns><see langword="true"/>, если копирование выполнено.</returns>
    bool TryCopyFile(string sourcePath, string destinationPath);
    /// <summary>Устанавливает время последней записи файла.</summary><param name="path">Путь к файлу.</param><param name="lastWriteTime">Новое время записи.</param>
    void SetLastWriteTime(string path, DateTime lastWriteTime);
    /// <summary>Проверяет существование каталога.</summary><param name="path">Путь к каталогу.</param><returns><see langword="true"/>, если каталог существует.</returns>
    bool DirectoryExists(string path);
    /// <summary>Создаёт каталог.</summary><param name="path">Путь к каталогу.</param>
    void CreateDirectory(string path);
    /// <summary>Удаляет каталог.</summary><param name="path">Путь к каталогу.</param><param name="recursive">Удалять ли содержимое рекурсивно.</param>
    void DeleteDirectory(string path, bool recursive);
    /// <summary>Перечисляет файлы в каталоге.</summary><param name="path">Путь к каталогу.</param><param name="searchPattern">Шаблон поиска.</param><param name="searchOption">Область поиска.</param><returns>Перечисление путей к файлам.</returns>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    /// <summary>Перечисляет каталоги.</summary><param name="path">Путь к каталогу.</param><param name="searchOption">Область поиска.</param><returns>Перечисление путей к каталогам.</returns>
    IEnumerable<string> EnumerateDirectories(string path, SearchOption searchOption);
}

/// <summary>Реализация файловых операций средствами <see cref="System.IO"/>.</summary>
internal sealed class LocalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public string ReadAllText(string path) => File.ReadAllText(path);
    public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
    public IEnumerable<string> ReadLines(string path) => File.ReadLines(path);
    public void CopyFile(string sourcePath, string destinationPath, bool overwrite) => File.Copy(sourcePath, destinationPath, overwrite);
    public void MoveFile(string sourcePath, string destinationPath) => File.Move(sourcePath, destinationPath);
    public bool TryCopyFile(string sourcePath, string destinationPath)
    {
        try { File.Copy(sourcePath, destinationPath, overwrite: false); return true; }
        catch (IOException) when (File.Exists(destinationPath)) { return false; }
    }
    public void SetLastWriteTime(string path, DateTime lastWriteTime) => File.SetLastWriteTime(path, lastWriteTime);
    public void DeleteFile(string path) => File.Delete(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateFiles(path, searchPattern, searchOption);
    public IEnumerable<string> EnumerateDirectories(string path, SearchOption searchOption) => Directory.EnumerateDirectories(path, "*", searchOption);
}