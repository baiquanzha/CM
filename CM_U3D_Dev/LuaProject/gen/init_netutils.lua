-- Code generated by protokitgo. DO NOT EDIT.

netutils = netutils or {}
function netutils:initAll()
    require(GEN_PACKAGE_NAME .. ".netutils.cmds_pb")
    require(GEN_PACKAGE_NAME .. ".netutils.common_pb")
    require(GEN_PACKAGE_NAME .. ".netutils.packet_pb")
end
