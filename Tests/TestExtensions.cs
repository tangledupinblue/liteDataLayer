using System;

namespace LiteDataLayer.Tests
{
    public static class TestExtensions
    {
        public static bool ExceptionThrown(this Action self) {
            try {
                self.Invoke();
                return false;
            } catch (Exception ex) {
                Console.WriteLine("Expected exception executing\r\n{0}\r\n{1}",
                        self.ToString(), ex.Message);
                return true;
            }
        } 

        public static bool ExceptionThrown<T>(this Action self, T exceptionType) {
            try {
                self.Invoke();
                return false;
            } catch (Exception ex) {
                Console.WriteLine("Expected exception executing\r\n{0}\r\n{1}",
                        self.ToString(), ex.Message);
                return (ex.GetType() == typeof(T));
            }
        }
    }     
}