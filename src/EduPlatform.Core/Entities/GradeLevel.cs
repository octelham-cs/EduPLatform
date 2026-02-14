namespace EduPlatform.Core.Entities
{
    // المستوى الدراسي (أولى ثانوي، تانية ثانوي، تالتة ثانوي)
    public class GradeLevel
    {
        public int Id { get; set; }

        // اسم المستوى بالعربية
        public string Name { get; set; } = string.Empty;

        // اسم المستوى بالإنجليزية
        public string NameEn { get; set; } = string.Empty;

        // ترتيب العرض
        public int Order { get; set; }

        // هل المستوى نشط؟
        public bool IsActive { get; set; } = true;
    }
}