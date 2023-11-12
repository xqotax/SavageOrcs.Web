// See https://aka.ms/new-console-template for more information
namespace SavageOrcs.Services.Parser
{
    public record GoogleMark
    {
        public string FileUrl { get; set; }
        public string Name { get; set; }
        public string EngName { get; set; }
        public string Description { get; set; }
        public string DescriptionEng { get; set; }
        public string City { get; set; }
        public string Resource { get; set; }
        public string ResourceEng { get; set; }
        public string ResourceUlr { get; set; }
        public string Curator { get; set; }
        public string Coordinate { get; set; }
        public string Status { get; set; }
        public string IsReadyToProcces { get; set; }
        public int Row { get; set; }
        public GoogleDriveFileDto? File { get; set; }
    }

    public class GoogleDriveFileDto
    {
        public string Name { get; internal set; }
        public DateTime? CreatedDate { get; internal set; }
        public string Id { get; internal set; }
        public MemoryStream Stream { get; internal set; }
        public byte[] Bytes { get; set; }
    }
}

