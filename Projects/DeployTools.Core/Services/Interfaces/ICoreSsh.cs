using System;
using System.Threading.Tasks;
using DeployTools.Core.Models;

namespace DeployTools.Core.Services.Interfaces
{
    public interface ICoreSsh
    {
        event EventHandler<UploadEventArgs> UploadDirectoryProgressEvent;
        event EventHandler<JournalEventArgs> JournalEvent;
        Task<SshResult> ConnectAsync(string address, string userName, string keyFile);
        Task<SshResult> RunCommandAsync(string command);
        Task<SshResult> UploadFileAsync(string localFile, string remoteFile);
        Task<SshResult> UploadContentAsync(string content, string remoteFile);
        Task<SshResult> UploadDirectoryAsync(string localDirectory, string remoteDirectory);
        Task<SshResult> FileExistsAsync(string remoteFile);
        Task<SshResult> DirectoryExistsAsync(string remoteDirectory);
    }
}
