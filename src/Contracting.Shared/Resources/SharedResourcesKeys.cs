namespace Contracting.Shared.Resources
{
    public static class SharedResourcesKeys
    {
        public const string InvalidCredentials = "InvalidCredentials";
        public const string Required = "Required";
        public const string CreateSuccess = "CreateSuccess";
        public const string EmailSendSuccess = "EmailSendSuccess";
        public const string DublicateEmail = "DublicateEmail";
        public const string Logout = "Logout";
        public const string UserNameLength = "UserNameLength";
        public const string PasswordLength = "PasswordLength";
        public const string FullNameLength = "FullNameLength";
        public const string EmailInvalid = "EmailInvalid";
        public const string EmailLength = "EmailLength";
        public const string PhoneNumberLength = "PhoneNumberLength";
        public const string PasswordRange = "PasswordRange";
        public const string RefreshTokenLength = "RefreshTokenLength";
        public const string RefreshTokenExpire = "RefreshTokenExpire";
        public const string ResonseLength = "ResonseLength";
        public const string GlobalException = "GlobalException";

        // Delete operations
        public const string DeleteSuccess = "DeleteSuccess";
        public const string DeleteFailed = "DeleteFailed";
        public const string EngineerDeleteSuccess = "EngineerDeleteSuccess";
        public const string StatusDeleteSuccess = "StatusDeleteSuccess";
        public const string ProjectDeleteSuccess = "ProjectDeleteSuccess";
        public const string PriorityDeleteSuccess = "PriorityDeleteSuccess";
        public const string DepartmentRemoveSuccess = "DepartmentRemoveSuccess";
        public const string BranchDeleteSuccess = "BranchDeleteSuccess";
        public const string RoleDeleteSuccess = "RoleDeleteSuccess";
        public const string RequestDeleteSuccess = "RequestDeleteSuccess";

        // Update operations
        public const string UpdateSuccess = "UpdateSuccess";
        public const string RoleUpdateSuccess = "RoleUpdateSuccess";
        public const string RoleCreateSuccess = "RoleCreateSuccess";

        // Not found messages
        public const string NotFound = "NotFound";
        public const string EngineerNotFound = "EngineerNotFound";
        public const string StatusNotFound = "StatusNotFound";
        public const string ProjectNotFound = "ProjectNotFound";
        public const string PriorityNotFound = "PriorityNotFound";
        public const string DepartmentNotFound = "DepartmentNotFound";
        public const string BranchNotFound = "BranchNotFound";
        public const string RoleNotFound = "RoleNotFound";
        public const string RequestNotFound = "RequestNotFound";

        // Business logic errors
        public const string RequestAlreadyActioned = "RequestAlreadyActioned";
        public const string BranchHasDepartments = "BranchHasDepartments";
        public const string Unauthorized = "Unauthorized";
        public const string RequestNoDepartment = "RequestNoDepartment";
        public const string CannotReassign = "CannotReassign";
        public const string TimeDurationMismatch = "TimeDurationMismatch";
        public const string ActionTakenSuccess = "ActionTakenSuccess";

    }
}
