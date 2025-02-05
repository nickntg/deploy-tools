using System;
using System.Threading.Tasks;
using DeployTools.Core.Services;
using NLog;

namespace DeployTools.Core.Helpers
{
    public class AwsCommandExecutor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task<T> ExecuteCommandAsync<T>(
            string message,
            Func<Task<T>> operation,
            Action<JournalEventArgs> logFunction)
        {
            Log.Info(message);

            var journalEvent = new JournalEventArgs
            {
                CommandExecuted = message,
                WasSuccessful = true
            };

            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                journalEvent.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(journalEvent);
            }
        }
    }
}
