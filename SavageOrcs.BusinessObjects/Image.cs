﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.BusinessObjects
{
    public class Image
    {
        public Guid Id { get; set; }

        public byte[] Content { get; set; }

        public bool IsVisible { get; set; }

        public Guid MarkId { get; set; }

        public virtual Mark Mark { get; set; }
    }
}
