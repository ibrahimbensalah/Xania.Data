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
using NUnit.Framework;

namespace Xania.Data.Tests
{
    public class EdmxReaderTests
    {
        public EdmxReaderTests()
        {
            Database.SetInitializer<XaniaDbContext>(null);

            using (var db = new XaniaDbContext())
            using (var fs = new FileStream("c:\\dev\\edmx.xml", FileMode.Create, FileAccess.Write))
            {
                var writer = new XmlTextWriter(fs, Encoding.Default);
                EdmxWriter.WriteEdmx(db, writer);
            }
        }

        [Test]
        public void Should_discover_correct_entity_types()
        {
            var expectedTypes = new[] {typeof(Organisation), typeof(Employee), typeof(Role)};
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

        // public int OrganisationId { get; set; }

        public virtual Organisation Organisation { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
