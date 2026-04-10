namespace Legacy89DiskKit.Cli.Logging;

public interface ILogMessageHandler
{
    void Handle(LogMessage message);
    void Flush();
}
