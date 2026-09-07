namespace dRz.GPT_Utilities.Archivist.Localization
{
    /// <summary>Набор русских существительных, используемых для форматирования статистики.</summary>
    public static class RussianWords
    {
        /// <summary>Формы существительного «архив».</summary>
        public static readonly RussianPluralForms Archives =
            new("архив", "архива", "архивов");

        /// <summary>Формы существительного «файл».</summary>
        public static readonly RussianPluralForms Files =
            new("файл", "файла", "файлов");
    }
}