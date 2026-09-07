using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using dRz.GPT_Utilities.Archivist.Files;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace dRz.GPT_Utilities.Archivist.Export
{
    /// <summary>Читает метаданные разговора из YAML front matter Markdown-файла.</summary>
    internal sealed class ChatMetadataReader : IChatMetadataReader
    {
        private static readonly IDeserializer YamlDeserializer =
            new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

        /// <summary>Регулярное выражение для выделения YAML front matter в начале файла.</summary>
        internal static readonly Regex FrontMatterRegex = new(
            @"\A---\s*\r?\n(?<yaml>.*?)\r?\n---\s*(?:\r?\n|$)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly IFileSystem _fileSystem;

        /// <summary>Создаёт средство чтения метаданных.</summary>
        /// <param name="fileSystem">Файловая система для чтения Markdown-файла.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> равен <see langword="null"/>.</exception>
        public ChatMetadataReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>Читает и разбирает метаданные из YAML front matter.</summary>
        /// <param name="filePath">Путь к Markdown-файлу.</param>
        /// <returns>Прочитанные метаданные разговора.</returns>
        /// <exception cref="FormatException">Front matter отсутствует либо содержит некорректный YAML или даты.</exception>
        public ChatMetadata Read(string filePath)
        {
            StringBuilder frontMatter = new();
            foreach (string line in _fileSystem.ReadLines(filePath))
            {
                frontMatter.AppendLine(line);
                if (line.Trim() == "---" && frontMatter.Length > line.Length + Environment.NewLine.Length)
                    break;
            }

            Match match = FrontMatterRegex.Match(frontMatter.ToString());
            if (!match.Success)
                throw new FormatException($"В файле отсутствует YAML front matter: {filePath}");

            RawChatMetadata? rawMetadata;
            try
            {
                rawMetadata = YamlDeserializer.Deserialize<RawChatMetadata>(match.Groups["yaml"].Value);
            }
            catch (YamlException exception)
            {
                throw new FormatException($"Некорректный YAML или дата в файле: {filePath}", exception);
            }

            if (rawMetadata is null)
                throw new FormatException($"Не удалось прочитать YAML: {filePath}");

            string? createTimeText = rawMetadata.CreateTime?.Trim();
            if (string.IsNullOrWhiteSpace(createTimeText) ||
                !DateTimeOffset.TryParse(createTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset createTime))
                throw new FormatException($"В YAML отсутствует или некорректен create_time: {filePath}");

            string? updateTimeText = rawMetadata.UpdateTime?.Trim();
            DateTimeOffset? updateTime = null;
            if (!string.IsNullOrWhiteSpace(updateTimeText))
            {
                if (!DateTimeOffset.TryParse(updateTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsedUpdateTime))
                    throw new FormatException($"В YAML указана некорректная дата update_time: {filePath}");
                updateTime = parsedUpdateTime;
            }

            return new ChatMetadata
            {
                CreateTime = createTime,
                CreateTimeText = createTimeText,
                UpdateTime = updateTime,
                UpdateTimeText = updateTimeText,
                HasUpdateTime = rawMetadata.UpdateTime is not null,
                Model = rawMetadata.Model,
                ModelName = rawMetadata.ModelName,
                DateExport = rawMetadata.DateExport,
                ChatLink = rawMetadata.ChatLink,
                ConversationId = rawMetadata.ConversationId,
                Title = rawMetadata.Title,
                Tags = rawMetadata.Tags ?? new List<string?>(),
                HasTags = rawMetadata.Tags is not null,
                Aliases = rawMetadata.Aliases ?? new List<string?>(),
                HasAliases = rawMetadata.Aliases is not null
            };
        }

        /// <summary>Внутреннее представление YAML-метаданных перед преобразованием в <see cref="ChatMetadata"/>.</summary>
        private sealed class RawChatMetadata
        {
            /// <summary>Исходное значение времени создания.</summary>
            public string? CreateTime { get; set; }
            /// <summary>Исходное значение времени обновления.</summary>
            public string? UpdateTime { get; set; }
            /// <summary>Идентификатор модели.</summary>
            public string? Model { get; set; }
            /// <summary>Отображаемое имя модели.</summary>
            public string? ModelName { get; set; }
            /// <summary>Дата экспорта.</summary>
            public string? DateExport { get; set; }
            /// <summary>Ссылка на разговор.</summary>
            public string? ChatLink { get; set; }
            /// <summary>Идентификатор разговора.</summary>
            public Guid? ConversationId { get; set; }
            /// <summary>Заголовок разговора.</summary>
            public string? Title { get; set; }
            /// <summary>Теги разговора.</summary>
            public List<string?>? Tags { get; set; }
            /// <summary>Альтернативные имена разговора.</summary>
            public List<string?>? Aliases { get; set; }
        }
    }
}