using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTool.AppBuilder.Editor
{
    public class EnvironmentVariables
    {
        /// <summary>
        /// 上传操作使用的服务器名
        /// </summary>
        public const string SERVER_INFO_NAME_KEY = "SERVER_INFO_NAME";

        /// <summary>
        /// 资源平台
        /// </summary>
        public const string PLATFORM_KEY = "PLATFORM";

        /// <summary>
        /// 资源的源目录
        /// </summary>
        public const string SOURCE_DIR_KEY = "SOURCE_DIR";

        /// <summary>
        /// 资源的版本
        /// </summary>
        public const string ASSET_VERSION_KEY = "ASSET_VERSION";

        /// <summary>
        /// 需要上传的文件列表路径
        /// </summary>
        public const string NEEDED_UPLOAD_LIST_NAME_KEY = "NEEDED_UPLOAD_LIST_NAME";


        /// <summary>
        /// 制作基础App
        /// </summary>
        public const string MAKE_BASE_APP_VERSION_KEY = "MAKE_BASE_APP_VERSION";

        /// <summary>
        /// Lua assetbundle 路径
        /// </summary>
        public const string LUA_AB_PATH_KEY = "LUA_AB_PATH";

        /// <summary>
        /// 游戏表数据仓库分支名
        /// </summary>
        public const string GAME_TABLE_DATA_REPOSITORY_BRANCH_NAME = "GAME_TABLE_DATA_REPOSITORY_BRANCH_NAME";

    }
}
