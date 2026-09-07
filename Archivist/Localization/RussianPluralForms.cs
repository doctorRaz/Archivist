namespace dRz.GPT_Utilities.Archivist.Localization
{
    /// <summary>Формы существительного для трёх вариантов русского склонения по числу.</summary>
    /// <param name="One">Форма для чисел, оканчивающихся на 1, кроме чисел 11–19.</param>
    /// <param name="Few">Форма для чисел, оканчивающихся на 2, 3 или 4, кроме чисел 12–19.</param>
    /// <param name="Many">Форма для остальных чисел.</param>
    public readonly record struct RussianPluralForms(
        string One,
        string Few,
        string Many);
}