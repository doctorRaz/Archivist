# GPT Utilities

## Сборник утилит для работы с экспортом из ChatGPT

- [Archivist](./Archivist/README.md) — утилита для обработки Markdown-файлов, экспортированных с помощью **[chatgpt-exporter](https://github.com/pionxzh/chatgpt-exporter)**.

<details>
<summary>Legacy</summary>

Исторические проекты, которые больше не входят в текущую разработку, сохранены в ветке [`archive/legacy-GPT_Utilities`](https://github.com/doctorRaz/Archivist/tree/archive/legacy-GPT_Utilities).

### MoveDuplicate 

[README](https://github.com/doctorRaz/Archivist/blob/archive/legacy-GPT_Utilities/MoveDuplicate/README.md)

Консольная утилита для автоматического перемещения файлов из корневого каталога в соответствующие подкаталоги по совпадению имени файла.

Поддерживает:

- рекурсивный поиск;
- игнорирование префикса даты `yyyy-MM-dd_`;
- сравнение файлов по `LastWriteTime` или `CreationTime`;
- режим предварительной проверки `/dryrun`;
- подробный вывод `/verbose`;
- замену существующих файлов;
- итоговую статистику и коды возврата.

### ConvovizRenamer

[README](https://github.com/doctorRaz/Archivist/blob/archive/legacy-GPT_Utilities/ConvovizRenamer/README.md)

Утилита для обработки Markdown-файлов, экспортированных из **Convoviz**.

Использует первый элемент YAML `aliases` как имя файла, синхронизирует YAML `title` и исправляет ссылки в `_index.md`.

Поддерживает:

- рекурсивную обработку каталогов;
- переименование Markdown-файлов;
- сохранение Unicode и кириллицы;
- удаление недопустимых символов из имён файлов;
- обнаружение конфликтов;
- двухпроходную обработку;
- исправление ссылок в `_index.md`.

### NexusRenamer

[README](https://github.com/doctorRaz/Archivist/blob/archive/legacy-GPT_Utilities/NexusRenamer/README.md)

Утилита для исправления имён Markdown-файлов, экспортированных из **Nexus**.

Переименовывает файлы по значению заголовка `# Title:` и исправляет соответствующие wiki-ссылки.

Основной сценарий — подготовка экспорта ChatGPT/Nexus для дальнейшего использования в Obsidian и других Markdown-хранилищах.

### GPTJson2Md

[README](https://github.com/doctorRaz/Archivist/blob/archive/legacy-GPT_Utilities/Json2Md/README.md)

Минималистичная консольная утилита для преобразования JSON-экспорта ChatGPT в Markdown.

Основные возможности:

- JSON → Markdown;
- сохранение исходного текста и кода;
- YAML frontmatter с метаданными;
- имя файла в формате `YYYY-MM-DD_Название-чата.md`;
- дата создания по первому сообщению;
- дата изменения по последнему сообщению;
- интерактивный режим;
- импорт в Obsidian, Logseq и другие Markdown-хранилища.

Разработка **GPTJson2Md** прекращена в августе 2026 года. Проект сохранён исключительно для исторических целей.

</details>
