# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.6.0] - 2019-09-14
### Changed
- Added Animation Import Overwrite Options to let user decide or skip overwriting AnimationClip Settings and Human Descriptions. (#104)
- Dependent package version update: Addressables 1.1.7->1.2.3, Asset Bundle Browser 1.6.0-> 1.7.0

### Fixed
- Fixed EditorTest compile errors (#106)
- Fixed Issue Last Imported Items produces exception (#100)

## [1.5.0] - 2018-07-31
*This is the first version of AssetGraph in the package form.*

### Changed
- Software License now changed to Unity Companion License from MIT License.
- API namespace changed from UnityEngine.AssetGraph to Unity.AssetGraph.
- Default AssetBundleBuildMap path changed. It can be configured from Project Settings window per project.
- Stopped to try importing old data automatically. Instead, menu for importing Old version (v1.1) data is now always visible from AssetGraph Window.
- Added 2018.3 support.
- Added 2019.2 support.

### Fixed
- Fixed issue that error icon not displayed properly on node when node has an error. (2018.1)

