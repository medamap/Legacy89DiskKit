namespace Legacy89DiskKit.Cli.Presentation.FileSystem;

public class FootnoteRegistry
{
    private readonly Dictionary<string, int> _numbersByText = new();
    private readonly List<FileListFootnote> _footnotes = new();

    public string Register(params string[] notes)
    {
        var numbers = new List<int>();

        foreach (var note in notes.Where(note => !string.IsNullOrWhiteSpace(note)).Distinct())
        {
            if (!_numbersByText.TryGetValue(note, out var number))
            {
                number = _footnotes.Count + 1;
                _numbersByText[note] = number;
                _footnotes.Add(new FileListFootnote(number, note));
            }

            numbers.Add(number);
        }

        return string.Concat(numbers.OrderBy(number => number).Select(number => $"*{number}"));
    }

    public IReadOnlyList<FileListFootnote> ToList()
    {
        return _footnotes;
    }
}
