namespace dRz.GPT_Utilities.Archivist.Infrastructure;

/// <summary>Абстракция диагностического вывода Archivist.</summary>
internal interface IArchivistLogger
{
    /// <summary>Записывает трассировочное сообщение.</summary>
    /// <param name="message">Текст сообщения.</param>
    void Trace(string message);

    /// <summary>Записывает предупреждение.</summary>
    /// <param name="message">Текст предупреждения.</param>
    void Warning(string message);

    /// <summary>Записывает сообщение об успешном выполнении.</summary>
    /// <param name="message">Текст сообщения.</param>
    void Success(string message);

    /// <summary>Записывает сообщение об обновлении.</summary>
    /// <param name="message">Текст сообщения.</param>
    void Update(string message);

    /// <summary>Записывает сообщение об ошибке.</summary>
    /// <param name="message">Текст сообщения.</param>
    /// <param name="exception">Исключение, связанное с ошибкой, если оно есть.</param>
    void Error(string message, Exception? exception = null);
}

/// <summary>Адаптер консольного вывода приложения.</summary>
internal sealed class ConsoleArchivistLogger : IArchivistLogger
{
    public void Trace(string message) => ConsoleWriter.Trace(message);
    public void Warning(string message) => ConsoleWriter.Warn(message);
    public void Success(string message) => ConsoleWriter.Success(message);
    public void Update(string message) => ConsoleWriter.Update(message);
    public void Error(string message, Exception? exception = null) => ConsoleWriter.Error(message, exception);
}
