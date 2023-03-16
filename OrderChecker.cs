using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Converters;

namespace language_prog_simu_6DOF
{
    class OrderChecker
    {
        public bool error = false;

        public bool running = false;
        public int runningTime = 0;

        private string condition = "EQ,NEQ,GT,LT,LE";

        public int curIndex;

        public double[] targetPos = new double[6];

        public double[] limitsPos = new double[6]; //x, y, z, yaw, pitch, roll

        private List<Variable> _variables = new List<Variable>();
        public List<Tuple<string, int>> labels;

        public List<Variable> Variables
        {
            get { return _variables; }
            set { _variables = value; }
        }
        public bool OrderCheck(string[] words, int index)
        {
            this.curIndex = index;
            double[] args = new double[3];
            Variable? var = null;
            switch (words[0].Trim())
            {
                case "LET":
                    //Nombre arguments = 3 ?
                    CheckNbArgs(3, words);

                    //2eme argument est bien une variable ?
                    IsVariable(words[1]);

                    //est ce que le 3eme est une variable ?
                    var = FindVariable(words[2].Trim());
                    if (var != null && !error)
                        LET(words[1], var.val);
                    else
                    {
                        int val;
                        if (int.TryParse(words[2].Trim(), out val) && !error)
                            LET(words[1], val);
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be written numerically or be a variable!", "Alert");
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
                    var = FindVariable(words[2].Trim());
                    if (var != null && !error)
                        INC(words[1], var.val);
                    else
                    {
                        int val;
                        if (int.TryParse(words[2].Trim(), out val) && !error)
                            INC(words[1], val);
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be written numerically or be a variable!", "Alert");
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
                    var = FindVariable(words[2].Trim());
                    if (var != null && !error)
                        MUL(words[1], var.val);
                    else
                    {
                        double val;
                        if (double.TryParse(words[2].Trim(), out val) && !error)
                            MUL(words[1], val);
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be written numerically or be a variable!", "Alert");
                            error = true;
                        }
                    }
                    break;

                case "POS_ABS":
                    if(CheckNbArgs(4, words))
                    {
                        //est ce un nombre convertissable en int
                        for (int i = 1; i <= 3; i++)
                        {
                            if (!double.TryParse(words[i].Trim(), out args[i - 1]))
                            {
                                //est il une variable
                                if (IsVariable(words[i].Trim(), false))
                                {
                                    args[i - 1] = FindVariable(words[i].Trim()).val;
                                }
                                else
                                {
                                    //est il un "*"
                                    if (IsStar(words[i].Trim(), false))
                                        args[i - 1] = targetPos[i - 1];
                                    else
                                    {
                                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[i]} must be a variable, a \'*\' or written numerically!", "Alert");
                                        error = true;
                                    }
                                }
                            }
                        }
                        if (!error)
                        {
                            for (int i = 0; i < 3; i++)
                                targetPos[i] = args[i];
                        }
                    }
                    break;

                case "POS_REL":
                    if(CheckNbArgs(4, words))
                    {
                        //est ce un nombre convertissable en int
                        for (int i = 1; i <= 3; i++)
                        {
                            if (!double.TryParse(words[i].Trim(), out args[i - 1]))
                            {
                                //est il une variable
                                if (IsVariable(words[i].Trim(), false))
                                {
                                    args[i - 1] = FindVariable(words[i].Trim()).val;
                                }
                                else
                                {
                                    //est il un "*"
                                    if (IsStar(words[i].Trim(), false))
                                        args[i - 1] = 0;
                                    else
                                    {
                                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[i]} must be a variable, a \'*\' or written numerically!", "Alert");
                                        error = true;
                                    }
                                }
                            }
                        }
                        if (!error)
                        {
                            for (int i = 0; i < 3; i++)
                                targetPos[i] += args[i];
                        }
                    }                    
                    break;

                case "ROT_ABS":
                    if (CheckNbArgs(4, words))
                    {
                        //est ce un nombre convertissable en int
                        for (int i = 1; i <= 3; i++)
                        {
                            if (!double.TryParse(words[i], out args[i - 1]))
                            {
                                //est il une variable
                                if (IsVariable(words[i], false))
                                {
                                    args[i - 1] = FindVariable(words[i]).val;
                                }
                                else
                                {
                                    //est il un "*"
                                    if (IsStar(words[i].Trim(), false))
                                        args[i - 1] = targetPos[i + 2];
                                    else
                                    {
                                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[i]} must be a variable, a \'*\' or written numerically!", "Alert");
                                        error = true;
                                    }
                                }
                            }
                        }
                        if (!error)
                        {
                            for (int i = 0; i < 3; i++)
                                targetPos[i + 3] = args[i];
                        }
                    }          
                    break;

                case "ROT_REL":
                    if (CheckNbArgs(4, words))
                    {
                        //est ce un nombre convertissable en int
                        for (int i = 1; i <= 3; i++)
                        {
                            if (!double.TryParse(words[i], out args[i - 1]))
                            {
                                //est il une variable
                                if (IsVariable(words[i], false))
                                {
                                    args[i - 1] = FindVariable(words[i]).val;
                                }
                                else
                                {
                                    //est il un "*"
                                    if (IsStar(words[i].Trim(), false))
                                        args[i - 1] = 0;
                                    else
                                    {
                                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[i]} must be a variable, a \'*\' or written numerically!", "Alert");
                                        error = true;
                                    }
                                }
                            }
                        }
                        if (!error)
                        {
                            for (int i = 0; i < 3; i++)
                                targetPos[i + 3] += args[i];
                        }
                    }                    
                    break;

                case "RESET":
                    for (int i = 0; i < 6; i++)
                        targetPos[i] = 0;
                    break;

                case "RUN":
                    //Nombre arguments = 2 ?
                    CheckNbArgs(2, words);
                    if (!error)
                    {
                        if (int.TryParse(words[1].Trim(), out runningTime))
                            running = true;
                        else
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[1]} must be written numerically!", "Alert");
                    }
                    break;

                case "WAIT":
                    //Nombre arguments = 2 ?
                    CheckNbArgs(2, words);

                    //est ce que le 2eme est une variable ?
                    var = FindVariable(words[1]);
                    if (var != null && !error)
                        Thread.Sleep(Convert.ToInt32(var.val));
                    else
                    {
                        int val;
                        if (int.TryParse(words[1].Trim(), out val) && !error)
                        {
                            Debug.WriteLine("sleep");
                            Thread.Sleep(val);
                            Debug.WriteLine("wake up");
                        }
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[1]} must be written numerically or be a variable!", "Alert");
                            error = true;
                        }
                    }
                    break;

                case "GOTO":
                    //Nombre arguments = 2 ?
                    CheckNbArgs(2, words);

                    labels.ForEach(label =>
                    {
                        if (label.Item1 == words[1].Trim())
                            curIndex = label.Item2;
                    });
                    break;

                case "IF":
                    bool flag = false;
                    int labelIndex = 0;
                    //Nombre arguments = 2 ?
                    CheckNbArgs(5, words);

                    //est ce que le 2eme est une variable ?
                    var = FindVariable(words[1].Trim());
                    if (var != null && !error)
                        args[0] = var.val;
                    else
                    {
                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[1]} must be a variable!", "Alert");
                        error = true;
                        break;
                    }   

                    //est ce que le 3eme est un comparateur
                    foreach (var cond in condition.Split(','))
                        if (words[2] == cond)
                            flag = true;
                    if (!flag)
                    {
                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[2]} must be a valid comparator!", "Alert");
                        error = true;
                        break;
                    }

                    //est ce que le 4eme est une variable ?
                    var = FindVariable(words[3]);
                    if (var != null && !error)
                        args[1] = var.val;
                    else
                    {
                        int val;
                        if (int.TryParse(words[3].Trim(), out val) && !error)
                            args[1] = val;
                        else
                        {
                            MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[3]} must be written numerically or be a variable!", "Alert");
                            error = true;
                            break;
                        }
                    }

                    //est ce que le 5eme est un label ?
                    labels.ForEach(label =>
                    {
                        if (words[4].Trim() == label.Item1)
                        {
                            labelIndex = label.Item2;
                            flag = false;
                        }
                    });

                    if (flag)
                    {
                        MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the word {words[4]} must be a Label!", "Alert");
                        error = true;
                        break;
                    }

                    if (!error)
                        IF(args[0], words[2], args[1], labelIndex);

                    break;

                case "LABEL":
                    break;

                default: //le 1er mot ne correspond à aucun ordres
                    error = true;
                    MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the order word may have been badly written!", "Alert");
                    break;
            }
            return error;
        }
        public void CreateLabel(string[] words, int index)
        {
            if (words[0] == "LABEL")
            {
                //Nombre arguments = 2 ?
                CheckNbArgs(2, words);

                //ajout du label dans une liste de Tuple<string, int>
                labels.Add(new Tuple<string, int>(words[1].Trim(), index));
            }
        }
        private void LET(string name, double val)
        {
            Variable var = FindVariable(name);
            if (var != null)
                Variables[var.id].val = val;
            else
                Variables.Add(new Variable(Variables.Count(), name, val));
        }
        private void INC(string varName, double valInc)
        {
            Variable var = FindVariable(varName);
            var.val += valInc;
        }
        private void MUL(string varName, double valInc)
        {
            Variable var = FindVariable(varName);
            var.val *= valInc;
        }
        private void IF(double varVal, string comp, double var2Val, int labelIndex)
        {
            switch (comp)
            {
                case "EQ":
                    if(varVal == var2Val)
                        curIndex = labelIndex;
                    break;
                case "NEQ":
                    if (varVal != var2Val)
                        curIndex = labelIndex;
                    break;
                case "GT":
                    if (varVal > var2Val)
                        curIndex = labelIndex;
                    break;
                case "GE":
                    if (varVal >= var2Val)
                        curIndex = labelIndex;
                    break;
                case "LT":
                    if (varVal < var2Val)
                        curIndex = labelIndex;
                    break;
                case "LE":
                    if (varVal <= var2Val)
                        curIndex = labelIndex;
                    break;
            }
        }
        public bool NormalizeTargetPos(double[] limitPosPlus, double[] limitPosMinus)
        {
            bool flag = false;
            //Verificatation des limits
            for (int i = 0; i < 6; i++)
            {
                if (targetPos[i] > limitPosPlus[i])
                {
                    targetPos[i] = limitPosPlus[i];
                    flag = true;
                }
                else if (targetPos[i] < limitPosMinus[i])
                {
                    targetPos[i] = limitPosMinus[i];
                    flag = true;
                }
            }
            return flag;
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
        private bool IsStar(string arg, bool msg = true)
        {
            if (arg.Length != 1 || arg != "*")
            {
                if (msg)
                {
                    error = true;
                    MessageBox.Show($"Line {curIndex + 1} : Syntax Error! {arg} must be a \'*\'", "Alert");
                    return false;
                }
                else
                    return false;
            }
            return true;
        }
        private bool CheckNbArgs(int nbArgs, string[] args)
        {
            if (args.Count() != nbArgs)
            {
                error = true;
                MessageBox.Show($"Line {curIndex + 1} : Syntax Error! the number of arguments is incorrect", "Alert");
                return false;
            }
            return true;
        }
    }
}
