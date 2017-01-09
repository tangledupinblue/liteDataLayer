using System;

namespace LiteDataLayer.Orm
{
    public class LiteOrmException : Exception
    {
        public LiteOrmException(string message) : base (message) {
        }
    }
}