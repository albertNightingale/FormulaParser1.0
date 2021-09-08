using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaEvaluator
{
    /// <summary>
    /// Evaluator library that is used to evaluate an expression and return an result
    /// @Author: AL083019
    /// </summary>
    public class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// Convert the variable into Integer values
        /// </summary>
        /// <param name="variable"> variable that is to be converted </param>
        /// <throw> ArgumentException if variable is null</throw>
        /// <throw> ArgumentException if variable is empty </throw>
        /// <returns></returns>
        public static int AllSevens(String variable)
        {
            if (variable == null)
                throw new ArgumentException("Invalid Argument because the variable entered is NULL. ");
            if (variable.Equals(""))
                throw new ArgumentException("Argument is empty");
            if (variable.Equals("A7"))
                return 7;
            else if (variable.Equals("A9"))
                return 9;
            else
                return 0;
        }

        /// <summary>
        /// Evaluate an expression and return the result in Integer
        /// </summary>
        /// <param name="exp"> The expression to be evaluated</param>
        /// <param name="variableEvaluator">Delegate Method </param>
        /// <returns>The result in Integer </returns>
        public static int Evaluate(string exp, Lookup variableEvaluator)
        {
            if (exp == null)
                throw new ArgumentException("Expression is null! Unable to Evaluate the expression! ");
            else if (exp.Equals("") || exp.Equals(" "))
                throw new ArgumentException("Expression is empty or only contains spaces");

            Stack<string> ops = new Stack<string>();
            Stack<int> val = new Stack<int>();

            string expwospace = string.Join("", exp.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)); // remove spaces

            // Console.WriteLine("expression: " + expwospace);

            string[] substrings = Regex.Split(expwospace, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");   // split out the expression into parts

            string s;
            for (int idx = 0; idx < substrings.Length; idx++)   // construct Stacks
            {
                s = substrings[idx];
                if (s.Equals(""))
                {
                    continue;
                }

                if (Determinetype(s).Equals("operator"))
                {
                    // Console.WriteLine("NEW Operator: " + s);

                    if (s.Equals("-") || s.Equals("+"))
                    {
                        if (ops.Count != 0 && (ops.Peek().Equals("-") || ops.Peek().Equals("+")))
                        {
                            if (val.Count >= 2)
                                val.Push(Doarith(val.Pop(), ops.Pop(), val.Pop()));
                            else
                                throw new ArgumentException("Invalid expression: " + ops.Peek() + " is in invalid position in " + expwospace);
                        }
                        ops.Push(s);
                    }
                    else if (s.Equals("/") || s.Equals("*"))
                    {
                        ops.Push(s);
                    }
                    else if (s.Equals("("))
                    {
                        ops.Push(s);
                    }
                    else if (s.Equals(")"))
                    {
                        if (ops.Count != 0 && (ops.Peek().Equals("-") || ops.Peek().Equals("+")))
                        {
                            if (val.Count >= 2)
                                val.Push(Doarith(val.Pop(), ops.Pop(), val.Pop()));
                            else
                                throw new ArgumentException("Invalid expression: " + ops.Peek() + " is in invalid position in " + expwospace);
                        }

                        if (ops.Count != 0 && ops.Peek().Equals("(")) // next element should be (
                        {
                            ops.Pop();
                        }
                        else
                        {
                            throw new ArgumentException("Invalid expression: ')' does not have a matching '(' in " + expwospace);
                        }

                        if (ops.Count != 0 && (ops.Peek().Equals("/") || ops.Peek().Equals("*")))
                        {
                            if (val.Count >= 2)
                                val.Push(Doarith(val.Pop(), ops.Pop(), val.Pop()));
                            else
                                throw new ArgumentException(ops.Peek() + " is in invalid positions in expression: " + expwospace);
                        }
                    }
                    else
                    {
                        // Error
                        throw new ArgumentException("Operators are invalid: " + s + " is in invalid position in " + expwospace);
                    }
                }
                else
                {
                    string value = "";
                    if (Determinetype(s).Equals("variable")) // s is a variable 
                    {
                        // Console.WriteLine("NEW Variable: " + s);
                        value = variableEvaluator(s).ToString(); // convert the variable into String
                    }
                    else if (Determinetype(s).Equals("value"))  // s is a value
                    {
                        // Console.WriteLine("NEW Value: " + s);
                        value = s.ToString();
                    }

                    int valueinint = int.Parse(value); 
                    if (val.Count == 0) // empty
                    {
                        val.Push(valueinint);
                    }
                    else if (ops.Count != 0 && (ops.Peek().Equals("*") || ops.Peek().Equals("/")))
                    {
                        val.Push(Doarith(valueinint, ops.Pop(), val.Pop())); // push the result of s operation with the top value of val. 
                    }
                    else
                    {
                        val.Push(valueinint);
                    }
                }

                /* For debugging only, displays two stacks
                String[] opstmp = ops.ToArray();
                String[] valtmp = val.ToArray();

                Console.Write("ops: ");
                foreach (String s1 in opstmp)
                {
                    Console.Write(s1 + ", ");
                }
                Console.WriteLine();

                Console.Write("val: ");
                foreach (String s1 in valtmp)
                {
                    Console.Write(s1 + ", ");
                }
                Console.WriteLine();
                */

            }

            if (ops.Count == 0)
            {
                if (val.Count == 1)
                {
                    return val.Pop();
                }
                else // this case happens when the expression is weird 
                {
                    throw new ArgumentException("Invalid expression: " + expwospace);
                }
            }
            else if (ops.Count == 1)
            {
                if (val.Count == 2)
                {
                    return Doarith(val.Pop(), ops.Pop(), val.Pop());
                }
                else // this case happens when the expression is weird
                {
                    throw new ArgumentException("Invalid expression: " + expwospace);
                }
            }

            return -1;
        }

        /// <summary>
        /// Determine the type of a String, whether it is value, variable, or operator, or invalid String
        /// if the string is an int, then it is a value
        /// if the string is a Letter + int, then it is a variable
        /// if the string is (, ), /, *, +, or -, then it is an operator
        /// Else, the string is invalid 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static String Determinetype(String s)
        {
            if (IsStrNumber(s) == true)
            {
                return "value";
            }
            else if (s.Length >= 2 && Char.IsLetter(s[0]) && Char.IsDigit(s[1]))
            {
                return "variable";
            }
            else if (s.Equals("(") || s.Equals(")") || s.Equals("/") || s.Equals("*") || s.Equals("+") || s.Equals("-"))
            {
                return "operator";
            }
            else
                throw new ArgumentException("Unable to determine the type of the s");
        }

        /// <summary>
        /// Check if a string is a number
        /// </summary>
        /// <param name="s"> the string that is to be verified </param>
        /// <returns>true if is a number, false if it is not a number</returns>
        private static bool IsStrNumber(String s)
        {
            for (int idx = 0; idx < s.Length; idx++)
            {
                if (!Char.IsNumber(s[idx]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Do Arithmatic with operand a and b, and operator o. 
        /// if o is + or *, then return a+b or a*b 
        /// if o is - or /, then return b-a or b/a
        /// if a is 0 when o is /, then throw an Arithmatic Exception because cannot divide by 0. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="o"></param>
        /// <param name="b"></param>
        /// <returns>the result of the operation</returns>
        private static int Doarith(int a, string o, int b)
        {
            if (o == null)
            {
                throw new ArgumentException("one or more of the parameters is null!");
            }

            if (o.Equals("+"))
            {
                return a + b;
            }
            else if (o.Equals("-"))
            {
                return b - a;
            }
            else if (o.Equals("*"))
            {
                return a * b;
            }
            else if (o.Equals("/"))
            {
                if (a == 0)
                {
                    throw new ArgumentException("Invalid Math Calculation: Argument a is zero. ");
                }
                else
                {
                    return b/a;
                }
            }
            else
            {
                throw new ArgumentException("Parameter o is not the appropriate arithmatic operator! " +
                    "The only valid parameters are +, -, /, *");
            }

        }

    }
}
