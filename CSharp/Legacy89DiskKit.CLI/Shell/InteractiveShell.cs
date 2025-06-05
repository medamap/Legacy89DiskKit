using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.CharacterEncoding.Application;

namespace Legacy89DiskKit.CLI.Shell;

public class InteractiveShell : IDisposable
{
    private readonly SlotManager _slotManager;
    private readonly ShellCommandHandler _commandHandler;
    private readonly TabCompletionHandler _tabCompletionHandler;
    private bool _isRunning;

    public InteractiveShell(IDiskContainerFactory diskContainerFactory, 
                           IFileSystemFactory fileSystemFactory,
                           CharacterEncodingService characterEncodingService)
    {
        _slotManager = new SlotManager(diskContainerFactory, fileSystemFactory);
        _commandHandler = new ShellCommandHandler(_slotManager, characterEncodingService, diskContainerFactory);
        _tabCompletionHandler = new TabCompletionHandler(_slotManager);
        _isRunning = false;
    }

    public void Run()
    {
        ShowWelcome();
        _isRunning = true;

        while (_isRunning)
        {
            try
            {
                var prompt = _slotManager.GetPrompt();
                var input = ReadLineWithTabCompletion(prompt);
                
                if (input == null)
                {
                    break;
                }

                var command = ShellCommand.Parse(input);
                
                if (command.Name == "exit" || command.Name == "quit")
                {
                    _isRunning = false;
                    break;
                }

                _commandHandler.ExecuteCommand(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        ShowGoodbye();
    }

    private void ShowWelcome()
    {
        Console.WriteLine("Legacy89DiskKit Interactive Shell v1.4.0");
        Console.WriteLine("Type 'help' for available commands, 'exit' to quit.");
        Console.WriteLine();
    }

    private void ShowGoodbye()
    {
        Console.WriteLine("Goodbye!");
    }

    private string? ReadLineWithTabCompletion(string prompt)
    {
        Console.Write(prompt + " ");
        
        var input = new System.Text.StringBuilder();
        var cursorPosition = 0;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return input.ToString();
            }
            else if (key.Key == ConsoleKey.Tab)
            {
                var currentInput = input.ToString();
                var completions = _tabCompletionHandler.GetCompletions(currentInput, cursorPosition);
                
                
                if (completions.Length == 1)
                {
                    var completion = completions[0];
                    ClearLine(prompt.Length + 1 + input.Length);
                    input.Clear();
                    input.Append(completion);
                    cursorPosition = completion.Length;
                    Console.Write(completion);
                }
                else if (completions.Length > 1)
                {
                    Console.WriteLine();
                    foreach (var completion in completions.Take(20))
                    {
                        Console.WriteLine("  " + completion);
                    }
                    if (completions.Length > 20)
                    {
                        Console.WriteLine($"  ... and {completions.Length - 20} more");
                    }
                    Console.Write(prompt + " " + input);
                    SetCursorPosition(prompt.Length + 1 + cursorPosition);
                }
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (cursorPosition > 0)
                {
                    input.Remove(cursorPosition - 1, 1);
                    cursorPosition--;
                    RedrawLine(prompt, input.ToString(), cursorPosition);
                }
            }
            else if (key.Key == ConsoleKey.Delete)
            {
                if (cursorPosition < input.Length)
                {
                    input.Remove(cursorPosition, 1);
                    RedrawLine(prompt, input.ToString(), cursorPosition);
                }
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                if (cursorPosition > 0)
                {
                    cursorPosition--;
                    SetCursorPosition(prompt.Length + 1 + cursorPosition);
                }
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                if (cursorPosition < input.Length)
                {
                    cursorPosition++;
                    SetCursorPosition(prompt.Length + 1 + cursorPosition);
                }
            }
            else if (key.Key == ConsoleKey.Home)
            {
                cursorPosition = 0;
                SetCursorPosition(prompt.Length + 1);
            }
            else if (key.Key == ConsoleKey.End)
            {
                cursorPosition = input.Length;
                SetCursorPosition(prompt.Length + 1 + cursorPosition);
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                ClearLine(prompt.Length + 1 + input.Length);
                input.Clear();
                cursorPosition = 0;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                input.Insert(cursorPosition, key.KeyChar);
                cursorPosition++;
                RedrawLine(prompt, input.ToString(), cursorPosition);
            }
        }
    }

    private void ClearLine(int totalLength)
    {
        Console.Write("\r" + new string(' ', totalLength) + "\r");
    }

    private void RedrawLine(string prompt, string input, int cursorPosition)
    {
        var currentLeft = Console.CursorLeft;
        ClearLine(prompt.Length + 1 + input.Length + 10);
        Console.Write(prompt + " " + input);
        SetCursorPosition(prompt.Length + 1 + cursorPosition);
    }

    private void SetCursorPosition(int position)
    {
        try
        {
            Console.CursorLeft = position;
        }
        catch
        {
            // Ignore cursor positioning errors
        }
    }

    public void Dispose()
    {
        _slotManager?.Dispose();
    }
}