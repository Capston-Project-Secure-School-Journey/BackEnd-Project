using Api.Common.Enums;

namespace Api.Domain.Models;

public class Parent : User
{
    public List<RelationshipWithStudent> RelationshipWithStudents { get; set; }
    public Parent()
    {
        RelationshipWithStudents = [];
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class RelationshipWithStudent
{
    public Relationship Relationship { get; set; }
    public Guid StudentId { get; set; }
}