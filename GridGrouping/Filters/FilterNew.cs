using Microsoft.CSharp;
using Mubasher.ClientTradingPlatform.Infrastructure.CompilerServices;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters
{
    public class FilterNew
    {
        string templatedCode = @"
            using System;
            using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters;
            using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping;

            namespace FilterAssembly
            {
                public class FilterInvoker : IFilterInvoker
                {
                    public bool Compare(Record record, object[] inputs)
                    {
                    " +
                        "#$$$"
                        + @"
                    }

                    public bool Compare(object record, object[] inputs)
                    {
                    " +
                        "&$$$"
                        + @"
                    }
                }
            }";

        private IFilterInvoker filterInvoker = null;
        public object[] InputArray = new object[0];

        private string Code { get; set; } = string.Empty;
        private List<string> Columns { get; set; } = new List<string>();

        public bool IsFilterAvailable()
        {
            if (!string.IsNullOrWhiteSpace(Code) && Columns.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsFilterAvailable(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(Code) && Columns.Count > 0 && Columns.Contains(name))
            {
                return true;
            }

            return false;
        }

        public FilterNew()
        {

        }

        public FilterNew(string code, List<string> columns, GridGroupingControl grid, Type defualtType, object[] inputs)
        {
            Code = code;
            Columns = columns;
            InputArray = inputs;

            try
            {
                if (defualtType != null)
                {
                    string finalCode = templatedCode;
                    string methodCode = "\n";

                    foreach (var item in columns)
                    {
                        if (grid.Table.Columns.Contains(item))
                        {
                            TableInfo.TableColumn col = grid.Table.Columns[item];

                            if (!string.IsNullOrWhiteSpace(col.MappingName))
                            {
                                methodCode += "var " + col.Name + " = (record.GetData() as " + defualtType.FullName + ")." + col.MappingName + ";";
                                methodCode += "\n";

                                methodCode += $" if({col.Name} == null) return false;";
                            }
                        }
                    }

                    methodCode += @"
                        if " + code;
                    methodCode += @"
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }";

                    //finalCode = finalCode.Replace("*", objectType.Namespace);
                    finalCode = finalCode.Replace("#$$$", methodCode);
                    finalCode = finalCode.Replace("&$$$", methodCode.Replace("record.GetData()", "record"));

                    var compiledClassType = GenericCompiler.GetCompiledCode(finalCode, "FilterAssembly.FilterInvoker", new List<Type>() { defualtType, typeof(IFilterInvoker) });

                    if (compiledClassType != null)
                    {
                        filterInvoker = Activator.CreateInstance(compiledClassType) as IFilterInvoker;
                    }
                    else
                    {
                        filterInvoker = null;
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        public bool Compare(Record record)
        {
            if (filterInvoker != null)
            {
                return filterInvoker.Compare(record, InputArray);
            }
            else
            {
                return true;
            }
        }

        public bool Compare(object record)
        {
            if (filterInvoker != null)
            {
                return filterInvoker.Compare(record, InputArray);
            }
            else
            {
                return true;
            }
        }
    }

    public interface IFilterInvoker
    {
        bool Compare(Record record, object[] inputs);

        bool Compare(object record, object[] inputs);
    }
}
