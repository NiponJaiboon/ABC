using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DbContextNameAttribute : Attribute
    {
        public string Name { get; }

        public DbContextNameAttribute(string name) => Name = name;
    }
}
