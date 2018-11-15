using System;

namespace ObjectOrientedExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var person1 = new Person(id: 42, name: "Tyler Hartwig");
            var person2 = new Person(id: 42, name: "Tyler Hartwig");
            
            // person1 == person2 ?
            
            Console.WriteLine(person1 == person2);
        }
    }
}