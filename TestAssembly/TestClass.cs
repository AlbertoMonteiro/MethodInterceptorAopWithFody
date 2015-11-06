using System.Collections.Generic;

namespace TestAssembly
{
    public class TestClass
    {
        public static List<string> Strings { get; set; } = new List<string>();

        [MyCustomMethodInterception]
        public void MyMethod()
        {
            Strings.Add($"Running {nameof(MyMethod)}");
        }
    }
}
