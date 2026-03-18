#pragma once

#include <string>
#include <map>

namespace legacy89diskkit::cpp::presentation
{

enum class MessageKey
{
    RootDescription,
    LanguageOptionDescription,
    EncodingOptionDescription,
    ListCommandDescription,
    FileCommandDescription,
    FileExtractCommandDescription,
    FileInjectCommandDescription,
    FileDeleteCommandDescription,
    FileRenameCommandDescription,
    FileCopyCommandDescription,
    DiskCommandDescription,
    DiskCreateCommandDescription,
    DiskFormatCommandDescription,
    BootCommandDescription,
    BootShowCommandDescription,
    LayoutCommandDescription,
    LayoutShowCommandDescription,
    
    ImageArgumentDescription,
    DiskFileArgumentDescription,
    HostPathArgumentDescription,
    SourceNameArgumentDescription,
    TargetNameArgumentDescription,
    NewNameArgumentDescription,
    
    FileSystemLabel,
    PlatformLabel,
    FileCountLabel,
    TotalCapacityLabel,
    UsedSpaceLabel,
    FreeSpaceLabel,
    BootTypeLabel,
    BootFileLabel,
    
    FileNameHeader,
    TypeHeader,
    FlagsHeader,
    SizeHeader,
    LoadHeader,
    EndHeader,
    ExecHeader,
    ClusterHeader,
    NoteHeader,
    
    FileExtractedMessage,
    FileInjectedMessage,
    FileDeletedMessage,
    FileRenamedMessage,
    FileCopiedMessage,
    DiskCreatedMessage,
    DiskFormattedMessage,
    
    ErrorFileNotFound,
    ErrorFileSystemNotSupported,
    ErrorInvalidUsage,
};

class CliLocalizer
{
public:
    static CliLocalizer& GetJa()
    {
        static CliLocalizer ja("ja");
        return ja;
    }

    static CliLocalizer& GetEn()
    {
        static CliLocalizer en("en");
        return en;
    }

    const std::string& Get(MessageKey key) const
    {
        auto it = messages_.find(key);
        if (it != messages_.end()) return it->second;
        static std::string empty = "";
        return empty;
    }

private:
    CliLocalizer(const std::string& lang)
    {
        if (lang == "ja")
        {
            messages_[MessageKey::RootDescription] = "Legacy89DiskKit CLI";
            messages_[MessageKey::LanguageOptionDescription] = "UI 表示言語を指定します: ja または en";
            messages_[MessageKey::EncodingOptionDescription] = "ディスク上ファイル名の表示デコードやテキスト入出力の文字エンコーディングを上書きします";
            messages_[MessageKey::ListCommandDescription] = "ファイル一覧とディスク概要を表示します";
            messages_[MessageKey::FileCommandDescription] = "既存ディスクイメージ上のファイル操作";
            messages_[MessageKey::FileExtractCommandDescription] = "ディスク上のファイルをホストへ書き出します";
            messages_[MessageKey::FileInjectCommandDescription] = "ホストファイルをディスクへ注入します";
            messages_[MessageKey::FileDeleteCommandDescription] = "ディスク上のファイルを削除します";
            messages_[MessageKey::FileRenameCommandDescription] = "ディスク上のファイル名を変更します";
            messages_[MessageKey::FileCopyCommandDescription] = "同一ディスク内でファイルを複製します";
            messages_[MessageKey::DiskCommandDescription] = "ディスク単位の操作";
            messages_[MessageKey::DiskCreateCommandDescription] = "新しいディスクイメージを作成し、指定したファイルシステムで初期化します";
            messages_[MessageKey::DiskFormatCommandDescription] = "既存ディスクイメージを再初期化します";
            messages_[MessageKey::BootCommandDescription] = "ブート情報の操作";
            messages_[MessageKey::BootShowCommandDescription] = "このディスクのブート情報を表示します";
            messages_[MessageKey::LayoutCommandDescription] = "ディレクトリレイアウトの操作";
            messages_[MessageKey::LayoutShowCommandDescription] = "現在のディレクトリエントリ順を表示します";
            
            messages_[MessageKey::ImageArgumentDescription] = "ディスクイメージのパス";
            messages_[MessageKey::DiskFileArgumentDescription] = "ディスク上のファイル名";
            messages_[MessageKey::HostPathArgumentDescription] = "ホスト上のパス";
            messages_[MessageKey::SourceNameArgumentDescription] = "コピー元ファイル名";
            messages_[MessageKey::TargetNameArgumentDescription] = "コピー先ファイル名";
            messages_[MessageKey::NewNameArgumentDescription] = "新しいファイル名";
            
            messages_[MessageKey::FileSystemLabel] = "ファイルシステム";
            messages_[MessageKey::PlatformLabel] = "プラットフォーム";
            messages_[MessageKey::FileCountLabel] = "ファイル数";
            messages_[MessageKey::TotalCapacityLabel] = "総容量";
            messages_[MessageKey::UsedSpaceLabel] = "使用量";
            messages_[MessageKey::FreeSpaceLabel] = "空き容量";
            messages_[MessageKey::BootTypeLabel] = "ブート";
            messages_[MessageKey::BootFileLabel] = "ブートファイル";
            
            messages_[MessageKey::FileNameHeader] = "名前";
            messages_[MessageKey::TypeHeader] = "種別";
            messages_[MessageKey::FlagsHeader] = "フラグ";
            messages_[MessageKey::SizeHeader] = "サイズ";
            messages_[MessageKey::LoadHeader] = "Load";
            messages_[MessageKey::EndHeader] = "End";
            messages_[MessageKey::ExecHeader] = "Exec";
            messages_[MessageKey::ClusterHeader] = "クラスタ";
            messages_[MessageKey::NoteHeader] = "注記";
            
            messages_[MessageKey::FileExtractedMessage] = "ファイルを書き出しました。";
            messages_[MessageKey::FileInjectedMessage] = "ファイルを注入しました。";
            messages_[MessageKey::FileDeletedMessage] = "ファイルを削除しました。";
            messages_[MessageKey::FileRenamedMessage] = "ファイル名を変更しました。";
            messages_[MessageKey::FileCopiedMessage] = "ファイルを複製しました。";
            messages_[MessageKey::DiskCreatedMessage] = "ディスクを作成してフォーマットしました。";
            messages_[MessageKey::DiskFormattedMessage] = "ディスクをフォーマットしました。";
            
            messages_[MessageKey::ErrorFileNotFound] = "ファイルが見つかりません: ";
            messages_[MessageKey::ErrorFileSystemNotSupported] = "このファイルシステムではこの操作をサポートしていません。";
            messages_[MessageKey::ErrorInvalidUsage] = "使い方が正しくありません。";
        }
        else
        {
            messages_[MessageKey::RootDescription] = "Legacy89DiskKit CLI";
            messages_[MessageKey::LanguageOptionDescription] = "Specify UI language: ja or en";
            messages_[MessageKey::EncodingOptionDescription] = "Override character encoding for file names and text I/O";
            messages_[MessageKey::ListCommandDescription] = "List files and disk summary information";
            messages_[MessageKey::FileCommandDescription] = "File operations on an existing disk image";
            messages_[MessageKey::FileExtractCommandDescription] = "Extract one disk file to a host path";
            messages_[MessageKey::FileInjectCommandDescription] = "Inject a host file into a disk image";
            messages_[MessageKey::FileDeleteCommandDescription] = "Delete one disk file";
            messages_[MessageKey::FileRenameCommandDescription] = "Rename one disk file";
            messages_[MessageKey::FileCopyCommandDescription] = "Duplicate a file inside the same disk image";
            messages_[MessageKey::DiskCommandDescription] = "Disk-level operations";
            messages_[MessageKey::DiskCreateCommandDescription] = "Create a new disk image and initialize it with an explicit file system";
            messages_[MessageKey::DiskFormatCommandDescription] = "Reinitialize an existing disk image";
            messages_[MessageKey::BootCommandDescription] = "Boot information operations";
            messages_[MessageKey::BootShowCommandDescription] = "Show boot information for this disk";
            messages_[MessageKey::LayoutCommandDescription] = "Directory layout operations";
            messages_[MessageKey::LayoutShowCommandDescription] = "Show the current directory entry order";
            
            messages_[MessageKey::ImageArgumentDescription] = "Path to the disk image";
            messages_[MessageKey::DiskFileArgumentDescription] = "File name on the disk";
            messages_[MessageKey::HostPathArgumentDescription] = "Path on the host";
            messages_[MessageKey::SourceNameArgumentDescription] = "Source file name";
            messages_[MessageKey::TargetNameArgumentDescription] = "Target file name";
            messages_[MessageKey::NewNameArgumentDescription] = "New file name";
            
            messages_[MessageKey::FileSystemLabel] = "File System";
            messages_[MessageKey::PlatformLabel] = "Platform";
            messages_[MessageKey::FileCountLabel] = "File Count";
            messages_[MessageKey::TotalCapacityLabel] = "Total Capacity";
            messages_[MessageKey::UsedSpaceLabel] = "Used Space";
            messages_[MessageKey::FreeSpaceLabel] = "Free Space";
            messages_[MessageKey::BootTypeLabel] = "Boot";
            messages_[MessageKey::BootFileLabel] = "Boot File";
            
            messages_[MessageKey::FileNameHeader] = "Name";
            messages_[MessageKey::TypeHeader] = "Type";
            messages_[MessageKey::FlagsHeader] = "Flags";
            messages_[MessageKey::SizeHeader] = "Size";
            messages_[MessageKey::LoadHeader] = "Load";
            messages_[MessageKey::EndHeader] = "End";
            messages_[MessageKey::ExecHeader] = "Exec";
            messages_[MessageKey::ClusterHeader] = "Cluster";
            messages_[MessageKey::NoteHeader] = "Note";
            
            messages_[MessageKey::FileExtractedMessage] = "File extracted successfully.";
            messages_[MessageKey::FileInjectedMessage] = "File injected successfully.";
            messages_[MessageKey::FileDeletedMessage] = "File deleted successfully.";
            messages_[MessageKey::FileRenamedMessage] = "File renamed successfully.";
            messages_[MessageKey::FileCopiedMessage] = "File copied successfully.";
            messages_[MessageKey::DiskCreatedMessage] = "Disk created and formatted successfully.";
            messages_[MessageKey::DiskFormattedMessage] = "Disk formatted successfully.";
            
            messages_[MessageKey::ErrorFileNotFound] = "File not found: ";
            messages_[MessageKey::ErrorFileSystemNotSupported] = "This operation is not supported by this file system.";
            messages_[MessageKey::ErrorInvalidUsage] = "Invalid usage.";
        }
    }

    std::map<MessageKey, std::string> messages_;
};

} // namespace legacy89diskkit::cpp::presentation
