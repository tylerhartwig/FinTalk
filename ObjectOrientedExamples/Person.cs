using System;
using Microsoft.VisualBasic.CompilerServices;

namespace ObjectOrientedExamples
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private readonly DateTime birthday = DateTime.Now;

        public Person(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public static bool operator== (Person a, Person b)
        {
            return a.birthday == b.birthday && a.Id == b.Id && a.Name == b.Name;
        }

        public static bool operator!= (Person a, Person b)
        {
            return !(a == b);
        }
    }
}