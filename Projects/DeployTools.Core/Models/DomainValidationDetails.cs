namespace DeployTools.Core.Models
{
    public class DomainValidationDetails
    {
        public string DomainName { get; set; }
        public string ResourceRecordName { get; set; }
        public string ResourceRecordType { get; set; }
        public string ResourceRecordValue { get; set; }
    }
}
