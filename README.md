# nucs.Filesystem

Filesystem helping library aiming to provide common functions when working with the filesystem.
From monitoring to easier coding.

#### Capabilities
###### Filesystem Namespace
- `class UniquePcId` - generates a unique GUID for this PC
- `UniquePath` - will generate a unique directory per PC (determined by hardware via UniquePcId).
- `FileSearch` - static methods to search files with patterns.
- `FileInfoExtensions`
- `FileInfo HandlePossibleDuplicate(this FileInfo)`- checks if the given file already exist, if yes - will add `(n)` to the file name to avoid duplicate name - just like windows does.
- `Paths` - extension to `System.IO.Path`.
	- `string NormalizePath(string path)` - normalizes path for storage or comparison
	- `DirectoryInfo WindowsDirectory` - returns windows directory.
	- `FileInfo ExecutingExe` - returns the path to entry exe
	- `FileInfo ExecutingDirectory `- returns the path to entry exe's directory
	- `FileInfo RandomLocation` - generates via `FileNameGenerator` a random `FileInfo`
	-  `bool IsDirectoryWritable(this DirectoryInfo directory)` - tests directory for writing with current permissions.
	- `bool CompareTo(this FileSystemInfo fi, FileSystemInfo fi2)` - compares two FileSystemInfo the right way.
     
---
###### Distribution Namespace
- `FileNameGenerator.Generate(DirectoryInfo)` - Generate a random logical name for a file to the given directory (from old trojan project). 
- `Distributers` - They provide enumeration of specific directories that are common in all computers.
	- `GeneralLocationsDistributer`, `ProgramFilesDistributer`, `RootDriveDistributer`, `UniquePathDistributer`, `WindowsDistributer`.
     
---
###### Enumerators Namespace
- `class ActiveProcessFiles : IEnumerable<FileInfo>, IEnumerable<FileProcessInfo>` - returns either all Processes with their entry exe tupled or all of the filenames that are available.
- `class AutorunServiceFiles : IEnumerable<FileCall>` - returns all services entry exe that are set to auto run.
- `class RecentFiles : IEnumerable<FileInfo>` - returns all the recent files that were opened.

     
---
###### Monitoring.Windows Namespace - objects that monitor certain behavior and report changes (such as new drive inserted)
- `DriveMonitor : MonitorBase<DriveInfo>` -  Monitors (and reports new) drives that are connected with the ability to distinguish between the different drive types:
	- `DriveMonitor.FileDrive()`- returns all drivers that contain can contain files (e.g. not cdrom).
	- `DriveMonitor.FixedDrives()`- returns all hard drives that are connected to the PC (not via usb).
	- `DriveMonitor.RemovableDrives()`- returns all storage devices connected via USB (removable).
	- `DriveMonitor.AllDrives()`- just all of them.
 
- `class ExplorerMonitor : MonitorBase<ExplorerWindowRepresentor>` - Monitors explorer directory browsing (spying style) and reports when directory changes.
     
---
###### Monitoring Namespace
- `ProcessCrashMonitor` - monitor a process and report when it has crashed and when it has restarted (`ProcessRebound`).
- `WerFaultKiller` - when a program crashes, windows will start a process named WerFault and will 'try' to fix the crash and even suggest to report or restart. this prevents from the crashing program to close so this class will kill the WerFault automatically.

---
###### Zip Namespace
- `ZipResource.Export(DirectoryInfo to, string resourcename)` - extracts an embedded resource zip file to the given directory.

---
#### MIT License

Copyright (c) 2016 Eli Belash

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
