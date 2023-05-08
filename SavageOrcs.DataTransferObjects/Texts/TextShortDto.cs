﻿using SavageOrcs.DataTransferObjects._Constants;

namespace SavageOrcs.DataTransferObjects.Texts
{
    public class TextShortDto
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Subject { get; set; }

        public bool EnglisVersion { get; set; }

        public DateTime CreatedDate { get; set; }

        public GuidIdAndStringNameWithEnglishName? Curator { get; set; }
    }
}