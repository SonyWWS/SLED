-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved. 

NUM_DOTS = 2000

TestFormatString = "http://www.blah.com/%s/%i/%u/index.html"

-- Test global tables within tables
a = {}
a[1] = 1
a[2] = 2
a[3] = 3
a[4] = 4
a[5] = 5
a[6] = 6
a[7] = 7
a[8] = 8
a[9] = 9
a[10] = 10
a[11] = 11
a[12] = 12
a["testFormatStr"] = "Hello %s %i %u Blah"
a["password"] = "pluto"
a["foo.bar"] = "baz"
a["foo.baz"] = {}
a["foo.baz"][1] = 23
a["foo.baz"]["foo.bar"] = 24
a["xkey"] = {}
a["xkey"]["1"] = 11
a["xkey"]["2"] = "killer whale"
a["xkey"]["3"] = true
a["xkey"]["4"] = {}
a["xkey"]["4"][1] = 1
a["xkey"]["4"][2] = 2
a["xkey"]["4"][3] = "three"
a["xkey"]["4"][4] = {}
a["xkey"]["4"][4][1] = 1
a["xkey"]["4"][4][2] = 2
a["xkey"]["4"][4][3] = {}
a["xkey"]["4"][4][3][1] = 1
a[5] = {}
a[5][1] = {}
a[5][1][1] = {}
a[5][1][2] = "gore"
a[5][1][5] = "whale"
a[5][1][1][1] = "ratchet"
a[5][1][1][2] = "bob"
a[5][1][1][3] = 12
a[5][1][11] = 11
a[5][3] = 7

b = {}
b[1] = {}
b[1][1] = {}
b[1][1][1] = {}
b[1][1][1][1] = 1
b[2] = 2

-- To test how StateMachine creates certain Lua functions
-- and to verify SLED's function parsing works on them
Action = {}
function Action:Initialize()
	self.Action =
	{ 
		ReloadGun = function() return self:ReloadGun() end,
		DoSomething = function() return self:DoSomething() end
	}
end
function Action:ReloadGun() end
function Action:DoSomething() end

-- 'classic' object oriented Lua approach
Object = {}
function Object:new(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

-- Create new table 'derived' from Object
TestOO = Object:new{ some_val = 1, another_val = 2, some_string = "Drake" }
function TestOO:Times(a, b, c)
	local var = (self.some_val * a) + (self.another_val * b) + self:DoTimes(a, b, c)
	return var
end
function TestOO:DoTimes(a, b, c)
	return a * b * c
end

-- Create 'instance' of TestOO table
local oo_test = TestOO:new{}

--
-- 日本語のコメント / Japanese comments
--
-- Test table & nested table w/ Japanese characters
langTest = {}
langTest["一"] = 5
langTest["二"] = "日本語のテキスト"
langTest["三"] = true
langTest["四"] = {}
langTest["四"]["東京"] = 3.14159
langTest["四"]["大阪"] = "お好み焼き"
langTest["四"]["名古屋"] = false

-- Here to test coroutines
function thread1(a, b)
	local plus = plus(a, b)
	coroutine.yield()
	
	local minus = minus(a, b)
	coroutine.yield()
	
	return plus * minus
end

function plus(a, b)
	local c = a + b
	return c
end

function minus(a, b)
	local c = a - b
	return c
end

function doAddAndMinus(a, b)
	local addMinusResult = 0
	
	local plusResult = 0
	plusResult = plus(a, b)
	
	local minusResult = 0
	minusResult = minus(a, b)
	
	addMinusResult = plus(minus(a, b), plus(a, b))
	
	return addMinusResult
end

function times(a, b, c)
	local test = a
	local test2 = b
	local test3 = c	
	local sum = 0
	
	local vars = { a, b, c }
	vars[4] = { "daxter", "clank", 3.14159 }
	
	local oo_test_result = oo_test:Times(a, b, c)
	
	local testAssert = false
	local testTTY = false
	
	-- Test assert function (can change testAssert while debugging to reduce spam).
	if testAssert then
		-- Do something that causes the assert to fail. This will force execution to halt.
		libsledluaplugin.assert((testAssert == false), "testAssert != true")
	end
	
	-- Test TTY function (can change testPrint while debugging to reduce spam).
	if testTTY then
		libsledluaplugin.tty("Hello world!")
		libsledluaplugin.tty("Hello", " world", "!")
		libsledluaplugin.tty("Hello" .. " world" .. "!")
	end
	
	--return 50
	----[[

	-- Create a coroutine
	global_co = coroutine.create( thread1 )
	-- Start it up and pass in some values
	coroutine.resume( global_co, vars[1], vars[2] )

	for i = 0, 10 do
		sum = sum + i
	end
	
	-- Test local table within table
	local array = {}
	array[1] = {}
	array[2] = 2	
	array[3] = "three"	
	array[1][4] = "four"
	array[1][5] = true
	
	-- Do a little more work...
	coroutine.resume(global_co)
		
	local x = doAddAndMinus(a, b)
	
	-- The coroutine will return true/false from
	-- the resume call and since this is the last
	-- resume that makes the coroutine function
	-- body end it will return the function body
	-- result too, so we'll catch both with these
	-- two variables
	local co_end_status
	local co_end_retval
	
	-- Do the final work & get results	
	co_end_status, co_end_retval = coroutine.resume( global_co )
	
	-- Set a breakpoint on the next line and see the value
	-- of co_end_status & co_end_retval...	
	test = test2 - test3
	
	-- Test a local table & nested table w/ Japanese characters
	local charTest = {}
	charTest["一"] = 5
	charTest["二"] = "日本語のテキスト"
	charTest["三"] = true
	charTest["四"] = {}
	charTest["四"]["東京"] = 3.14159
	charTest["四"]["大阪"] = "お好み焼き"
	charTest["四"]["名古屋"] = false
	
	-- Call C functions
	local anotherTemp = 5
	if (CFunc1() == "CFunc1" and CFunc2() == "CFunc2") then
		anotherTemp = anotherTemp + 5
	end
	
	test2 = test * test3
	
	local locFunc
	locFunc = function(a)
		return test2 * a
	end
	
	local locFuncValue = locFunc(test3)
			
	j = function(a)
		a = test2 - test3 + test
				
		-- Test local table within table
		local array2 = {}
		array2[1] = {}
		array2[2] = 2
		array2[3] = "three"
		array2[1][4] = "four"
		array2[1][5] = true
		
		local y = doAddAndMinus(a, test2)
		
		a = a + array2[2] + vars[1]
		
		return a
	end

	return NUM_DOTS + j(test2) 
	--]]
end
