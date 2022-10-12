using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MTool.AppUpdaterLib.Runtime.Interfaces
{
    public interface IResUpdateRuleFilter
    {
        bool Filter(ref string remoteName);
    }
}
