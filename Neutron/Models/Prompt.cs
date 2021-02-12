namespace Neutron.Models
{
    public class Prompt
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Prompt(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
