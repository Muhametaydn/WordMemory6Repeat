using System;
using System.Collections.Generic;

namespace WordMemoryApp.Models
{
    public class AnalysisReportViewModel
    {
        public List<QuestionTypeStat> QuestionTypeStats { get; set; } = new();
        public List<string> FullyLearnedWords { get; set; } = new();
    }

    public class QuestionTypeStat
    {
        public QuestionType QuestionType { get; set; }
        public int TotalAsked { get; set; }   // Sorulan toplam soru
        public int CorrectCount { get; set; }   // Doğru sayısı
        public double SuccessRate => TotalAsked == 0
            ? 0
            : Math.Round((double)CorrectCount / TotalAsked * 100, 1);
    }
}
