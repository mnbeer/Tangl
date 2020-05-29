using System;

namespace Tangl
{
    public class TanglAttribute : Attribute
    {
        private string _target;

        public TanglAttribute(Type type, string propertyName)
        {
            _target = $"{type.FullName}.{propertyName}";
        }

        public TanglAttribute(string target)
        {
            _target = target;
        }

        public string Target => _target;
    }
}
