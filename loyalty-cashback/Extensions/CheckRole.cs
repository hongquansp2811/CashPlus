using System;
using System.Collections.Generic;
using LOYALTY.Controllers;

namespace LOYALTY.Extensions
{
    public class CheckRole
    {
        public static bool Role(string pre, string function, int Role)
        {
            var AllPer = pre.Split("+/");

            foreach (var x in AllPer)
            {
                var actionCode = x.Split("+");
                if (actionCode[0] == function)
                {
                    foreach (var i in actionCode)
                    {
                        if (i == Role.ToString())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    public static string convertListFuncTotring(List<user_permissions> lst)
    {

        string code = "";

        lst.ForEach(item =>
        {
            if (code != "") code += "/";
            code += item.function_code + "+";

            item.actions.ForEach(child =>
            {
                code += child.action_type + "+";
            });
        });
        return code;
    }        
    }
}