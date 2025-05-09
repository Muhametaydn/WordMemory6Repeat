using System;
using System.Collections.Generic;
// Aşağıdaki using’e dikkat!
using WordMemoryApp.Models;

namespace WordMemoryApp.Models
{
    public class WordSample
    {
        public int WordSampleID { get; set; }
        public int WordID { get; set; }

        // Örnek cümle
        public string Samples { get; set; } = null!;

        // Navigation property
        public Word? Word { get; set; }
    }
}
