namespace dRz.GPT_Utilities.Archivist.Export;

/// <summary>Читает метаданные разговора из Markdown-файла.</summary>
internal interface IChatMetadataReader
{
    /// <summary>Читает и разбирает YAML front matter указанного файла.</summary>
    /// <param name="filePath">Путь к Markdown-файлу.</param>
    /// <returns>Метаданные разговора.</returns>
    ChatMetadata Read(string filePath);
}
