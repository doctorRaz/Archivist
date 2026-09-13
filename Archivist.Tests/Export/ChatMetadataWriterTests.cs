using System;
using System.IO;
using dRz.GPT_Utilities.Archivist.Export;
using dRz.GPT_Utilities.Archivist.Files;
using dRz.GPT_Utilities.Archivist.Tests.Infrastructure;
using NUnit.Framework;

namespace dRz.GPT_Utilities.Archivist.Tests.Export
{
    /// <summary>
    /// Проверяет сериализацию и финальное состояние времени записи метаданных разговора.
    /// </summary>
    [TestFixture]
    public sealed class ChatMetadataWriterTests
    {
        private static readonly DateTimeOffset CreateTime =
            new(2026, 9, 3, 5, 21, 1, 317, TimeSpan.Zero);

        private static readonly DateTimeOffset UpdateTime =
            new(2026, 9, 3, 5, 26, 14, 558, TimeSpan.Zero);

        private static readonly DateTimeOffset UpdateTimeWithOffset =
            new(2026, 9, 3, 8, 26, 14, 558, TimeSpan.FromHours(3));

        /// <summary>
        /// Проверяет, что запись метаданных завершает обработку файла временем UpdateTime.
        /// </summary>
        [Test]
        public void Write_SetsLastWriteTimeFromUpdateTime_AsFinalFileOperation()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\ntitle: Test\n---\nBody\n");

            // Задаём заведомо другое время, чтобы проверить именно финальную установку
            // времени после перезаписи YAML, а не случайное совпадение с системным временем.
            File.SetLastWriteTime(path, CreateTime.AddDays(-1).LocalDateTime);

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                UpdateTime = UpdateTime,
                UpdateTimeText = "2026-09-03T05:26:14.558Z",
                HasUpdateTime = true,
                Title = "Test"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            Assert.That(File.ReadAllText(path), Does.Contain("update_time: \"2026-09-03T05:26:14.558Z\""));
            Assert.That(
                File.GetLastWriteTime(path),
                Is.EqualTo(UpdateTime.LocalDateTime).Within(TimeSpan.FromSeconds(2)));
        }

        /// <summary>
        /// Проверяет перевод DateTimeOffset со смещением в локальное время на границе файловой системы.
        /// </summary>
        [Test]
        public void Write_UsesLocalDateTimeForLastWriteTime_WhenUpdateTimeHasOffset()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\ntitle: Test\n---\nBody\n");

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                UpdateTime = UpdateTimeWithOffset,
                UpdateTimeText = "2026-09-03T08:26:14.558+03:00",
                HasUpdateTime = true,
                Title = "Test"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            // Проверяем именно локальное представление того же момента времени.
            // Это отличает LocalDateTime от UtcDateTime и фиксирует контракт файловой системы.
            Assert.That(
                File.GetLastWriteTime(path),
                Is.EqualTo(UpdateTimeWithOffset.LocalDateTime).Within(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void Write_SerializesConversationId_FromChatLink()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            const string conversationId = "6a990386-7394-83eb-b0c4-0fb5b6215713";
            File.WriteAllText(path, "---\n" + "title: Test\n" + "create_time: \"2026-09-03T05:21:01.317Z\"\n" + "chat_link: \"https://chatgpt.com/c/6a990386-7394-83eb-b0c4-0fb5b6215713\"\n" + "---\nBody\n");

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                ChatLink = $"https://chatgpt.com/c/{conversationId}",
                Title = "Test"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            Assert.That(File.ReadAllText(path), Does.Contain($"conversation_id: \"{conversationId}\""));
        }

        [Test]
        public void Write_ForcesDoubleQuotes_ForDateAndConversationId()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\ntitle: Test\n---\nBody\n");

            const string conversationId = "6a990386-7394-83eb-b0c4-0fb5b6215713";
            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                UpdateTime = UpdateTime,
                UpdateTimeText = "2026-09-03T05:26:14.558Z",
                HasUpdateTime = true,
                ChatLink = $"https://chatgpt.com/c/{conversationId}"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            string yaml = File.ReadAllText(path);
            Assert.That(yaml, Does.Contain("create_time: \"2026-09-03T05:21:01.317Z\""));
            Assert.That(yaml, Does.Contain("update_time: \"2026-09-03T05:26:14.558Z\""));
            Assert.That(yaml, Does.Contain($"conversation_id: \"{conversationId}\""));
        }

        [Test]
        public void Write_PreservesOptionalAliasesAndTags_WhenPresent()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\ntitle: Test\naliases:\n  - Original\ntags:\n  - archivist\n---\nBody\n");

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                Title = "Test",
                Aliases = new() { "Original" },
                HasAliases = true,
                Tags = new() { "archivist" },
                HasTags = true
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            string yaml = File.ReadAllText(path);
            Assert.That(yaml, Does.Contain("aliases:"));
            Assert.That(yaml, Does.Contain("- Original"));
            Assert.That(yaml, Does.Contain("tags:"));
            Assert.That(yaml, Does.Contain("- archivist"));
        }

        [Test]
        public void Write_DoesNotAddOptionalFields_WhenAbsent()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\ntitle: Test\n---\nBody\n");

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                Title = "Test"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            string yaml = File.ReadAllText(path);
            Assert.That(yaml, Does.Not.Contain("aliases:"));
            Assert.That(yaml, Does.Not.Contain("tags:"));
            Assert.That(yaml, Does.Not.Contain("update_time:"));
            Assert.That(yaml, Does.Not.Contain("chat_link:"));
            Assert.That(yaml, Does.Not.Contain("conversation_id:"));
        }

        [Test]
        public void Write_PreservesMarkdownBody_Unchanged()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            const string body = "# Header\n\nBody with **markdown**.\n";
            File.WriteAllText(path, "---\ntitle: Test\n---\n" + body);

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                Title = "Test"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            Assert.That(File.ReadAllText(path), Does.EndWith(body));
        }

        [Test]
        public void Write_DoesNotLeaveCarriageReturnInsideQuotedScalars()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\r\ntitle: Test\r\n---\r\nBody\r\n");

            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                Title = "Test"
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            string result = File.ReadAllText(path);
            Assert.That(result, Does.Contain("create_time: \"2026-09-03T05:21:01.317Z\"\r\n"));
            Assert.That(result, Does.Not.Contain("Z\"\r\r\n"));
        }

        [Test]
        public void Write_QuotesTitle_WhenYamlSpecialCharactersArePresent()
        {
            using TempDirectory temp = new();
            string path = Path.Combine(temp.Path, "conversation.md");
            File.WriteAllText(path, "---\ntitle: Test\n---\nBody\n");

            const string title = "Test: YAML # title [special]";
            ChatMetadata metadata = new()
            {
                CreateTime = CreateTime,
                CreateTimeText = "2026-09-03T05:21:01.317Z",
                Title = title
            };

            new ChatMetadataWriter(new LocalFileSystem()).Write(path, metadata);

            string yaml = File.ReadAllText(path);
            Assert.That(yaml, Does.Contain("title: 'Test: YAML # title [special]'"));
        }
    }
}
