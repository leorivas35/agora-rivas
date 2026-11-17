using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class DocumentContent : BaseEntity
    {
        public byte[] Content { get; set; } = default!;
        public int DocumentId { get; set; }
        public AgoraFile Document { get; set; } = default!;
        
    }
}