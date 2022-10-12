require "Core/Util/TextLocal"
require "Core/Util/TextUtil"
require "Core/Util/JsonUtil"
require "Core/Util/LGameUtil"
require "Core/Util/LGameAdapt"
require "Core/Spine/LSpineMeshMgr"

ClientVersion = 2;  --客户端版本
ETrue = 1
EFalse = 0

--通用颜色
COLOR_WHITE = {1, 1, 1, 1}
COLOR_WHITE_HALF_A = {1, 1, 1, 0.5}
COLOR_WHITE_NO_A = {1, 1, 1, 0}
COLOR_GRAY = {0.5, 0.5, 0.5, 1}
COLOR_BLACK_NO_A = {0, 0, 0, 0}
COLOR_BLACK_HALF_A = {0, 0, 0, 0.5}
COLOR_RED_OUTLINE = {0.6745098, 0.1254902, 0}
COLOR_GREEN_OUTLINE = {0.2941177, 0.6078432, 0.07843138}
COLOR_BLUE_OUTLINE = {0.1333333, 0.4666667, 0.654902}
COLOR_GRAY_OUTLINE = {0.4705882, 0.4392157, 0.509804}