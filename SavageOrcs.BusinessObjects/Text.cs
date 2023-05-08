﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.BusinessObjects
{
    public class Text
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Subject { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public bool EnglishVersion { get; set; }

        public Guid? UkrTextId { get; set; }

        public Guid? CuratorId { get; set; }

        public virtual Curator? Curator { get; set; }

        public virtual ICollection<Block> Blocks { get; set; } = new HashSet<Block>();

        public virtual ICollection<TextToCluster> TextsToClusters { get; set; } = new HashSet<TextToCluster>();

        public virtual ICollection<TextToMark> TextsToMarks { get; set; } = new HashSet<TextToMark>();
    }
}
