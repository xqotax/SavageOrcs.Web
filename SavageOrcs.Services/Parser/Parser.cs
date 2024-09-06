using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using SavageOrcs.BusinessObjects;
using System.Globalization;
using System.Text;

namespace SavageOrcs.Services.Parser
{
    public class Parser
    {
        public string? CertificatesFolder  { get; set; }
        public string? CertificateSheetName { get; set; }
        public string? CertificateSheetEmail { get; set; }
        public string? CertificateSheetProjectName { get; set; }
        public string? CertificateDriveName { get; set; }
        public string? CertificateDriveEmail { get; set; }
        public string? CertificateDriveProjectName { get; set; }

        public string? SheetName {  get; set; } 
        public string? SheetId { get; set; }   
        
        public Curator[] Curators { get; set; } = Array.Empty<Curator>();
        public Area[] Areas { get; set; } = Array.Empty<Area>();

        public ParserResult Start()
        {
            var pathToSheetCertificate = Path.Combine(CertificatesFolder!, CertificateSheetName!);

            var service = (SheetsService)GoogleServiceFactory.CreateService(
                               pathToSheetCertificate,
                               CertificateSheetEmail!,
                               CertificateSheetProjectName!,
                               new[] { SheetsService.Scope.Spreadsheets },
                               GoogleServiceType.Sheet);

            var marks = GetData(service);


            var pathToDriveCertificate = Path.Combine(CertificatesFolder!, CertificateDriveName!);
            var driveService = (DriveService)GoogleServiceFactory.CreateService(
                                    pathToDriveCertificate,
                                    CertificateDriveEmail!,
                                    CertificateDriveProjectName!,
                                    new string[] { DriveService.Scope.Drive },
                                    GoogleServiceType.Drive);

            var i = 0;
            foreach (var mark in marks)
            {
                i++;
                if (string.IsNullOrEmpty(mark.FileUrl))
                    continue;

                var fileId = GetFileIdFromLink(mark.FileUrl);

                if (string.IsNullOrEmpty(fileId))
                    continue;

                mark.File = DownloadFile(driveService, fileId);

                if (i % 10 == 0)
                {
                    i = i + i - i;
                }

            }

            var dbMarks = new Dictionary<int, Mark>();
            var dbImages = new Dictionary<int, Image>();

            foreach (var mark in marks)
            {
                var dbMark = new Mark
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Name = mark.Name,
                    NameEng = mark.EngName,
                    Description = mark.Description,
                    DescriptionEng = mark.DescriptionEng,
                    ResourceName = mark.Resource,
                    ResourceNameEng = mark.ResourceEng,
                    ResourceUrl = mark.ResourceUlr,
                    MapId = 1,
                    IsVisible = true,
                };
                dbMarks[mark.Row] = dbMark;

                if (!string.IsNullOrEmpty(mark.Coordinate) && mark.Coordinate.Contains('@'))
                {
                    int startIndex = mark.Coordinate.IndexOf('@') + 1;
                    mark.Coordinate = mark.Coordinate[startIndex..];

                    int firstComma = mark.Coordinate.IndexOf(',');
                    int secondComma = mark.Coordinate.IndexOf(',', firstComma + 1);
                    if (secondComma != -1)
                    {
                        mark.Coordinate = mark.Coordinate[..secondComma];
                    }
                    var cords = mark.Coordinate.Split(',');
                    if (cords.Length == 2)
                    {
                        try
                        {
                            dbMark.Lat = Convert.ToDouble(cords[0], CultureInfo.InvariantCulture);
                            dbMark.Lng = Convert.ToDouble(cords[1], CultureInfo.InvariantCulture);
                        }
                        catch
                        {

                        }
                    }
                }

                if (!string.IsNullOrEmpty(mark.Curator))
                    dbMark.CuratorId = Curators.FirstOrDefault(x => x.Name != null && x.Name.Trim().ToLower() == mark.Curator.Trim().ToLower())?.Id;

                if (!string.IsNullOrEmpty(mark.City))
                {
                    var cityNames = mark.City.Split(',');
                    if (cityNames.Length == 3)
                    {
                        for (int k = 0; k < cityNames.Length; k++)
                        {
                            cityNames[k] = cityNames[k].Trim();
                        }

                        if (cityNames[2].EndsWith('.'))
                        {
                            cityNames[2] = cityNames[2].TrimEnd('.');
                        }

                        dbMark.AreaId = Areas.FirstOrDefault(x => x.Name.ToLower().Trim() == cityNames[0].ToLower().Trim()
                        && x.Community.ToLower().Trim() == cityNames[1].ToLower().Trim()
                        && x.Region.ToLower().Trim() == cityNames[2].ToLower().Trim())?.Id;
                    }
                }

                if (mark.File != null && mark.File.Bytes != null && mark.File.Bytes.Length != 0)
                {
                    var image = new Image
                    {
                        Id = Guid.NewGuid(),
                        Content = mark.File.Bytes,
                        IsVisible = true,
                        MarkId = dbMark.Id
                    };

                    dbImages[mark.Row] = image;
                }
            }

            var result = new ParserResult()
            {
                Images = dbImages,
                Marks = dbMarks
            };



            return result;
        }

        private static string GetFileIdFromLink(string link)
        {
            var startIndex = link.IndexOf("d/");
            if (startIndex == -1) return "";

            link = link[(startIndex + 2)..];

            var lastIndex = link.IndexOf('/');
            if (lastIndex == -1) return link;

            return link[..lastIndex];
        }

        private static GoogleDriveFileDto? DownloadFile(DriveService driveService, string fileId)
        {
            try
            {
                var fileRequest = driveService.Files.Get(fileId);
                fileRequest.Fields = "id,name,createdTime";

                var fileResponse = fileRequest.Execute();

                using var stream = new MemoryStream();
                var downloadRequest = driveService.Files.Get(fileId);
                downloadRequest.Download(stream);

                return new GoogleDriveFileDto
                {
                    Id = fileId,
                    CreatedDate = fileResponse.CreatedTimeDateTimeOffset?.Date!,
                    Name = fileResponse.Name,
                    //Stream = stream,
                    Bytes = ConvertMemoryStreamToBase64DataUrl(stream, Path.GetExtension(fileResponse.Name))
                };
            }
            catch
            {
                return null;
            }

        }

        private static byte[] ConvertMemoryStreamToBase64DataUrl(MemoryStream memoryStream, string fileType)
        {
            byte[] bytes = memoryStream.ToArray();

            string base64String = Convert.ToBase64String(bytes);

            // Формуємо рядок data URL
            string dataUrl = $"data:image/{fileType};base64,{base64String}";

            return GetBytes(dataUrl);
        }

        private static byte[] GetBytes(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        private List<GoogleMark> GetData(SheetsService service)
        {
            var docId = SheetId;
            var sheetName = SheetName;

            var sheetResponse = service.Spreadsheets.Get(docId).Execute();


            var marks = new List<GoogleMark>();
            foreach (var sheet in sheetResponse.Sheets.Reverse())
            {
                if (sheet.Properties.Title != sheetName) continue;

                var range = $"{sheetName}!A1:ZZZ";
                var response = service.Spreadsheets.Values.Get(docId, range).Execute();

                var headers = new Dictionary<int, string>();
                foreach (var header in response.Values[0])
                    if (header is not null)
                        headers.Add(response.Values[0].IndexOf(header), header.ToString()!);


                var statusCol = headers.FirstOrDefault(x => x.Value.Trim() == "Статус");
                if (statusCol.Value is null)
                    break;


                for (var i = 1; i < response.Values.Count; i++)
                {
                    var mark = new GoogleMark
                    {
                        Row = i,
                    };
                    for (var j = 0; j < response.Values[i].Count; j++)
                    {
                        if (headers.TryGetValue(j, out string? header))
                        {
                            switch (header.Trim())
                            {
                                case "Посилання на зображення":
                                    mark.FileUrl = response.Values[i][j].ToString()!;
                                    break;
                                case "Назва":
                                    mark.Name = response.Values[i][j].ToString()!;
                                    break;
                                case "Назва для іноземних користувачів":
                                    mark.EngName = response.Values[i][j].ToString()!;
                                    break;
                                case "Опис":
                                    mark.Description = response.Values[i][j].ToString()!;
                                    break;
                                case "Опис для іноземних користувачів":
                                    mark.DescriptionEng = response.Values[i][j].ToString()!;
                                    break;
                                case "Населений пункт":
                                    mark.City = response.Values[i][j].ToString()!;
                                    break;
                                case "Назва ресурсу":
                                    mark.Resource = response.Values[i][j].ToString()!;
                                    break;
                                case "Назва ресурсу для іноземних користувачів":
                                    mark.ResourceEng = response.Values[i][j].ToString()!;
                                    break;
                                case "Посилання на ресурс ":
                                    mark.ResourceUlr = response.Values[i][j].ToString()!;
                                    break;
                                case "Куратор":
                                    mark.Curator = response.Values[i][j].ToString()!;
                                    break;
                                case "Координати":
                                    mark.Coordinate = response.Values[i][j].ToString()!;
                                    break;
                                case "Готовий в обробку":
                                    mark.IsReadyToProcces = response.Values[i][j].ToString()!;
                                    break;
                                case "Статус":
                                    mark.Status = response.Values[i][j].ToString()!;
                                    break;

                            }
                        }
                    }

                    if (mark.Status == "Оброблений")
                        break;
                    //else
                    //{
                    //    string updateRange = $"{sheetName}!{GetColumnName(statusCol.Key + 1)}{i + 1}"; 
                    //    var valueRange = new ValueRange { Values = new List<IList<object>> { new List<object> { "Оброблений" } } };
                    //    var updateRequest = service.Spreadsheets.Values.Update(valueRange, docId, updateRange);
                    //    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    //    var updateResponse = updateRequest.Execute();
                    //}

                    marks.Add(mark);
                }
            }

            return marks;
        }

        private static string GetColumnName(int columnNumber)
        {
            const int baseValue = 'A' - 1;
            string columnName = "";

            while (columnNumber > 0)
            {
                int remainder = columnNumber % 26;
                if (remainder == 0)
                {
                    columnName = 'Z' + columnName;
                    columnNumber = (columnNumber / 26) - 1;
                }
                else
                {
                    columnName = (char)(baseValue + remainder) + columnName;
                    columnNumber = columnNumber / 26;
                }
            }

            return columnName;
        }

    }

    public class ParserResult
    {
        public Dictionary<int, Mark> Marks { get; set; }   = new Dictionary<int, Mark>();
        public Dictionary<int, Image> Images { get; set; } = new Dictionary<int, Image>();
    }
}
