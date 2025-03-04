namespace Api.Common.Utilities
{
    public static class Constants
    {
        #region SEED ACCOUNT ADMIN
        
        public const string AdminEmail = "admin@gmail.com";
        public const string AdminPassword = "123456";
        public const string AdminUsername = "admin";
        
        #endregion
        
        private const string StudentHashKey = $"StudentId_";
        public static string GetStudentStringToHash(Guid studentId) => StudentHashKey + studentId.ToString();
    }
}
