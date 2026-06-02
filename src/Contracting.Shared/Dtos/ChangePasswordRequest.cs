namespace Contracting.Shared.Dtos;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
