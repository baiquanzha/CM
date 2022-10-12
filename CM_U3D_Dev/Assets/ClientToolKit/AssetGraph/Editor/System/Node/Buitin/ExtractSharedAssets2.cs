using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine.Profiling;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph {
    [CustomNode("Configure Bundle/Extract Shared Assets Extend V0", 200)]
    public class ExtractSharedAssets2 : Node {

        //[Serializable]
        //public class SharedGroupAssets
        //{
        //    public string assetsNames;
        //}


        /// <summary>
        /// 需要被编组的文件夹
        /// </summary>
        [Serializable]
        public class NeedToGroupedFolder
        {
            public string folder;
        }

        /// <summary>
        /// 需要被过滤的文件夹
        /// </summary>
        [Serializable]
        public class NeedToFilterFolder
        {
            public string folder;
        }

        enum GroupingType : int {
            ByFileSize,
            ByRuntimeMemorySize
        };

        [SerializeField] private string m_bundleNameTemplate;
        [SerializeField] private string m_groupedNameTemplate;
        [SerializeField] private SerializableMultiTargetInt m_groupExtractedAssets;
        [SerializeField] private SerializableMultiTargetInt m_groupSizeByte;
        [SerializeField] private SerializableMultiTargetInt m_groupingType;


        //[SerializeField] private List<SharedGroupAssets> m_SharedGroupAssets;
        //ReorderableList m_SharedGroupList;

        //[SerializeField] private List<NeedToGroupedFolder> m_NeedToGroupedFolders;
        //ReorderableList m_NeedToGroupedFolderList;


        [SerializeField] private List<NeedToFilterFolder> m_NeedToFilterFolders = null;
        ReorderableList m_NeedToFilteredFolderList;

        public override string ActiveStyle {
    		get {
    			return "node 3 on";
    		}
    	}

    	public override string InactiveStyle {
    		get {
    			return "node 3";
    		}
    	}

    	public override string Category {
    		get {
    			return "Configure";
    		}
    	}

    	public override Model.NodeOutputSemantics NodeInputType {
    		get {
    			return Model.NodeOutputSemantics.AssetBundleConfigurations;
    		}
    	}

    	public override Model.NodeOutputSemantics NodeOutputType {
    		get {
    			return Model.NodeOutputSemantics.AssetBundleConfigurations;
    		}
    	}

    	public override void Initialize(Model.NodeData data) {
    		m_bundleNameTemplate = "shared_*";
            m_groupedNameTemplate = "needed_grouped_*";
            m_groupExtractedAssets = new SerializableMultiTargetInt();
            m_groupSizeByte = new SerializableMultiTargetInt();
            m_groupingType = new SerializableMultiTargetInt();
    		data.AddDefaultInputPoint();
    		data.AddDefaultOutputPoint();
    	}

    	public override Node Clone(Model.NodeData newData) {
    		var newNode = new ExtractSharedAssets2();
            newNode.m_groupExtractedAssets = new SerializableMultiTargetInt(m_groupExtractedAssets);
            newNode.m_groupSizeByte = new SerializableMultiTargetInt(m_groupSizeByte);
            newNode.m_groupingType = new SerializableMultiTargetInt(m_groupingType);
    		newNode.m_bundleNameTemplate = m_bundleNameTemplate;
            newNode.m_groupedNameTemplate = m_groupedNameTemplate;
            newData.AddDefaultInputPoint();
    		newData.AddDefaultOutputPoint();
    		return newNode;
    	}

    	public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged) {
            /*
            if (m_NeedToGroupedFolderList == null)
            {
                m_NeedToGroupedFolderList = new ReorderableList(this.m_NeedToGroupedFolders, typeof(NeedToGroupedFolder), 
                    true, true, true, true);
                m_NeedToGroupedFolderList.onReorderCallback = list=>
                {
                    this.m_NeedToGroupedFolders.Sort((x, y) =>
                    {
                        int xIndex = m_NeedToGroupedFolders.FindIndex(f => f == x);
                        int yIndex = m_NeedToGroupedFolders.FindIndex(f => f == y);

                        return xIndex - yIndex;
                    });
                };
                m_NeedToGroupedFolderList.onAddCallback = list =>
                {
                    this.m_NeedToGroupedFolders.Add(new NeedToGroupedFolder());
                };
                m_NeedToGroupedFolderList.onRemoveCallback = list =>
                {
                    this.m_NeedToGroupedFolders.RemoveAt(list.index);
                };
                m_NeedToGroupedFolderList.onCanRemoveCallback = list =>
                {
                    int index = list.index;

                    return !(index < 0 || index > m_NeedToGroupedFolders.Count);
                };
                m_NeedToGroupedFolderList.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect,"需要被编组的文件夹路径");
                };
                m_NeedToGroupedFolderList.drawElementCallback = (rect,index,selected, focused) =>
                {
                    bool oldEnabled = GUI.enabled;
                    GUI.enabled = !(index < 0 || index > m_NeedToGroupedFolders.Count);
                    var folder = this.m_NeedToGroupedFolders[index];
                    folder.folder = EditorGUI.TextField(rect, $"Folder_{index}", folder.folder);


                    GUI.enabled = oldEnabled;
                };
            }*/

            if (m_NeedToFilteredFolderList == null)
            {
                m_NeedToFilteredFolderList = new ReorderableList(m_NeedToFilterFolders, typeof(NeedToFilterFolder),
                    true, false, true, true);
                m_NeedToFilteredFolderList.onReorderCallback = list =>
                {
                    this.m_NeedToFilterFolders.Sort((x, y) =>
                    {
                        int xIndex = m_NeedToFilterFolders.FindIndex(f => f == x);
                        int yIndex = m_NeedToFilterFolders.FindIndex(f => f == y);

                        return xIndex - yIndex;
                    });
                }; 
                m_NeedToFilteredFolderList.onAddCallback = list =>
                {
                    this.m_NeedToFilterFolders.Add(new NeedToFilterFolder());
                };
                m_NeedToFilteredFolderList.onRemoveCallback = list =>
                {
                    this.m_NeedToFilterFolders.RemoveAt(list.index);
                };
                m_NeedToFilteredFolderList.onCanRemoveCallback = list =>
                {
                    int index = list.index;
                    return !(index < 0 || index > m_NeedToFilterFolders.Count);
                };
                m_NeedToFilteredFolderList.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "需要被过滤的文件夹路径");
                };
                m_NeedToFilteredFolderList.drawElementCallback = (rect, index, selected, focused) =>
                {
                    bool oldEnabled = GUI.enabled;
                    bool enabled = !(index < 0 || index > m_NeedToFilterFolders.Count);
                    GUI.enabled = enabled;
                    var folder = this.m_NeedToFilterFolders[index];

                    folder.folder = EditorGUI.TextField(rect, $"Folder_{index}", folder.folder);

                    GUI.enabled = oldEnabled;
                };

            }

    		EditorGUILayout.HelpBox("Extract Shared Assets: Extract shared assets between asset bundles and add bundle configurations.", MessageType.Info);
    		editor.UpdateNodeName(node);

    		GUILayout.Space(10f);

    		var newValue = EditorGUILayout.TextField("Bundle Name Template", m_bundleNameTemplate);
    		if(newValue != m_bundleNameTemplate) {
    			using(new RecordUndoScope("Bundle Name Template Change", node, true)) {
    				m_bundleNameTemplate = newValue;
    				onValueChanged();
    			}
    		}

            newValue = EditorGUILayout.TextField("Need Grouped Bundle Name Template", m_groupedNameTemplate);
            if (newValue != m_groupedNameTemplate)
            {
                using (new RecordUndoScope("Need Grouped Bundle Name Template Change", node, true))
                {
                    m_groupedNameTemplate = newValue;
                    onValueChanged();
                }
            }

            GUILayout.Space(10f);

            //Show target configuration tab
            editor.DrawPlatformSelector(node);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                var disabledScope = editor.DrawOverrideTargetToggle(node, m_groupSizeByte.ContainsValueOf(editor.CurrentEditingGroup), (bool enabled) => {
                    using(new RecordUndoScope("Remove Target Grouping Size Settings", node, true)){
                        if(enabled) {
                            m_groupExtractedAssets[editor.CurrentEditingGroup] = m_groupExtractedAssets.DefaultValue;
                            m_groupSizeByte[editor.CurrentEditingGroup] = m_groupSizeByte.DefaultValue;
                            m_groupingType[editor.CurrentEditingGroup] = m_groupingType.DefaultValue;
                        } else {
                            m_groupExtractedAssets.Remove(editor.CurrentEditingGroup);
                            m_groupSizeByte.Remove(editor.CurrentEditingGroup);
                            m_groupingType.Remove(editor.CurrentEditingGroup);
                        }
                        onValueChanged();
                    }
                });

                using (disabledScope) {
                    var useGroup = EditorGUILayout.ToggleLeft ("Subgroup shared assets by size", m_groupExtractedAssets [editor.CurrentEditingGroup] != 0);
                    if (useGroup != (m_groupExtractedAssets [editor.CurrentEditingGroup] != 0)) {
                        using(new RecordUndoScope("Change Grouping Type", node, true)){
                            m_groupExtractedAssets[editor.CurrentEditingGroup] = (useGroup)? 1:0;
                            onValueChanged();
                        }
                    }

                    using (new EditorGUI.DisabledScope (!useGroup)) {
                        var newType = (GroupingType)EditorGUILayout.EnumPopup("Grouping Type",(GroupingType)m_groupingType[editor.CurrentEditingGroup]);
                        if (newType != (GroupingType)m_groupingType[editor.CurrentEditingGroup]) {
                            using(new RecordUndoScope("Change Grouping Type", node, true)){
                                m_groupingType[editor.CurrentEditingGroup] = (int)newType;
                                onValueChanged();
                            }
                        }

                        var newSizeText = EditorGUILayout.TextField("Size(KB)",m_groupSizeByte[editor.CurrentEditingGroup].ToString());
                        int newSize = 0;
                        Int32.TryParse (newSizeText, out newSize);

                        if (newSize != m_groupSizeByte[editor.CurrentEditingGroup]) {
                            using(new RecordUndoScope("Change Grouping Size", node, true)){
                                m_groupSizeByte[editor.CurrentEditingGroup] = newSize;
                                onValueChanged();
                            }
                        }
                    }
                }
            }

    		EditorGUILayout.HelpBox("Bundle Name Template replaces \'*\' with number.", MessageType.Info);

            //m_NeedToGroupedFolderList.DoLayoutList();
            GUILayout.Space(10);
            m_NeedToFilteredFolderList.DoLayoutList();
        }



        /**
    	 * Prepare is called whenever graph needs update. 
    	 */
        public override void Prepare (BuildTarget target, 
    		Model.NodeData node, 
    		IEnumerable<PerformGraph.AssetGroups> incoming, 
    		IEnumerable<Model.ConnectionData> connectionsToOutput, 
    		PerformGraph.Output Output) 
    	{
    		if(string.IsNullOrEmpty(m_bundleNameTemplate)) {
    			throw new NodeException("Bundle Name Template is empty.", "Set valid bundle name template.",node);
    		}
            if (m_groupExtractedAssets [target] != 0) {
                if(m_groupSizeByte[target] < 0) {
                    throw new NodeException("Invalid size. Size property must be a positive number.", "Set valid size.", node);
                }
            }

    		// Pass incoming assets straight to Output
    		if(Output != null) {
    			var destination = (connectionsToOutput == null || !connectionsToOutput.Any())? 
    				null : connectionsToOutput.First();

    			if(incoming != null) {

                    var buildMap = AssetBundleBuildMap.GetBuildMap ();
                    buildMap.ClearFromId (node.Id);

                    var dependencyCollector = new Dictionary<string, List<string>>(); // [asset path:group name]
    				var sharedDependency = new Dictionary<string, List<AssetReference>>();
    				var groupNameMap = new Dictionary<string, string>();
                    HashSet<string> allMainAssets = new HashSet<string>();
                    

                    // build dependency map
                    foreach (var ag in incoming) {
    					foreach (var key in ag.assetGroups.Keys) {
    						var assets = ag.assetGroups[key];
                            
                            foreach (var a in assets) {
                                if (!allMainAssets.Contains(a.importFrom))
                                {
                                    allMainAssets.Add(a.importFrom);
                                }

                                bool needCollectAssets = true;
                                if (this.m_NeedToFilterFolders != null && this.m_NeedToFilterFolders.Count > 0)
                                {
                                    foreach (var folder in m_NeedToFilterFolders)
                                    {
                                        if (!string.IsNullOrEmpty(folder.folder))
                                        {
                                            if (a.importFrom.Contains(folder.folder))
                                            {
                                                needCollectAssets = false;
                                            }
                                        }
                                    }
                                }

                                if (needCollectAssets)
                                {
                                    CollectDependencies(key, new string[] { a.importFrom }, ref dependencyCollector);
                                }
                            }
    					}
    				}

                    HashSet<string> needRemovedSet = new HashSet<string>();
                    foreach (var entry in dependencyCollector)
                    {
                        if (allMainAssets.Contains(entry.Key))
                        {
                            needRemovedSet.Add(entry.Key);
                        }
                    }

                    foreach (var assetPath in needRemovedSet)
                    {
                        dependencyCollector.Remove(assetPath);
                    }

                    //Dictionary<string, List<string>> nGroupedAssets = new Dictionary<string, List<string>>();
                    //if (!string.IsNullOrEmpty(this.m_groupedNameTemplate))
                    //{
                    //    foreach (var entry in dependencyCollector)
                    //    {
                    //        if (entry.Value != null && entry.Value.Count > 1)
                    //        {
                    //            bool contains = false;
                    //            foreach (var folder in (this.m_NeedToGroupedFolders))
                    //            {
                    //                if (!string.IsNullOrEmpty(folder.folder) && entry.Key.Contains(folder.folder))
                    //                {
                    //                    if (!nGroupedAssets.ContainsKey(folder.folder))
                    //                    {
                    //                        nGroupedAssets.Add(folder.folder,new List<string>());
                    //                    }
                    //                    nGroupedAssets[folder.folder].Add(entry.Key);
                    //                }
                    //            }
                    //        }
                    //    }

                    //    int nCounter = 0;
                    //    foreach (var kvp in nGroupedAssets)
                    //    {
                    //        var assets = kvp.Value;
                    //        var nGroupedAbName = m_groupedNameTemplate.Replace("*", nCounter.ToString());
                    //        foreach (var asset in assets)
                    //        {
                    //            dependencyCollector.Remove(asset);
                    //            if (!sharedDependency.ContainsKey(nGroupedAbName))
                    //            {
                    //                sharedDependency[nGroupedAbName] = new List<AssetReference>();
                    //            }

                    //            sharedDependency[nGroupedAbName].Add(AssetReference.CreateReference(asset));
                    //        }
                    //        nCounter++;
                    //    }

                    //}

                    foreach (var entry in dependencyCollector) {
    					if(entry.Value != null && entry.Value.Count > 1) {
                            //sw.WriteLine(entry.Key);
                            var joinedName = string.Join("-", entry.Value.ToArray());
    						if(!groupNameMap.ContainsKey(joinedName)) {
    							var count = groupNameMap.Count;
    							var newName = m_bundleNameTemplate.Replace("*", count.ToString());
    							if(newName == m_bundleNameTemplate) {
    								newName = m_bundleNameTemplate + count.ToString();
    							}
    							groupNameMap.Add(joinedName, newName);
    						}
    						var groupName = groupNameMap[joinedName];

    						if(!sharedDependency.ContainsKey(groupName)) {
    							sharedDependency[groupName] = new List<AssetReference>();
    						}
    						sharedDependency[groupName].Add( AssetReference.CreateReference(entry.Key) );
    					}
    				}
                   
                    if (sharedDependency.Keys.Count > 0) {
                        // subgroup shared dependency bundles by size
                        if (m_groupExtractedAssets [target] != 0) {
                            List<string> devidingBundleNames = new List<string> (sharedDependency.Keys);
                            long szGroup = m_groupSizeByte[target] * 1000;

                            foreach(var bundleName in devidingBundleNames) {
                                var assets = sharedDependency[bundleName];
                                int groupCount = 0;
                                long szGroupCount = 0;
                                foreach(var a in assets) {
                                    var subGroupName = $"{bundleName}_{groupCount}";
                                    if (!sharedDependency.ContainsKey(subGroupName)) {
                                        sharedDependency[subGroupName] = new List<AssetReference>();
                                    }
                                    sharedDependency[subGroupName].Add(a);

                                    szGroupCount += GetSizeOfAsset(a, (GroupingType)m_groupingType[target]);
                                    if(szGroupCount >= szGroup) {
                                        szGroupCount = 0;
                                        ++groupCount;
                                    }
                                }
                                sharedDependency.Remove (bundleName);
                            }
                        }

                        foreach (var bundleName in sharedDependency.Keys) {
                            var bundleConfig = buildMap.GetAssetBundleWithNameAndVariant (node.Id, bundleName, string.Empty);
                            bundleConfig.AddAssets (node.Id, sharedDependency[bundleName].Select(a => a.importFrom));
                        }

    					foreach(var ag in incoming) {
    						Output(destination, new Dictionary<string, List<AssetReference>>(ag.assetGroups));
    					}
    					Output(destination, sharedDependency);
    				} else {
    					foreach(var ag in incoming) {
    						Output(destination, ag.assetGroups);
    					}
    				}

    			} else {
    				// Overwrite output with empty Dictionary when no there is incoming asset
    				Output(destination, new Dictionary<string, List<AssetReference>>());
    			}
    		}
    	}

    	private void CollectDependencies(string groupKey, string[] assetPaths, ref Dictionary<string, List<string>> collector) {
    		var dependencies = AssetDatabase.GetDependencies(assetPaths);
    		foreach(var d in dependencies) {
                // AssetBundle must not include script asset
                if (TypeUtility.GetMainAssetTypeAtPath (d) == typeof(MonoScript)) {
                    continue;
                }

                if (TypeUtility.GetMainAssetTypeAtPath(d) == typeof(SceneAsset))
                {
                    continue;
                }

                if (!collector.ContainsKey(d)) {
    				collector[d] = new List<string>();
    			}
    			if(!collector[d].Contains(groupKey)) {
    				collector[d].Add(groupKey);
    				collector[d].Sort();
    			}
    		}
    	}

        private long GetSizeOfAsset(AssetReference a, GroupingType t) {

            long size = 0;

            // You can not read scene and do estimate
			if (a.isSceneAsset) {
                t = GroupingType.ByFileSize;
            }

            if (t == GroupingType.ByRuntimeMemorySize) {
                var objects = a.allData;
                foreach (var o in objects) {
                    size += Profiler.GetRuntimeMemorySizeLong (o);
                }

                a.ReleaseData ();
            } else if (t == GroupingType.ByFileSize) {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(a.absolutePath);
                if (fileInfo.Exists) {
                    size = fileInfo.Length;
                }
            }

            return size;
        }
    }
}