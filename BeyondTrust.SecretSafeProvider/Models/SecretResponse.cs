namespace BeyondTrust.SecretSafeProvider.Models;

public record SecretResponse(
    string? Id = null,
    string? Title = null,
    string? Description = null,
    string? Username = null,
    long OwnerId = default,
    string? FolderId = null,
    DateTime CreatedOn = default,
    string? CreatedBy = null,
    DateTime ModifiedOn = default,
    string? ModifiedBy = null,
    string? Owner = null,
    string? Folder = null,
    string? FolderPath = null,
    List<OwnerInfo>? Owners = null,
    string? OwnerType = null,
    string? Notes = null);
