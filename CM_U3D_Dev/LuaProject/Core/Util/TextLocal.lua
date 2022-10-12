--文本本地化工具
EnumLanguage = {
    En = 1,
}

TextLocal = {
    textInfoDic = nil,

    language = EnumLanguage.En,
    languageName = "en",
}

local languageNames = {"en"}

local textInfo

function TextLocal.Init()
    TextLocal.textInfoDic = Config.GetConfigs("I18NConf")
    TextLocal.SetLanguage(EnumLanguage.En)
end

function TextLocal.SetLanguage(_language)
    TextLocal.language = _language
    TextLocal.languageName = languageNames[_language]
end

function TextLocal.GetText(key)
    textInfo = TextLocal.textInfoDic[key]
    return textInfo[TextLocal.languageName]
end