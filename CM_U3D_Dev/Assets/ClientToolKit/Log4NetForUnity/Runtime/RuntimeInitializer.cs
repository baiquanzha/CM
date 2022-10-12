using MTool.Log4NetForUnity.Runtime;
using log4net.Config;
using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Log4Net.Unity
{
    public sealed class RuntimeInitializer
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitLog4Net()
        {
            XmlDocument doc = null;
            string xmlConfigTextContents;
            string configPath = $"{Application.persistentDataPath}/{Const.ConfigName}{Const.ConfigSuffix}";

            if (File.Exists(configPath))
            {
                Debug.Log("Use application config from read&write path .");
                xmlConfigTextContents = File.ReadAllText(configPath,new System.Text.UTF8Encoding(false,true));
            }
            else
            {
                var asset = Resources.Load<TextAsset>(Const.ConfigName);
                if (!asset)
                {
                    Debug.LogWarning($"Current application no file that name is \"{Const.ConfigName}\" , use default xmlconfig contents.");
                    xmlConfigTextContents = Const.DefaultConfig;
                }
                else
                {
                    xmlConfigTextContents = asset.text;
                    Resources.UnloadAsset(asset);
                }
            }

            doc = new XmlDocument();
            doc.LoadXml(xmlConfigTextContents);
            XmlConfigurator.Configure(doc.DocumentElement);
        }

#endregion

    }
}
