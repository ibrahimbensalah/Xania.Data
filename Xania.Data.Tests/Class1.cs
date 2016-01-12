using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Xania.Data.EntityFramework;

namespace Xania.Data.Tests
{
    public class EdmxReaderTests
    {
        private readonly DbContextDescriptor<XaniaDbContext> _descriptor;

        public EdmxReaderTests()
        {
            Database.SetInitializer<XaniaDbContext>(null);
            _descriptor = DbContextDescriptor.Create<XaniaDbContext>();
        }

        [Test]
        public void Should_discover_correct_entity_types()
        {
            _descriptor.GetEntityTypes().Should().HaveCount(3);
            _descriptor.GetEntityTypes().Select(e => e.ClrType).Should()
                .BeEquivalentTo(typeof(Organisation), typeof(Employee), typeof(Role));
        }

        [Test]
        public void Should_discover_correct_entity_associations()
        {
            _descriptor.GetAssociations().Should().HaveCount(1);
            _descriptor.GetAssociations().Select(e => e.Name).Should()
                .BeEquivalentTo("Organisation_Employees");
        }

        [Test]
        public void Should_discover_correct_association_ends()
        {
            var asso = _descriptor.GetAssociations()
                .Single(e => e.Name.Equals("Organisation_Employees"));
            asso.Ends.Should().HaveCount(2);
            asso.Ends.Select(e => e.EntityType.ClrType).Should()
                .BeEquivalentTo(typeof(Organisation), typeof(Employee));
        }
    }


    public class XaniaDbContext : DbContext
    {
        public XaniaDbContext()
        {

        }

        public IDbSet<Organisation> Organisations { get; set; }

        public IDbSet<Employee> Employees { get; set; }
        public IDbSet<Role> Roles { get; set; }
    }

    public class Organisation
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int OrganisationId { get; set; }

        public virtual Organisation Organisation { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
