local acos	= math.acos
local sqrt 	= math.sqrt
local max 	= math.max
local min 	= math.min
local cos	= math.cos
local sin	= math.sin
local abs	= math.abs
local rawset = rawset
local rawget = rawget

local rad2Deg = CS.UnityEngine.Mathf.Rad2Deg
local deg2Rad = CS.UnityEngine.Mathf.Deg2Rad

Vector3f = 
{	
	class = "Vector3f",
}

local fields = {}

setmetatable(Vector3f, Vector3f)

Vector3f.__index = function(t,k)
	local var = rawget(Vector3f, k)
	
	if var == nil then							
		var = rawget(fields, k)
		
		if var ~= nil then
			return var(t)				
		end		
	end
	
	return var
end

Vector3f.__call = function(t,x,y,z)
	return Vector3f.New(x,y,z)
end

function Vector3f.New(x, y, z)
	local v = {x = x or 0, y = y or 0, z = z or 0}		
	setmetatable(v, Vector3f)		
	return v
end
	
function Vector3f:Set(x,y,z)	
	self.x = x or 0
	self.y = y or 0
	self.z = z or 0
end

function Vector3f:SetV3f(pos)	
	self.x = pos.x
	self.y = pos.y
	self.z = pos.z
    return self;
end

function Vector3f:Get()	
	return self.x, self.y, self.z	
end

function Vector3f:GetV3()
	return Vector3(self.x, self.y, self.z);
end

function Vector3f:Clone()
	return Vector3f.New(self.x, self.y, self.z)
end

function Vector3f.Distance(va, vb)
	return sqrt((va.x - vb.x)^2 + (va.y - vb.y)^2 + (va.z - vb.z)^2)
end

function Vector3f.Distance2(va, x, y, z)
	return sqrt((va.x - x)^2 + (va.y - y)^2 + (va.z - z)^2)
end

--比较距离
local tempDis
function Vector3f.CompareDistance(va, vb, dis)
    tempDis = (va.x - vb.x)^2 + (va.y - vb.y)^2 + (va.z - vb.z)^2
    if tempDis > dis * dis then
        return 1;
    elseif tempDis < dis * dis then
        return -1;
    else
        return 0;
    end
end

function Vector3f.Dot(lhs, rhs)
	return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z))
end

function Vector3f.Lerp(from, to, t)	
	t = clamp(t, 0, 1)
	return Vector3f.New(from.x + ((to.x - from.x) * t), from.y + ((to.y - from.y) * t), from.z + ((to.z - from.z) * t))
end

function Vector3f:Magnitude()
	return sqrt(self.x * self.x + self.y * self.y + self.z * self.z)
end

function Vector3f.Max(lhs, rhs)
	return Vector3f.New(max(lhs.x, rhs.x), max(lhs.y, rhs.y), max(lhs.z, rhs.z))
end

function Vector3f.Min(lhs, rhs)
	return Vector3f.New(min(lhs.x, rhs.x), min(lhs.y, rhs.y), min(lhs.z, rhs.z))
end

function Vector3f:Normalize()
	local v = self:Clone()
	return v:SetNormalize()
end

function Vector3f:SetNormalize()
	local num = self:Magnitude()	
	
	if num == 1 then
		return self
    elseif num > 1e-5 then    
        self:Div(num)
    else    
		self:Set(0,0,0)
	end 

	return self
end
	
function Vector3f:SqrMagnitude()
	return self.x * self.x + self.y * self.y + self.z * self.z
end

local dot = Vector3f.Dot
local radian;
function Vector3f.Angle(from, to)
    radian = dot(from.normalized, to.normalized);
    if radian < -1 then
        radian = -1;
    elseif radian > 1 then
        radian = 1;
    end
	return acos(radian) * rad2Deg
end

function Vector3f:ClampMagnitude(maxLength)	
	if self:SqrMagnitude() > (maxLength * maxLength) then    
		self:SetNormalize()
		self:Mul(maxLength)        
    end
	
    return self
end


function Vector3f.OrthoNormalize(va, vb, vc)	
	va:SetNormalize()
	vb:Sub(vb:Project(va))
	vb:SetNormalize()
	
	if vc == nil then
		return va, vb
	end
	
	vc:Sub(vc:Project(va))
	vc:Sub(vc:Project(vb))
	vc:SetNormalize()		
	return va, vb, vc
end

function Vector3f.RotateTowards2(from, to, maxRadiansDelta, maxMagnitudeDelta)	
	local v2 	= to:Clone()
	local v1 	= from:Clone()
	local len2 	= to:Magnitude()
	local len1 	= from:Magnitude()	
	v2:Div(len2)
	v1:Div(len1)
	
	local dota	= dot(v1, v2)
	local angle = acos(dota)			
	local theta = min(angle, maxRadiansDelta)	
	local len	= 0
	
	if len1 < len2 then
		len = min(len2, len1 + maxMagnitudeDelta)
	elseif len1 == len2 then
		len = len1
	else
		len = max(len2, len1 - maxMagnitudeDelta)
	end
						    
    v2:Sub(v1 * dota)
    v2:SetNormalize()     
	v2:Mul(sin(theta))
	v1:Mul(cos(theta))
	v2:Add(v1)
	v2:SetNormalize()
	v2:Mul(len)
	return v2	
end

function Vector3f.RotateTowards1(from, to, maxRadiansDelta, maxMagnitudeDelta)	
	local omega, sinom, scale0, scale1, len, theta
	local v2 	= to:Clone()
	local v1 	= from:Clone()
	local len2 	= to:Magnitude()
	local len1 	= from:Magnitude()	
	v2:Div(len2)
	v1:Div(len1)
	
	local cosom = dot(v1, v2)
	
	if len1 < len2 then
		len = min(len2, len1 + maxMagnitudeDelta)	
	elseif len1 == len2 then
		len = len1
	else
		len = max(len2, len1 - maxMagnitudeDelta)
	end 	
	
	if 1 - cosom > 1e-6 then	
		omega 	= acos(cosom)
		theta 	= min(omega, maxRadiansDelta)		
		sinom 	= sin(omega)
		scale0 	= sin(omega - theta) / sinom
		scale1 	= sin(theta) / sinom
		
		v1:Mul(scale0)
		v2:Mul(scale1)
		v2:Add(v1)
		v2:Mul(len)
		return v2
	else 		
		v1:Mul(len)
		return v1
	end			
end
	
function Vector3f.MoveTowards(current, target, maxDistanceDelta)
	local delta = target - current	
    local sqrDelta = delta:SqrMagnitude()
	local sqrDistance = maxDistanceDelta * maxDistanceDelta
	
    if sqrDelta > sqrDistance then    
		local magnitude = sqrt(sqrDelta)
		
		if magnitude > 1e-6 then
			delta:Mul(maxDistanceDelta / magnitude)
			delta:Add(current)
			return delta
		else
			return current:Clone()
		end
    end
	
    return target:Clone()
end

function ClampedMove(lhs, rhs, clampedDelta)
	local delta = rhs - lhs
	
	if delta > 0 then
		return lhs + min(delta, clampedDelta)
	else
		return lhs - min(-delta, clampedDelta)
	end
end

local overSqrt2 = 0.7071067811865475244008443621048490

local function OrthoNormalVector(vec)
	local res = Vector3f.New()
	
	if abs(vec.z) > overSqrt2 then			
		local a = vec.y * vec.y + vec.z * vec.z
		local k = 1 / sqrt (a)
		res.x = 0
		res.y = -vec.z * k
		res.z = vec.y * k
	else			
		local a = vec.x * vec.x + vec.y * vec.y
		local k = 1 / sqrt (a)
		res.x = -vec.y * k
		res.y = vec.x * k
		res.z = 0
	end
	
	return res
end

function Vector3f.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta)
	local len1 = current:Magnitude()
	local len2 = target:Magnitude()
	
	if len1 > 1e-6 and len2 > 1e-6 then	
		local from = current / len1
		local to = target / len2		
		local cosom = dot(from, to)
				
		if cosom > 1 - 1e-6 then		
			return Vector3f.MoveTowards (current, target, maxMagnitudeDelta)		
		elseif cosom < -1 + 1e-6 then		
			local axis = OrthoNormalVector(from)						
			local q = Quaternion.AngleAxis(maxRadiansDelta * rad2Deg, axis)	
			local rotated = q:MulVec3(from)
			local delta = ClampedMove(len1, len2, maxMagnitudeDelta)
			rotated:Mul(delta)
			return rotated
		else		
			local angle = acos(cosom)
			local axis = Vector3f.Cross(from, to)
			axis:SetNormalize ()
			local q = Quaternion.AngleAxis(min(maxRadiansDelta, angle) * rad2Deg, axis)			
			local rotated = q:MulVec3(from)
			local delta = ClampedMove(len1, len2, maxMagnitudeDelta)
			rotated:Mul(delta)
			return rotated
		end
	end
		
	return Vector3f.MoveTowards(current, target, maxMagnitudeDelta)
end
	
function Vector3f.SmoothDamp(current, target, currentVelocity, smoothTime)
	local maxSpeed = math.huge
	local deltaTime = Time.deltaTime
    smoothTime = max(0.0001, smoothTime)
    local num = 2 / smoothTime
    local num2 = num * deltaTime
    local num3 = 1 / (((1 + num2) + ((0.48 * num2) * num2)) + (((0.235 * num2) * num2) * num2))    
    local vector2 = target:Clone()
    local maxLength = maxSpeed * smoothTime
	local vector = current - target
    vector:ClampMagnitude(maxLength)
    target = current - vector
    local vec3 = (currentVelocity + (vector * num)) * deltaTime
    currentVelocity = (currentVelocity - (vec3 * num)) * num3
    local vector4 = target + (vector + vec3) * num3	
	
    if Vector3f.Dot(vector2 - current, vector4 - vector2) > 0 then    
        vector4 = vector2
        currentVelocity:Set(0,0,0)
    end
	
    return vector4, currentVelocity
end	
	
function Vector3f.Scale(a, b)
	local v = a:Clone()
	return v:SetScale(b)
end

function Vector3f:SetScale(b)
	self.x = self.x * b.x
	self.y = self.y * b.y
	self.z = self.z * b.z	
	return self
end
	
function Vector3f.Cross(lhs, rhs)
	local x = lhs.y * rhs.z - lhs.z * rhs.y
	local y = lhs.z * rhs.x - lhs.x * rhs.z
	local z = lhs.x * rhs.y - lhs.y * rhs.x
	return Vector3f.New(x,y,z)	
end
	
function Vector3f:Equals(other)
	return self.x == other.x and self.y == other.y and self.z == other.z
end
		
function Vector3f.Reflect(inDirection, inNormal)
	local num = -2 * dot(inNormal, inDirection)
	inNormal = inNormal * num
	inNormal:Add(inDirection)
	return inNormal
end

	
function Vector3f.Project(vector, onNormal)
	local num = onNormal:SqrMagnitude()
	
	if num < 1.175494e-38 then	
		return Vector3f.New(0,0,0)
	end
	
	local num2 = dot(vector, onNormal)
	local v3 = onNormal:Clone()
	v3:Mul(num2/num)	
	return v3
end
	
function Vector3f.ProjectOnPlane(vector, planeNormal)
	local v3 = Vector3f.Project(vector, planeNormal)
	v3:Mul(-1)
	v3:Add(vector)
	return v3
end		

function Vector3f.Slerp2(from, to, t)		
	if t <= 0 then
		return from:Clone()
	elseif t >= 1 then
		return to:Clone()
	end
	
	local v2 	= to:Clone()
	local v1 	= from:Clone()
	local len2 	= to:Magnitude()
	local len1 	= from:Magnitude()	
	v2:Div(len2)
	v1:Div(len1)
	
	local omega = dot(v1, v2) 	
	local len 	= (len2 - len1) * t + len1    		
    local theta = acos(omega) * t
	
    v2:Sub(v1 * omega)
    v2:SetNormalize()     
	v2:Mul(sin(theta))
	v1:Mul(cos(theta))
	v2:Add(v1)
	v2:SetNormalize()
	v2:Mul(len)
    return v2	
end

function Vector3f.Slerp(from, to, t)
	local omega, sinom, scale0, scale1

	if t <= 0 then		
		return from:Clone()
	elseif t >= 1 then		
		return to:Clone()
	end
	
	local v2 	= to:Clone()
	local v1 	= from:Clone()
	local len2 	= to:Magnitude()
	local len1 	= from:Magnitude()	
	v2:Div(len2)
	v1:Div(len1)

	local len 	= (len2 - len1) * t + len1
	local cosom = dot(v1, v2)
	
	if 1 - cosom > 1e-6 then
		omega 	= acos(cosom)
		sinom 	= sin(omega)
		scale0 	= sin((1 - t) * omega) / sinom
		scale1 	= sin(t * omega) / sinom
	else 
		scale0 = 1 - t
		scale1 = t
	end

	v1:Mul(scale0)
	v2:Mul(scale1)
	v2:Add(v1)
	v2:Mul(len)
	return v2
end


function Vector3f:Mul(q)
	if type(q) == "number" then
		self.x = self.x * q
		self.y = self.y * q
		self.z = self.z * q
	else
		self:MulQuat(q)
	end
	
	return self
end

function Vector3f:Div(d)
	self.x = self.x / d
	self.y = self.y / d
	self.z = self.z / d
	
	return self
end

function Vector3f:Add(vb)
	self.x = self.x + vb.x
	self.y = self.y + vb.y
	self.z = self.z + vb.z
	
	return self
end

function Vector3f:AddPos(x, y, z)
	self.x = self.x + x
	self.y = self.y + y
	self.z = self.z + z
	return self
end

function Vector3f:Sub(vb)
	self.x = self.x - vb.x
	self.y = self.y - vb.y
	self.z = self.z - vb.z
	
	return self
end

function Vector3f:SubPos(x, y, z)
	self.x = self.x - x
	self.y = self.y - y
	self.z = self.z - z
	
	return self
end

function Vector3f:MulQuat(quat)	   
	local num 	= quat.x * 2
	local num2 	= quat.y * 2
	local num3 	= quat.z * 2
	local num4 	= quat.x * num
	local num5 	= quat.y * num2
	local num6 	= quat.z * num3
	local num7 	= quat.x * num2
	local num8 	= quat.x * num3
	local num9 	= quat.y * num3
	local num10 = quat.w * num
	local num11 = quat.w * num2
	local num12 = quat.w * num3
	
	local x = (((1 - (num5 + num6)) * self.x) + ((num7 - num12) * self.y)) + ((num8 + num11) * self.z)
	local y = (((num7 + num12) * self.x) + ((1 - (num4 + num6)) * self.y)) + ((num9 - num10) * self.z)
	local z = (((num8 - num11) * self.x) + ((num9 + num10) * self.y)) + ((1 - (num4 + num5)) * self.z)
	
	self:Set(x, y, z)	
	return self
end

function Vector3f.AngleAroundAxis (from, to, axis)	 	 
	from = from - Vector3f.Project(from, axis)
	to = to - Vector3f.Project(to, axis) 	    
	local angle = Vector3f.Angle (from, to)	   	    
	return angle * (Vector3f.Dot (axis, Vector3f.Cross (from, to)) < 0 and -1 or 1)
end


Vector3f.__tostring = function(self)
	return "["..self.x..","..self.y..","..self.z.."]"
end

Vector3f.__div = function(va, d)
	return Vector3f.New(va.x / d, va.y / d, va.z / d)
end

Vector3f.__mul = function(va, d)
	if type(d) == "number" then
		return Vector3f.New(va.x * d, va.y * d, va.z * d)
	else
		local vec = va:Clone()
		vec:MulQuat(d)
		return vec
	end	
end

Vector3f.__add = function(va, vb)
	return Vector3f.New(va.x + vb.x, va.y + vb.y, va.z + vb.z)
end

Vector3f.__sub = function(va, vb)
	return Vector3f.New(va.x - vb.x, va.y - vb.y, va.z - vb.z)
end

Vector3f.__unm = function(va)
	return Vector3f.New(-va.x, -va.y, -va.z)
end

Vector3f.__eq = function(a,b)
	local v = a - b
	local delta = v:SqrMagnitude()
	return delta < 1e-10
end

fields.up 		= function() return Vector3f.New(0,1,0) end
fields.down 	= function() return Vector3f.New(0,-1,0) end
fields.right	= function() return Vector3f.New(1,0,0) end
fields.left		= function() return Vector3f.New(-1,0,0) end
fields.forward 	= function() return Vector3f.New(0,0,1) end
fields.back		= function() return Vector3f.New(0,0,-1) end
fields.zero		= function() return Vector3f.New(0,0,0) end
fields.one		= function() return Vector3f.New(1,1,1) end
fields.magnitude 	= Vector3f.Magnitude
fields.normalized 	= Vector3f.Normalize
fields.sqrMagnitude = Vector3f.SqrMagnitude
