using System;

namespace Tangl
{
    public class TanglAttribute : Attribute
    {
        private string _target;
        public TanglAttribute(string target)
        {
            _target = target;
        }

        public string Target => _target;
    }
}
