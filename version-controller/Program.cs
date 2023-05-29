FileSystemWatcher subscribe_to_file_changes()
{

    var path = Path.Combine(Directory.GetCurrentDirectory(), "..");
    path = new DirectoryInfo(path).FullName;
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

    var ignore_directories = new [] {
        "/bin/",
        "/obj/",
        "/bundles/",
        "/.history/",
        "/.git/"
    };

    watcher.Changed += (object sender, FileSystemEventArgs e) =>
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            
            var full_path = new FileInfo(e.FullPath).FullName;
            var partial = full_path.Substring(path.Length);
            var to_be_excluded_by = ignore_directories.Where((to_ignore) => full_path.Contains(to_ignore)).ToList();
            if ( to_be_excluded_by.Count != 0)
            {
                Console.WriteLine($"IGNORE changes in { String.Join( "," , to_be_excluded_by)} folder(s): {e.FullPath}");
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
            if (partial.StartsWith(Path.DirectorySeparatorChar))
            {
                partial = partial.Substring(1);
            }
            var backup_path = Path.Combine(history_path, partial);
            Console.WriteLine(partial );
            Console.WriteLine($"will copy to \"{backup_path}\"");
            Directory.CreateDirectory(backup_path);
            var date_time = DateTime.Now;
            var output_name = $"{date_time.ToString("yyyy-MM-dd--HH:mm:sszzz")}{Path.GetExtension(full_path)}";
            var to_copy_to = Path.Combine(backup_path, output_name);
            File.Copy(full_path, to_copy_to);
        };
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

var watcher =subscribe_to_file_changes();

while(true){} // yeah infinite loop