--此文件加载会按照顺序加载 添加时需注意
_G.GetProject = function()
	return {
        {
            name = "Core",
            {
                --全局定义文件
                name = "Base",
                "IDictionary.lua",
                "IList.lua",
                "HashTable.lua",
                "Vector3f.lua",
                "EncryptInt.lua",
            },
            {
                --全局定义文件
                name = "Data",
                "BaseExtend.lua",
                "UnityMapping.lua",
            },
            {
                --游戏相关类
                name = "Game",
                {
                    {
                        --事件类
                        name = "Event",
                        "EventMgr.lua",
                        "Event.lua",
                    },
                    {
                        --时间相关类
                        name = "Time",
                        "Timer.lua",
                        "TimerMgr.lua",
                    },
                    {
                        --刷新类
                        name = "Update",
                        "MainUpdate.lua",
                    },
                    {
                        --场景类
                        name = "Scene",
                        "SceneBase.lua",
                        "SceneMgr.lua",
                    },
                },
            },
            {
                --UI
                name = "UI",
                "UIMgr.lua",
                {
                    name = "Base",
                    "UIBase.lua",
                    "UIControlBase.lua",
                    "UIWindowBase.lua",
                    "UIControlEventBind.lua",
                },
                {
                    name = "UIControl",
                    "UIGameObject.lua",
                    "UIButton.lua",
                    "UIImage.lua",
                    "UIText.lua",
                    "UILayoutGroup.lua",
                    "UITab.lua",
                    "UITabGroup.lua",
                    "UICountDownTimerBase.lua",
                    "UICountDownTimer.lua",
                    "UIAnimation.lua",
                    "UISlider.lua",
                    "UICanvasGroup.lua",
                    "UIInputField.lua",
                    "UIGraphic.lua",
                    "UICanvas.lua",
                    "UILoopScrollRect.lua",
                },
            },
            {
                --模拟类 基类
                name = "Class",
                "BaseCls.lua",
            },
        },
	}
end