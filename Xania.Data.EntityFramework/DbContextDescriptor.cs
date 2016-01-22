using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Xania.Data.EntityFramework
{
    public static class DbContextDescriptor
    {
        public static DbContextDescriptor<TContext> Create<TContext>()
            where TContext: DbContext, new()
        {
            return new DbContextDescriptor<TContext>(() => new TContext());
        }

        public static IEnumerable<XmlElement> GetChildElements(this XmlNode xml, params string[] pathArr)
        {
            var children = xml.ChildNodes.OfType<XmlElement>();

            if (pathArr.Length == 0)
                return children;

            // ReSharper disable once AssignNullToNotNullAttribute
            for (int i = 0; i < pathArr.Length; i++)
            {
                var path = pathArr[i];
                children = children
                    .Where(e => e.HavingName(path))
                    .SelectMany(e => e.ChildNodes.OfType<XmlElement>());
            }

            return children;
        }

        public static bool HavingName(this XmlElement element, string name)
        {
            return element.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetAttributeValue(this XmlElement element, string attributeName)
        {
            return element.Attributes.OfType<XmlAttribute>()
                .Where(e => e.Name.Equals(attributeName))
                .Select(e => e.Value)
                .SingleOrDefault();
        }
    }

    public class DbContextDescriptor<TContext> where TContext: DbContext
    {
        private readonly Func<TContext> _factoryFunc;
        private XmlDocument _edmx;
        private ICollection<EdmxEntity> _entityTypes;

        public DbContextDescriptor(Func<TContext> factoryFunc)
        {
            _factoryFunc = factoryFunc;
        }

        private void EnsureInitialized()
        {
            // var db = 
        }

        public ICollection<EdmxEntity> GetEntityTypes()
        {
            if (_entityTypes != null)
                return _entityTypes;

            var edmx = GetEdmx().GetChildElements("Edmx", "Runtime", "ConceptualModels", "Schema");

            _entityTypes = (from XmlElement entityType in edmx
                where entityType.HavingName("EntityType")
                select new EdmxEntity(entityType)).ToArray();

            return _entityTypes;
        }

        public IEnumerable<EdmxAssociation> GetAssociations()
        {
            var edmx = GetEdmx().GetChildElements("Edmx", "Runtime", "ConceptualModels", "Schema");
            return
                from XmlElement asso in edmx
                where asso.HavingName("Association")
                select new EdmxAssociation(asso)
                {
                    Ends = from endElement in asso.ChildNodes.OfType<XmlElement>()
                           where endElement.HavingName("End")
                           join entityType in GetEntityTypes() on endElement.GetAttributeValue("Type") equals "Self."+entityType.Name
                           select new EdmxAssociationEnd(endElement)
                           {
                               EntityType = entityType
                           }
                };
        }

        private XmlDocument GetEdmx()
        {
            if (_edmx == null)
            {
                string edmxContent = null;
                using (var db = _factoryFunc())
                {
                    var stringWriter = new StringWriter();
                    var xmlWriter = new XmlTextWriter(stringWriter);
                    EdmxWriter.WriteEdmx(db, xmlWriter);

                    edmxContent = stringWriter.ToString();
                }

                using (var fs = new StreamWriter(@"C:\dev\temp\edmx.xml", false))
                {
                    fs.Write(edmxContent);
                    fs.Flush();
                }

                _edmx = new XmlDocument();
                _edmx.LoadXml(edmxContent);
            }
            return _edmx;
        }
    }

    public class EdmxAssociation
    {
        private readonly XmlElement _element;

        public EdmxAssociation(XmlElement element)
        {
            _element = element;
        }

        public string Name
        {
            get { return _element.GetAttributeValue("Name"); }
        }

        public IEnumerable<EdmxAssociationEnd> Ends { get; internal set; }
    }

    public class EdmxAssociationEnd
    {
        private readonly XmlElement _element;

        public EdmxAssociationEnd(XmlElement element)
        {
            _element = element;
        }

        public EdmxEntity EntityType { get; internal set; }
    }

    public class EdmxEntity
    {
        private readonly XmlElement _element;

        public EdmxEntity(XmlElement element)
        {
            _element = element;
        }

        public Type ClrType
        {
            get
            {
                var typeName = _element.GetAttributeValue("customannotation:ClrType");

                if (typeName == null)
                    return null;

                return Type.GetType(typeName, true);
            }
        }

        public object Name
        {
            get { return _element.GetAttributeValue("Name"); }
        }
    }
}
