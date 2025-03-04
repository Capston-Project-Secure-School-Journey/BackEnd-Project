using Api.Common.Enums;

namespace Api.Domain.Models;

public class Parent: User
{
    public Parent()
    {
        RelationshipWithStudents = [];
    }
    
    public List<RelationshipWithStudent> RelationshipWithStudents { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class RelationshipWithStudent
{
    public Relationship Relationship { get; set; }
    public Guid StudentId { get; set; }
}