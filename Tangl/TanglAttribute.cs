using System;

namespace Tangl
{
    public class TanglAttribute : Attribute
    {
        private string _target;
        private readonly bool _includeAttributes;
        private readonly string _except;

        public TanglAttribute(Type type, string propertyName, bool includeAttributes = true, string except = null)
        {
            _target = $"{type.FullName}.{propertyName}";
            _includeAttributes = includeAttributes;
            _except = except;
        }

        public TanglAttribute(string target, bool includeAttributes = true, string except = null)
        {
            _target = target;
            _includeAttributes = includeAttributes;
            _except = except;
        }

        public string Target => _target;
        public string Except => _except;
        public bool IncludeAttributes => _includeAttributes;
    }
}
