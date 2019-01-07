using System;
namespace ConceptDownloader.Services
{
    [AttributeUsage(AttributeTargets.Class |AttributeTargets.Struct)]
    public class ServiceAttribute : Attribute
    {
        public string Name { get; set; }
        public ServiceAttribute(string name)
        {
            this.Name = name;
        }
    }
}
