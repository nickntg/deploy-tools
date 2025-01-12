using System;
using System.Threading.Tasks;
using DeployTools.Core.Models;

namespace DeployTools.Core.Services.Interfaces
{
    public interface ICoreSsh
    {
        event EventHandler<UploadEventArgs> UploadDirectoryProgressEvent;
        event EventHandler<JournalEventArgs> JournalEvent;
        Task<SshResult> Connect(string address, string userName, string keyFile);
        Task<SshResult> RunCommand(string command);
        Task<SshResult> UploadFile(string localFile, string remoteFile);
        Task<SshResult> UploadContent(string content, string remoteFile);
        Task<SshResult> UploadDirectory(string localDirectory, string remoteDirectory);
        Task<SshResult> FileExists(string remoteFile);
        Task<SshResult> DirectoryExists(string remoteDirectory);
    }
}
