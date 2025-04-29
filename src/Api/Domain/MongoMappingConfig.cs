using Api.Common.Enums;
using Api.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Api.Domain;

public static class MongoMappingConfig
{
    public static void RegisterMappings()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(PickupSchedule)))
        {
            BsonClassMap.RegisterClassMap<PickupSchedule>(cm =>
            {
                cm.AutoMap();

                cm.MapIdMember(x => x.Id)
                    .SetSerializer(new GuidSerializer(BsonType.String))
                    .SetElementName("id");

                cm.MapMember(x => x.SchoolId)
                    .SetSerializer(new GuidSerializer(BsonType.String))
                    .SetElementName("schoolId");

                cm.MapMember(x => x.SchoolName)
                    .SetElementName("schoolName");

                cm.MapMember(x => x.SessionType)
                    .SetSerializer(new EnumSerializer<SessionType>(BsonType.String))
                    .SetElementName("sessionType");

                cm.MapMember(x => x.ClassId)
                    .SetSerializer(new GuidSerializer(BsonType.String))
                    .SetElementName("classId");

                cm.MapMember(x => x.ClassName)
                    .SetElementName("className");

                cm.MapMember(x => x.Date)
                    .SetElementName("date")
                    .SetSerializer(new DateOnlySerializer());

                cm.MapMember(x => x.DriverId)
                    .SetSerializer(new GuidSerializer(BsonType.String))
                    .SetElementName("driverId");

                cm.MapMember(x => x.DriverName)
                    .SetElementName("driverName");

                cm.MapMember(x => x.DriverAvatar)
                    .SetElementName("driverAvatar");

                cm.MapMember(x => x.VehicleType)
                    .SetElementName("vehicleType");

                cm.MapMember(x => x.DriverGender)
                    .SetSerializer(new EnumSerializer<Gender>(BsonType.String))
                    .SetElementName("driverGender");

                cm.MapMember(x => x.LicenseNumber)
                    .SetElementName("licenseNumber");

                cm.MapMember(x => x.IsAllNotesRead)
                    .SetElementName("isAllNotesRead");

                cm.MapMember(x => x.JourneyStatus)
                    .SetSerializer(new EnumSerializer<JourneyStatus>(BsonType.String))
                    .SetElementName("journeyStatus");

                cm.MapMember(x => x.NumberOfStudents)
                    .SetElementName("numberOfStudents");

                cm.MapMember(x => x.NumberOfCurrentStudents)
                    .SetElementName("numberOfCurrentStudents");

                cm.MapMember(x => x.BestRoute)
                    .SetElementName("bestRoute")
                    .SetSerializer(
                        new DictionaryInterfaceImplementerSerializer<Dictionary<string, string>>(
                            DictionaryRepresentation.Document));

                cm.MapMember(x => x.Students)
                    .SetElementName("students");
            });

            if (!BsonClassMap.IsClassMapRegistered(typeof(StudentOnBus)))
            {
                BsonClassMap.RegisterClassMap<StudentOnBus>(cm =>
                {
                    cm.AutoMap();

                    cm.MapMember(x => x.StudentId)
                        .SetElementName("studentId")
                        .SetSerializer(new GuidSerializer(BsonType.String));

                    cm.MapMember(x => x.ParentId)
                        .SetElementName("parentId")
                        .SetSerializer(new GuidSerializer(BsonType.String));

                    cm.MapMember(x => x.PickupAddress)
                        .SetElementName("pickupAddress");

                    cm.MapMember(x => x.PickupLat)
                        .SetElementName("pickupLat");

                    cm.MapMember(x => x.PickupLng)
                        .SetElementName("pickupLng");

                    cm.MapMember(x => x.Gender)
                        .SetElementName("gender")
                        .SetSerializer(new EnumSerializer<Gender>(BsonType.String));

                    cm.MapMember(x => x.AvatarUrl)
                        .SetElementName("avatarUrl");

                    cm.MapMember(x => x.ClassName)
                        .SetElementName("className");

                    cm.MapMember(x => x.ClassId)
                        .SetElementName("classId")
                        .SetSerializer(new GuidSerializer(BsonType.String));

                    cm.MapMember(x => x.FullName)
                        .SetElementName("fullName");

                    cm.MapMember(x => x.IsPickedUp)
                        .SetElementName("isPickedUp");

                    cm.MapMember(x => x.PickedUpTime)
                        .SetElementName("pickedUpTime");

                    cm.MapMember(x => x.IsDroppedOff)
                        .SetElementName("isDroppedOff");

                    cm.MapMember(x => x.DroppedOffTime)
                        .SetElementName("droppedOffTime");

                    cm.MapMember(x => x.SkipPickup)
                        .SetElementName("skipPickup");
                });
            }
        }
    }
}