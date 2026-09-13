using dRz.GPT_Utilities.Archivist.Export;
using dRz.GPT_Utilities.Archivist.Files;
using dRz.GPT_Utilities.Archivist.Infrastructure;
using dRz.GPT_Utilities.Archivist.Tests.Infrastructure;
using System;
using System.IO;
using NUnit.Framework;

namespace dRz.GPT_Utilities.Archivist.Tests.Files
{
    /// <summary>
    /// Проверяет установку времени последней записи файла после синхронизации.
    /// </summary>
    public sealed class FileSynchronizerLastWriteTimeTests
    {
        private static readonly DateTimeOffset CreateTime =
            new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        private const string ConversationA =
            "https://chatgpt.com/c/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

        /// <summary>
        /// Проверяет, что при обновлении существующего файла его время последней записи
        /// устанавливается из UpdateTime входящей беседы, а не остаётся временем копирования.
        /// </summary>
        [Test]
        public void Synchronize_UpdateExistingFile_SetsLastWriteTimeFromUpdateTime()
        {
            using TempDirectory temp = new();
            string destination = MarkdownFactory.Write(
                temp.Combine("dst", "Chat.md"),
                CreateTime,
                CreateTime.AddHours(1),
                ConversationA,
                "old");

            // Явно задаём время, отличное от UpdateTime источника, чтобы тест проверял
            // именно установку времени после обновления, а не случайное совпадение.
            DateTime oldLastWriteTime = CreateTime.AddHours(-5).LocalDateTime;
            File.SetLastWriteTime(destination, oldLastWriteTime);

            DateTimeOffset updateTime = CreateTime.AddDays(3);
            string source = MarkdownFactory.Write(
                temp.Combine("src", "Chat.md"),
                CreateTime,
                updateTime,
                ConversationA,
                "new");

            ChatMetadata metadata = new ChatMetadataReader(new LocalFileSystem()).Read(source);
            FileOperationResult result = new FileSynchronizerService(
                new ChatMetadataReader(new LocalFileSystem()),
                new ConsoleArchivistLogger(),
                new UniqueFileNameProvider(new LocalFileSystem()),
                new LocalFileSystem()).Synchronize(source, destination, metadata);

            Assert.That(result.Status, Is.EqualTo(FileOperationStatus.Updated));
            Assert.That(File.ReadAllText(destination), Does.Contain("new"));

            DateTime expected = updateTime.LocalDateTime;
            DateTime actual = File.GetLastWriteTime(destination);
            Assert.That(actual, Is.EqualTo(expected).Within(TimeSpan.FromSeconds(2)));
        }
    }
}
