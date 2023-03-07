using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace language_prog_simu_6DOF
{
    class Variable
    {
        public int id { get; set; }
        public string name { get; set; }
        public int val { get; set; }
        public Variable(int id, string name, int val)
        {
            this.id = id;
            this.name = name;
            this.val = val;
        }
        public static Variable SearchVar(List<Variable> vars, string varName)
        {
            Variable var = null;

            vars.ForEach(v =>
            {
                if (v.name == varName)
                {
                    var = v;
                }
            });
            return var;
        }
    }
}
