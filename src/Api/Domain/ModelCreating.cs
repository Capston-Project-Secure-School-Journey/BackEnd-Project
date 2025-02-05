using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain
{
    public static class ModelCreating
    {
        public static ModelBuilder OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasDiscriminator<string>("user_type")
                .HasValue<Driver>("driver")
                .HasValue<Parrent>("parrent")
                .HasValue<User>("user")
                .HasValue<SchoolPerson>("school_person");
            
            return builder;
        }
    }
}
