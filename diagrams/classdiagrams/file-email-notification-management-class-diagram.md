classDiagram
direction TB
    class Context {
	    +DbSet~FileManagement~ FileManagements
	    +DbSet~Notification~ Notifications
	    +bool BypassSoftDelete
	    +IMongoDatabase MongoDatabase
	    +IMongoCollection~ShuttleSchedule~ ShuttleScheduleCollection
	    +IMongoCollection~JourneyNote~ JourneyNoteCollection
    }

    class FileManagement {
	    +Guid Id
	    +string FileName
	    +string S3Key
	    +string FileType
	    +float FileSize
	    +DateTime UploadDate
	    +Guid? UploadBy
	    +Guid? RelatedObjectId
	    +RelatedObjectType? RelatedObjectType
	    +bool IsUploaded
	    +DateTime CreatedAt
	    +DateTime UpdatedAt
	    +bool IsDeleted
    }

    class Notification {
	    +Guid Id
	    +string Title
	    +string Content
	    +int Type
	    +Guid RecipientId
	    +Guid? SenderId
	    +DateTime CreatedAt
	    +bool IsRead
	    +string Navigation
	    +int Priority
    }

    class BaseUploadFileService {
	    #IUploadTransactionManager UploadTransactionManager
	    #Task~FileManagement~ AddFileManagement()
	    #Task DeleteFileManagement()
	    #Task~string~ GetS3Key()
	    #Task~FileManagement~ GetFileManagement()
	    +Task MarkFileAsUploadedAsync()
	    +Task~FileManagement~ GetFileData()
    }

    class S3FileUploadService {
	    -AmazonS3Client _s3Client
	    -string _bucketName
	    -IFileDeleter _fileDeleter
	    +Task~UploadFileResponse~ UploadFileAsync()
	    +Task~UploadFileResponse~ UploadStreamAsync()
	    +Task~bool~ DeleteFileManagementAsync()
	    +Task~PreSignedUrlResponse~ GeneratePreSignedUploadUrlAsync()
	    +Task~string~ GeneratePreSignedDownloadUrlAsync()
	    +Task~UploadFileResponse~ CopyObjectAsync()
	    +Task RollBackAsync()
	    +Task BeginTransactionAsync()
	    +Task ConvertHeicFileToPngInS3()
	    +Task~MemoryStream~ DownloadFileFromS3Async()
    }

    class IFileUploadService {
	    +Task~UploadFileResponse~ UploadFileAsync()
	    +Task~UploadFileResponse~ UploadStreamAsync()
	    +Task~bool~ DeleteFileManagementAsync()
	    +Task~PreSignedUrlResponse~ GeneratePreSignedUploadUrlAsync()
	    +Task~string~ GeneratePreSignedDownloadUrlAsync()
	    +Task~UploadFileResponse~ CopyObjectAsync()
	    +Task RollBackAsync()
	    +Task BeginTransactionAsync()
	    +Task ConvertHeicFileToPngInS3()
	    +Task~MemoryStream~ DownloadFileFromS3Async()
    }

    class IFileDeleter {
	    +Task~bool~ DeleteFileAsync()
    }

    class MailService {
	    -MailSettings _mailSettings
	    +Task~bool~ SendEmail()
    }

    class SendNotificationJob {
	    +Task ExecuteAsync()
    }

    class CleanFileJob {
	    +Task ExecuteAsync()
    }

    class INotificationSender {
	    +Task SendOneAsync()
	    +Task SendByNotificationAsync()
	    +Task SendByNotificationManyAsync()
	    +Task SendManyAsync()
	    +Task SendDataAsync()
	    +Task SendDataManyAsync()
	    +Task SendDataToTopicAsync()
    }

    class IUploadTransactionManager {
    }

	<<interface>> IFileUploadService
	<<interface>> IFileDeleter
	<<interface>> INotificationSender

    S3FileUploadService --|> BaseUploadFileService
    S3FileUploadService ..|> IFileUploadService
    Context "1" o-- "*" FileManagement : manages
    Context "1" o-- "*" Notification : manages
    S3FileUploadService --> IFileDeleter : uses
    S3FileUploadService --> Context : uses
    S3FileUploadService --> BaseUploadFileService : base
    S3FileUploadService --> IUploadTransactionManager : uses
    CleanFileJob --> Context : uses
    CleanFileJob --> IFileDeleter : uses
    SendNotificationJob --> INotificationSender : uses