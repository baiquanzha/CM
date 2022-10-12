using System.IO;
using UnityEngine;
using MTool.Core.Pipeline;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class ClearLuaBytesAction : BaseBuildFilterAction
    {
        public override bool Test(IFilter filter, IPipelineInput input)
        {
            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            ClearLuaBytesFile(filter, input);
            this.State = ActionState.Completed;
        }

        private void ClearLuaBytesFile(IFilter filter, IPipelineInput input)
        {
            string luabytesfolder = Application.dataPath + "/lua";
            string metapath = Application.dataPath + "/lua.meta";
            if (Directory.Exists(luabytesfolder))
            {
                Directory.Delete(luabytesfolder, true);
            }
            if (File.Exists(metapath))
            {
                File.Delete(metapath);
            }
            Logger.Info("Clear lua bytes file.");
        }
    }
}