using System.Globalization;
using YamlDotNet.Serialization;

namespace dRz.GPT_Utilities.Archivist.Export
{
    /// <summary>Метаданные Markdown-файла из экспорта ChatGPT.</summary>
    internal sealed class ChatMetadata
    {
        /// <summary>Дата и время создания разговора.</summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>Исходное текстовое представление <c>create_time</c>.</summary>
        [YamlIgnore]
        internal string? CreateTimeText { get; set; }

        /// <summary>Дата и время последнего изменения разговора.</summary>
        public DateTimeOffset? UpdateTime { get; set; }

        /// <summary>Исходное текстовое представление <c>update_time</c>.</summary>
        [YamlIgnore]
        internal string? UpdateTimeText { get; set; }

        /// <summary>Признак наличия поля <c>update_time</c> в исходном YAML.</summary>
        [YamlIgnore]
        internal bool HasUpdateTime { get; set; }

        /// <summary>Идентификатор модели ChatGPT.</summary>
        public string? Model { get; set; }

        /// <summary>Отображаемое имя модели ChatGPT.</summary>
        public string? ModelName { get; set; }

        /// <summary>Дата экспорта.</summary>
        public string? DateExport { get; set; }

        /// <summary>Ссылка на разговор ChatGPT.</summary>
        public string? ChatLink { get; set; }

        /// <summary>Название разговора.</summary>
        public string? Title { get; set; }

        /// <summary>Теги разговора.</summary>
        public List<string?> Tags { get; set; } = new();

        /// <summary>Признак наличия поля <c>tags</c> в исходном YAML.</summary>
        [YamlIgnore]
        internal bool HasTags { get; set; }

        /// <summary>Псевдонимы разговора.</summary>
        public List<string?> Aliases { get; set; } = new();

        /// <summary>Признак наличия поля <c>aliases</c> в исходном YAML.</summary>
        [YamlIgnore]
        internal bool HasAliases { get; set; }

        /// <summary>Возвращает дату экспорта, преобразованную из строкового значения <see cref="DateExport"/>.</summary>
        [YamlIgnore]
        public DateTime? ExportDateTime
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DateExport))
                    return null;
                return DateTime.ParseExact(
                    DateExport,
                    "yyyy-MM-dd'T'HH-mm-ss",
                    CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Уникальный идентификатор разговора.
        /// Явно заданное значение имеет приоритет; если оно отсутствует, идентификатор извлекается из <see cref="ChatLink"/>.
        /// </summary>
        [YamlIgnore]
        public Guid? ConversationId
        {
            get => _conversationId ?? ParseConversationId(ChatLink);
            set => _conversationId = value;
        }

        /// <summary>Явно сохранённый идентификатор разговора.</summary>
        private Guid? _conversationId;

        /// <summary>
        /// Извлекает идентификатор разговора из ссылки поддерживаемого сервиса.
        /// Идентификатором считается последний непустой сегмент пути, если он является GUID.
        /// </summary>
        /// <param name="chatLink">Ссылка на разговор.</param>
        /// <returns>Идентификатор разговора или <see langword="null"/>, если ссылка не поддерживается или последний сегмент пути не является GUID.</returns>
        private static Guid? ParseConversationId(string? chatLink)
        {
            if (string.IsNullOrWhiteSpace(chatLink) ||
                !Uri.TryCreate(chatLink, UriKind.Absolute, out Uri? uri) ||
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                return null;

            // Пока Archivist поддерживает ChatGPT и DeepSeek. Неизвестные домены
            // не должны случайно превращать произвольный GUID из URL в ConversationId.
            bool isSupportedHost =
                string.Equals(uri.Host, "chatgpt.com", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Host, "chat.deepseek.com", StringComparison.OrdinalIgnoreCase);

            if (!isSupportedHost)
                return null;

            // Берём только путь URL: query string и fragment не могут повлиять на идентификатор.
            string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return null;

            return Guid.TryParse(segments[^1], out Guid id) ? id : null;
        }
    }
}