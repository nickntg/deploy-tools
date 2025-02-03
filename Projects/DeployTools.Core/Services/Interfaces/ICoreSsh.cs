using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.Models;

namespace DeployTools.Core.Services.Interfaces
{
    public interface ICoreSsh
    {
        event EventHandler<UploadEventArgs> UploadDirectoryProgressEvent;
        event EventHandler<JournalEventArgs> JournalEvent;
        Task<bool> IsConnectedAsync();
        Task DisconnectAsync();
        Task<SshResult> ConnectAsync(string address, string userName, string keyFile);
        Task<SshResult> RunCommandAsync(string command);
        Task<SshResult> UploadFileAsync(string localFile, string remoteFile);
        Task<SshResult> UploadContentAsync(string content, string remoteFile);
        Task<SshResult> UploadDirectoryAsync(string localDirectory, string remoteDirectory, string configFileEndsWith, Dictionary<string, string> fileContentsMap);
        Task<SshResult> FileExistsAsync(string remoteFile);
        Task<SshResult> DirectoryExistsAsync(string remoteDirectory);
    }
}
