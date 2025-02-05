namespace Api.Domain.Models;

using Api.Common.Enums;

public class Parrent: User
{
    public Relationship RelationshipWithStudent { get; set; }
}