using System;
using System.Linq;
using System.Text;
using MTool.AppBuilder.Runtime.Configuration;
using MTool.Core.Pipeline;
using YamlDotNet;
using File = System.IO.File;
using UnityEngine;
using MTool.AppBuilder.Editor.Builds.Contexts;
using MTool.LoggerModule.Runtime;
using MTool.AppBuilder.Editor.Builds.InnerLoggers;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppBuilder.Editor.Builds
{
    public sealed class AppBuilderPipelineProcessor : BasePipelineProcessor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public override IPipelineContext Context { get; } = new AppBuildContext();


        private ILogger mLogger;
        protected override ILogger Logger
        {
            get
            {
                if (mLogger == null)
                {
                    mLogger = LoggerManager.GetLogger("AppBuilderPipelineProcessor");
                }
                return mLogger;
            }
        }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public override ProcessResult Process(IPipelineInput input)
        {
            ProcessResult result;
            if (!this.TestAll(input))
            {
                result = ProcessResult.Create(ProcessState.Error, $"Test failure , error info : {Context.Error} .");
            }
            else
            {
                result = base.Process(input);
            }
            
            if (result.State == ProcessState.Error)
            {
                Logger?.Error(Context.Error);
            }

            return result;
        }

        /// <summary>
        /// 根据编译过程配置初始化管线处理器
        /// </summary>
        /// <param name="config">编译过程配置</param>
        public void InitFormConfig(AppBuildProcessConfig config)
        {
            foreach (var filterConfig in config.Filters)
            {
                var filterType = GetActionType(filterConfig.TypeFullName);
                var instance = (IFilter)System.Activator.CreateInstance(filterType);
                this.Register(instance);
                if (filterConfig.Action.IsActionQueue)
                {
                    var queueActionsFilter = instance as QueueActionsPipelineFilter;
                    foreach (var childAction in filterConfig.Action.Childs)
                    {
                        var actionType = GetActionType(childAction.TypeFullName);
                        var actionInst = (IPipelineFilterAction)Activator.CreateInstance(actionType);
                        queueActionsFilter.Enqueue(actionInst);
                    }
                }
                else
                {
                    var actionType = GetActionType(filterConfig.Action.TypeFullName);
                    var actionInst = (IPipelineFilterAction)Activator.CreateInstance(actionType);
                    BasePipelineFilter basePipelineFilter = instance as BasePipelineFilter;
                    basePipelineFilter.SetAction(actionInst);
                }
            }
        }

        private Type GetActionType(string typeFullName)
        {
            var actionType = Type.GetType(typeFullName);
            if (actionType == null)
            {
                actionType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                    from type in assembly.GetTypes()
                    where
                        type.FullName == typeFullName
                    select type).FirstOrDefault();
            }

            if(actionType == null)
                throw new TypeLoadException($"Invallid type , full name is \"{typeFullName}\" .");
            return actionType;
        }

        private static AppBuildProcessConfig GetAppBuildProcessConfig(string configPath)
        {
            string content = File.ReadAllText(configPath, new UTF8Encoding(false, true));
            AppBuildProcessConfig config = YAMLSerializationHelper.DeSerialize<AppBuildProcessConfig>(content);
            return config;
        }

        /// <summary>
        /// 读取编译过程配置并初始化管线处理器
        /// </summary>
        /// <param name="configPath">编译过程配置的路径</param>
        /// <returns></returns>
        public static AppBuilderPipelineProcessor ReadFromBuildProcessConfig(string configPath)
        {
            var config = GetAppBuildProcessConfig(configPath);

            if (config == null)
            {
                var logger = LoggerManager.GetLogger(typeof(AppBuilderPipelineProcessor).Name);
                logger.Error($"Get build process config failure ! Config path : {configPath} .");
                return null;
            }

            var processor = new AppBuilderPipelineProcessor();
            
            processor.InitFormConfig(config);

            return processor;
        }

        #endregion

    }
}
