namespace DeployTools.Core.Models.Background
{
    public class CertificateJobInfo
    {
        public string CertificateId { get; set; }
        public CertificateAction CertificateAction { get; set;}
    }

    public enum CertificateAction
    {
        Create = 0,
        WaitForValidation = 1,
        Delete = 2
    }
}
