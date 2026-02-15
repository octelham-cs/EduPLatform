namespace EduPlatform.Core.Entities
{
    // الترم الدراسي
    public class AcademicTerm
    {
        public int Id { get; set; }

        // اسم الترم
        public string Name { get; set; } = string.Empty;

        // السنة الدراسية
        public int Year { get; set; }

        // تاريخ البداية
        public DateTime StartDate { get; set; }

        // تاريخ النهاية
        public DateTime EndDate { get; set; }

        // هل الترم نشط؟
        public bool IsActive { get; set; } = true;
    }
}