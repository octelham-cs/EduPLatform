namespace EduPlatform.Core.Entities
{
    // الشعبة (علمي علوم، علمي رياضة، أدبي)
    public class Branch
    {
        public int Id { get; set; }

        // اسم الشعبة بالعربية
        public string Name { get; set; } = string.Empty;

        // اسم الشعبة بالإنجليزية
        public string NameEn { get; set; } = string.Empty;

        // هل الشعبة نشطة؟
        public bool IsActive { get; set; } = true;
    }
}