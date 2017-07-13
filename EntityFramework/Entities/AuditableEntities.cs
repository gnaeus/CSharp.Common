using System;

namespace EntityFramework.Common.Entities
{
    /// <summary>
    /// Used to standardize soft deleting entities. Soft-delete entities are not actually deleted,
    /// marked as IsDeleted = true in the database, but can not be retrieved to the application.
    /// </summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
    
    /// <summary>
    /// An entity can implement this interface if <see cref="CreatedUtc"/> of this entity must be stored.
    /// <see cref="CreatedUtc"/> is automatically set when saving Entity to database.
    /// </summary>
    public interface ICreationTrackable
    {
        DateTime CreatedUtc { get; set; }
    }

    /// <summary>
    /// This interface is implemented by entities that is wanted
    /// to store creation information (who and when created).
    /// Creation time and creator user are automatically set when saving Entity to database.
    /// </summary>
    public interface ICreationAuditable<TUserId> : ICreationTrackable
        where TUserId : struct
    {
        TUserId CreatorUserId { get; set; }
    }

    /// <summary>
    /// This interface is implemented by entities that is wanted
    /// to store creation information (who and when created).
    /// Creation time and creator user are automatically set when saving Entity to database.
    /// </summary>
    public interface ICreationAuditable : ICreationTrackable
    {
        string CreatorUser { get; set; }
    }

    /// <summary>
    /// An entity can implement this interface if <see cref="UpdatedUtc"/> of this entity must be stored.
    /// <see cref="UpdatedUtc"/> is automatically set when updating Entity.
    /// </summary>
    public interface IModificationTrackable
    {
        DateTime? UpdatedUtc { get; set; }
    }
    
    /// <summary>
    /// This interface is implemented by entities that is wanted
    /// to store modification information (who and when modified lastly).
    /// Properties are automatically set when updating the Entity.
    /// </summary>
    public interface IModificationAuditable<TUserId> : IModificationTrackable
        where TUserId : struct
    {
        TUserId? UpdaterUserId { get; set; }
    }

    /// <summary>
    /// This interface is implemented by entities that is wanted
    /// to store modification information (who and when modified lastly).
    /// Properties are automatically set when updating the Entity.
    /// </summary>
    public interface IModificationAuditable : IModificationTrackable
    {
        string UpdaterUser { get; set; }
    }

    /// <summary>
    /// An entity can implement this interface if <see cref="DeletedUtc"/> of this entity must be stored.
    /// <see cref="DeletedUtc"/> is automatically set when deleting Entity.
    /// </summary>
    public interface IDeletionTrackable : ISoftDeletable
    {
        DateTime? DeletedUtc { get; set; }
    }

    /// <summary>
    /// This interface is implemented by entities which wanted
    /// to store deletion information (who and when deleted).
    /// </summary>
    public interface IDeletionAuditable<TUserId> : IDeletionTrackable
        where TUserId : struct
    {
        TUserId? DeleterUserId { get; set; }
    }

    /// <summary>
    /// This interface is implemented by entities which wanted
    /// to store deletion information (who and when deleted).
    /// </summary>
    public interface IDeletionAuditable : IDeletionTrackable
    {
        string DeleterUser { get; set; }
    }

    /// <summary>
    /// This interface is implemented by entities which modification times must be tracked.
    /// Related properties automatically set when saving/updating/deleting Entity objects.
    /// </summary>
    public interface IFullTrackable : ICreationTrackable, IModificationTrackable, IDeletionTrackable
    {
    }

    /// <summary>
    /// This interface is implemented by entities which must be audited.
    /// Related properties automatically set when saving/updating/deleting Entity objects.
    /// </summary>
    public interface IFullAuditable<TUserId> : IFullTrackable,
        ICreationAuditable<TUserId>, IModificationAuditable<TUserId>, IDeletionAuditable<TUserId>
        where TUserId : struct
    {
    }

    /// <summary>
    /// This interface is implemented by entities which must be audited.
    /// Related properties automatically set when saving/updating/deleting Entity objects.
    /// </summary>
    public interface IFullAuditable : IFullTrackable,
        ICreationAuditable, IModificationAuditable, IDeletionAuditable
    {
    }
}
