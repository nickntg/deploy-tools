using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeployTools.Core.Models;
using DeployTools.Core.Services.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DeployTools.Core.Services
{
    public class CoreSsh : ICoreSsh, IDisposable
    {
        public event EventHandler<UploadEventArgs> UploadDirectoryProgressEvent;
        public event EventHandler<JournalEventArgs> JournalEvent;

        private SshClient _sshClient;
        private string _remoteAddress;
        private string _userName;
        private string _keyFile;

        public Task<bool> IsConnectedAsync()
        {
            return Task.FromResult(_sshClient is not null && _sshClient.IsConnected);
        }

        public async Task DisconnectAsync()
        {
            if (await IsConnectedAsync())
            {
                _sshClient.Disconnect();
            }
        }

        public async Task<SshResult> ConnectAsync(string address, string userName, string keyFile)
        {
            var journal = new JournalEventArgs
            {
                CommandExecuted = "ConnectAsync"
            };

            try
            {
                _keyFile = keyFile;
                _remoteAddress = address;
                _userName = userName;

                _sshClient = new SshClient(address, userName, new PrivateKeyFile(keyFile));
                await _sshClient.ConnectAsync(CancellationToken.None);

                journal.WasSuccessful = true;

                return SshResult.Success();
            }
            catch (FileNotFoundException)
            {
                _sshClient?.Dispose();
                _sshClient = null;

                journal.WasSuccessful = false;

                return CreateError("Could not load key file");
            }
            catch (SocketException ex)
            {
                _sshClient?.Dispose();
                _sshClient = null;

                journal.WasSuccessful = false;

                return CreateError($"Socket error [{ex.Message}]");
            }
            catch (SshAuthenticationException ex)
            {
                _sshClient?.Dispose();
                _sshClient = null;

                journal.WasSuccessful = false;

                return CreateError($"Socket error [{ex.Message}]");
            }
            finally
            {
                OnRaiseJournalEvent(journal);
            }
        }

        public Task<SshResult> RunCommandAsync(string command)
        {
            if (_sshClient is null || !_sshClient.IsConnected)
            {
                return Task.FromResult(CreateError("Not connected"));
            }

            var journal = new JournalEventArgs
            {
                CommandExecuted = command
            };

            try
            {
                using (var cmd = _sshClient.RunCommand(command))
                {
                    journal.WasSuccessful = true;
                    journal.Output = cmd.Result;

                    return Task.FromResult(CreateSuccess(cmd.Result));
                }
            }
            catch (Exception ex)
            {
                journal.WasSuccessful = false;

                return Task.FromResult(CreateError($"Failed to run command [{ex.Message}]"));
            }
            finally
            {
                OnRaiseJournalEvent(journal);
            }
        }

        public async Task<SshResult> UploadFileAsync(string localFile, string remoteFile)
        {
            if (_sshClient is null || !_sshClient.IsConnected)
            {
                return CreateError("Not connected");
            }

            if (!File.Exists(localFile))
            {
                return CreateError("File does not exist");
            }

            var journal = new JournalEventArgs
            {
                CommandExecuted = $"Copy {localFile} to {remoteFile}"
            };

            try
            {
                journal.WasSuccessful = true;

                using (var stream = File.OpenRead(localFile))
                {
                    return await UploadStream(stream, remoteFile);
                }
            }
            catch (Exception ex)
            {
                journal.WasSuccessful = false;

                return CreateError($"Failed to copy [{ex.Message}]");
            }
            finally
            {
                OnRaiseJournalEvent(journal);
            }
        }

        public async Task<SshResult> UploadContentAsync(string content, string remoteFile)
        {
            if (_sshClient is null || !_sshClient.IsConnected)
            {
                return CreateError("Not connected");
            }

            var journal = new JournalEventArgs
            {
                CommandExecuted = $"Copy content to {remoteFile}"
            };

            try
            {
                journal.WasSuccessful = true;

                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    return await UploadStream(ms, remoteFile);
                }
            }
            catch (Exception ex)
            {
                journal.WasSuccessful = false;

                return CreateError($"Failed to copy [{ex.Message}]");
            }
            finally
            {
                OnRaiseJournalEvent(journal);
            }
        }

        public async Task<SshResult> UploadDirectoryAsync(string localDirectory, 
            string remoteDirectory,
            string configFileEndsWith,
            Dictionary<string, string> fileContentsReplaceMap)
        {
            if (_sshClient is null || !_sshClient.IsConnected)
            {
                return CreateError("Not connected");
            }

            if (!Directory.Exists(localDirectory))
            {
                return CreateError("File does not exist");
            }

            try
            {
                var progress = new UploadEventArgs();

                var allFiles = Directory.GetFiles(localDirectory, "*", SearchOption.AllDirectories);
                progress.FilesTotal = allFiles.Length;

                foreach (var file in allFiles)
                {
                    var fi = new FileInfo(file);
                    progress.BytesTotal += fi.Length;
                }

                OnRaiseUploadDirectoryProgressEvent(progress);

                using (var sftpClient = new SftpClient(_remoteAddress, _userName, new PrivateKeyFile(_keyFile)))
                {
                    await sftpClient.ConnectAsync(CancellationToken.None);

                    var directories = Directory.GetDirectories(localDirectory, "*", SearchOption.AllDirectories).ToList();
                    directories.Insert(0, localDirectory);
                    foreach (var directory in directories)
                    {
                        var localDiff = directory.Replace(localDirectory, string.Empty).Replace("\\", "/");
                        var remotePath = $"{remoteDirectory}{localDiff}";

                        try
                        {
                            sftpClient.ListDirectory(remotePath);
                        }
                        catch (SftpPathNotFoundException)
                        {
                            var journal = new JournalEventArgs
                            {
                                CommandExecuted = $"Create directory {remotePath}"
                            };

                            try
                            {
                                journal.WasSuccessful = true;

                                await sftpClient.CreateDirectoryAsync(remotePath, CancellationToken.None);
                            }
                            catch (Exception)
                            {
                                journal.WasSuccessful = false;

                                throw;
                            }
                            finally
                            {
                                OnRaiseJournalEvent(journal);
                            }
                        }

                        var filesToTransfer = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);
                        foreach (var file in filesToTransfer)
                        {
                            var fi = new FileInfo(file);
                            var remoteFile = $"{remotePath}/{fi.Name}";

                            var journal = new JournalEventArgs
                            {
                                CommandExecuted = $"Copy {file} to {remoteFile}"
                            };

                            try
                            {
                                if (!string.IsNullOrEmpty(configFileEndsWith) &&
                                    file.EndsWith(configFileEndsWith))
                                {
                                    var fileContents = await File.ReadAllTextAsync(file, Encoding.UTF8);
                                    foreach (var key in fileContentsReplaceMap.Keys)
                                    {
                                        fileContents = fileContents.Replace(key, fileContentsReplaceMap[key]);
                                    }

                                    journal.CommandExecuted += "\nUpdated file contents\n" + fileContents;

                                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContents)))
                                    {
                                        sftpClient.UploadFile(ms, remoteFile);
                                    }
                                }
                                else
                                {
                                    await using (var fs = File.OpenRead(file))
                                    {
                                        sftpClient.UploadFile(fs, remoteFile);
                                    }
                                }

                                journal.WasSuccessful = true;

                                progress.BytesCompleted += fi.Length;
                                progress.FilesCompleted++;

                                OnRaiseUploadDirectoryProgressEvent(progress);
                            }
                            catch (Exception)
                            {
                                journal.WasSuccessful = false;

                                throw;
                            }
                            finally
                            {
                                OnRaiseJournalEvent(journal);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return CreateError($"Error uploading file [{ex.Message}]");
            }

            return SshResult.Success();
        }

        public async Task<SshResult> FileExistsAsync(string remoteFile)
        {
            if (_sshClient is null || !_sshClient.IsConnected)
            {
                return CreateError("Not connected");
            }

            var journal = new JournalEventArgs
            {
                CommandExecuted = $"Check file exists {remoteFile}"
            };

            try
            {
                using (var sftpClient = new SftpClient(_remoteAddress, _userName, new PrivateKeyFile(_keyFile)))
                {
                    await sftpClient.ConnectAsync(CancellationToken.None);

                    var fi = new FileInfo(remoteFile);
                    var directory = remoteFile.Replace(fi.Name, string.Empty).TrimEnd('/');

                    journal.WasSuccessful = true;

                    var contents = sftpClient.ListDirectory(directory).ToList();

                    var found = contents.FirstOrDefault(x => x.FullName.Equals(remoteFile));

                    if (found is not null)
                    {
                        return CreateSuccess(string.Empty, true);
                    }

                    return CreateSuccess(string.Empty);
                }
            }
            catch (SftpPathNotFoundException)
            {
                return CreateSuccess(string.Empty);
            }
            catch (Exception ex)
            {
                journal.WasSuccessful = false;

                return CreateError($"Error listing [{ex.Message}]");
            }
            finally
            {
                OnRaiseJournalEvent(journal);
            }
        }

        public async Task<SshResult> DirectoryExistsAsync(string remoteDirectory)
        {
            if (_sshClient is null || !_sshClient.IsConnected)
            {
                return CreateError("Not connected");
            }

            var journal = new JournalEventArgs
            {
                CommandExecuted = $"Check directory exists {remoteDirectory}"
            };

            try
            {
                using (var sftpClient = new SftpClient(_remoteAddress, _userName, new PrivateKeyFile(_keyFile)))
                {
                    await sftpClient.ConnectAsync(CancellationToken.None);

                    journal.WasSuccessful = true;

                    sftpClient.ListDirectory(remoteDirectory);

                    return CreateSuccess(string.Empty, true);
                }
            }
            catch (SftpPathNotFoundException)
            {
                return CreateSuccess(string.Empty);
            }
            catch (Exception ex)
            {
                journal.WasSuccessful = false;

                return CreateError($"Error listing [{ex.Message}]");
            }
            finally
            {
                OnRaiseJournalEvent(journal);
            }
        }

        protected virtual void OnRaiseUploadDirectoryProgressEvent(UploadEventArgs e)
        {
            var raiseEvent = UploadDirectoryProgressEvent;

            raiseEvent?.Invoke(this, e);
        }

        protected virtual void OnRaiseJournalEvent(JournalEventArgs e)
        {
            var raiseEvent = JournalEvent;

            e.CommandCompleted = DateTimeOffset.UtcNow;

            raiseEvent?.Invoke(this, e);
        }

        private async Task<SshResult> UploadStream(Stream stream, string remoteFile)
        {
            try
            {
                using (var sftpClient = new SftpClient(_remoteAddress, _userName, new PrivateKeyFile(_keyFile)))
                {
                    await sftpClient.ConnectAsync(CancellationToken.None);

                    sftpClient.UploadFile(stream, remoteFile);
                }
            }
            catch (Exception ex)
            {
                return CreateError($"Error uploading file [{ex.Message}]");
            }

            return SshResult.Success();
        }

        public void Dispose()
        {
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        private SshResult CreateError(string errorMessage, bool listingFound = false)
        {
            return new SshResult
            {
                ErrorMessage = errorMessage,
                IsSuccessful = false,
                ListingFound = listingFound
            };
        }

        private SshResult CreateSuccess(string response, bool listingFound = false)
        {
            return new SshResult
            {
                Result = response,
                IsSuccessful = true,
                ListingFound = listingFound
            };
        }
    }

    public class UploadEventArgs : EventArgs
    {
        public int FilesCompleted { get; set; }
        public int FilesTotal { get; set; }
        public long BytesCompleted { get; set; }
        public long BytesTotal { get; set; }
    }

    public class JournalEventArgs : EventArgs
    {
        public DateTimeOffset CommandStarted { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset CommandCompleted { get; set; }
        public string CommandExecuted { get; set; }
        public bool WasSuccessful { get; set; }
        public string Output { get; set; }
    }
}
