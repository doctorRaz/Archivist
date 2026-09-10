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

/// <summary>Адаптер диагностического вывода через консоль.</summary>
internal sealed class ConsoleArchivistLogger : IArchivistLogger
{
    /// <summary>Записывает трассировочное сообщение через <see cref="ConsoleWriter"/>.</summary>
    /// <param name="message">Текст сообщения.</param>
    public void Trace(string message) => ConsoleWriter.Trace(message);

    /// <summary>Записывает предупреждение через <see cref="ConsoleWriter"/>.</summary>
    /// <param name="message">Текст предупреждения.</param>
    public void Warning(string message) => ConsoleWriter.Warn(message);

    /// <summary>Записывает сообщение об успешном выполнении через <see cref="ConsoleWriter"/>.</summary>
    /// <param name="message">Текст сообщения.</param>
    public void Success(string message) => ConsoleWriter.Success(message);

    /// <summary>Записывает сообщение об обновлении через <see cref="ConsoleWriter"/>.</summary>
    /// <param name="message">Текст сообщения.</param>
    public void Update(string message) => ConsoleWriter.Update(message);

    /// <summary>Записывает сообщение об ошибке через <see cref="ConsoleWriter"/>.</summary>
    /// <param name="message">Текст сообщения.</param>
    /// <param name="exception">Исключение, связанное с ошибкой, если оно есть.</param>
    public void Error(string message, Exception? exception = null) => ConsoleWriter.Error(message, exception);
}
