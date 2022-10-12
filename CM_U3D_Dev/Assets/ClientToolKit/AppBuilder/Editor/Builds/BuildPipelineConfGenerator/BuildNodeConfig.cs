using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTool.AppBuilder.Editor.Builds.Actions;
using MTool.AppBuilder.Runtime.BuildPipelineConfGenerator;
using MTool.Core.Pipeline;

namespace MTool.AppBuilder.Editor.Builds.BuildPipelineConfGenerator
{
    public static class BuildNodeBaseTypeDic
    {
        public readonly static Dictionary<BuildNodeType, Type> Data = new Dictionary<BuildNodeType, Type>
        {
            { BuildNodeType.Action,typeof(BaseBuildFilterAction)},
            {BuildNodeType.Filter,typeof(BasePipelineFilter)}
        };
    }
    

}
