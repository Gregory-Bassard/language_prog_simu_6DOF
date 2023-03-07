using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace language_prog_simu_6DOF
{
    class OrderChecker
    {
        public bool error = false;

        private int curIndex;

        private List<Variable> _variables = new List<Variable>();
        public List<Variable> Variables
        {
            get { return _variables; }
            set { _variables = value; }
        }

        public bool OrderCheck(string[] words, int index)
        {
            curIndex = index;
            string[] args = new string[4];
            Variable var = null;
            switch (words[0].Trim())
            {
                case "LET":
                    //Nombre arguments = 3 ?
                    CheckNbArgs(3, words);

                    //2eme argument est bien une variable ?
                    IsVariable(words[1]);

                    //est ce que le 3eme est une variable ?
                    var = FindVariable(words[2]);
                    if (var != null && !error)
                        LET(words[1], var.val);
                    else
                    {
                        int val;
                        if (int.TryParse(words[2], out val) && !error)
                            LET(words[1], val);
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be written numerically or a variable", "Alert");
                            error = true;
                        }
                    }
                    break;
                case "INC":
                    //Nombre arguments = 3 ?
                    CheckNbArgs(3, words);

                    //2eme argument est bien une variable ?
                    IsVariable(words[1]);

                    //est ce que le 3eme est une variable ?
                    var = FindVariable(words[2]);
                    if (var != null && !error)
                        INC(words[1], var.val);
                    else
                    {
                        int val;
                        if (int.TryParse(words[2], out val) && !error)
                            INC(words[1], val);
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be written numerically or a variable", "Alert");
                            error = true;
                        }
                    }
                    break;
                case "MUL":
                    //Nombre arguments = 3 ?
                    CheckNbArgs(3, words);

                    //2eme argument est bien une variable ?
                    IsVariable(words[1]);

                    //est ce que le 3eme est une variable ?
                    var = FindVariable(words[2]);
                    if (var != null && !error)
                        MUL(words[1], var.val);
                    else
                    {
                        int val;
                        if (int.TryParse(words[2], out val) && !error)
                            MUL(words[1], val);
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be written numerically or a variable", "Alert");
                            error = true;
                        }
                    }
                    break;
                case "POS_ABS":
                    break;
                case "POS_REL":
                    break;
                case "ROT_ABS":
                    break;
                case "ROT_REL":
                    break;
                case "RESET":
                    break;
                case "VERRIN_ABS":
                    break;
                case "VERRIN_REL":
                    break;
                case "RUN":
                    break;
                case "WAIT":
                    break;
                case "LABEL":
                    break;
                case "GOTO":
                    break;
                default: //le 1er mot ne correspond à aucun ordres
                    error = true;
                    MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the order word may have been badly written", "Alert");
                    break;
            }
            if (!error)
            {

            }
            return error;
        }
        private void LET(string name, int val)
        {
            if (FindVariable(name) != null)
            {
                MessageBox.Show($"Line {curIndex + 1} : Conflict Error! variable {name} already exist", "Alert");
            }

            Variable var = new Variable(Variables.Count(), name, val);
            Variables.Add(var);
        }
        private void INC(string varName, int valInc)
        {
            Variable var = FindVariable(varName);
            var.val += valInc;
        }
        private void MUL(string varName, int valInc)
        {
            Variable var = FindVariable(varName);
            var.val *= valInc;
        }
        private Variable FindVariable(string arg)
        {
            if (IsVariable(arg, false))
                foreach (Variable var in Variables)
                    if (arg == var.name)
                        return var;
            return null;
        }
        private bool IsVariable(string arg, bool msg = true)
        {
            if (arg.Length <= 1 || arg[0] != '$')
            {
                if (msg)
                {
                    error = true;
                    MessageBox.Show($"Line {curIndex + 1} : Syntax Error! {arg} must be a variable", "Alert");
                    return false;
                }
                else
                    return false;
            }
            return true;
        }
        private void CheckNbArgs(int nbArgs, string[] args)
        {
            if (args.Count() != nbArgs)
            {
                error = true;
                MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the number of arguments is incorrect", "Alert");
            }
        }
    }
}
