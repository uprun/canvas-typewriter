FileSystemWatcher subscribe_to_file_changes()
{

    var path = Path.Combine(Directory.GetCurrentDirectory(), "..");
    var watcher = new FileSystemWatcher(path);
    Console.WriteLine($"Watching \"{path}\"");

    var history_path = Path.Combine(path, ".history");
    System.IO.Directory.CreateDirectory(history_path);

    watcher.NotifyFilter = NotifyFilters.Attributes
                            | NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.FileName
                            | NotifyFilters.LastAccess
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size;

    watcher.Changed += OnChanged;
    watcher.Created += OnCreated;
    watcher.Deleted += OnDeleted;
    watcher.Renamed += OnRenamed;
    watcher.Error += OnError;

    watcher.IncludeSubdirectories = true;
    watcher.EnableRaisingEvents = true;

    return watcher;
}

void OnError(object sender, ErrorEventArgs e)
{
    Console.WriteLine(e);
}

void OnChanged(object sender, FileSystemEventArgs e)
{
    if (e.ChangeType != WatcherChangeTypes.Changed)
    {
        return;
    }
    Console.WriteLine($"Changed: {e.FullPath}");
}

void OnCreated(object sender, FileSystemEventArgs e)
{
    string value = $"Created: {e.FullPath}";
    Console.WriteLine(value);
}

void OnDeleted(object sender, FileSystemEventArgs e) =>
    Console.WriteLine($"Deleted: {e.FullPath}");

void OnRenamed(object sender, RenamedEventArgs e)
{
    Console.WriteLine($"Renamed:");
    Console.WriteLine($"    Old: {e.OldFullPath}");
    Console.WriteLine($"    New: {e.FullPath}");
}

subscribe_to_file_changes();

while(true){} // yeah infinite loop