using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoymaxTestBot.Models
{
    public class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Patronymic { get; set; }
        public DateTime DateBirth { get; set; }

    }
}
