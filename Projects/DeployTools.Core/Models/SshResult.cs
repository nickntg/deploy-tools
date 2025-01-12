namespace DeployTools.Core.Models
{
    public class SshResult
    {
        private static readonly SshResult SuccessResult = new() { IsSuccessful = true };

        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public string Result { get; set; }
        public bool ListingFound { get; set; }

        public static SshResult Success()
        {
            return SuccessResult;
        }
    }
}
