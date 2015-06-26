--[[
--   lua standard library -- Basic Functions
--]]

--[[
Issues an error when the value of its argument <em>v</em> is false (i.e., nil or false); otherwise, returns all its arguments. 
<em>message</em> is an error message; when absent, it defaults to "assertion failed!" 
--]]
function assert( v, message ) end

--[[
This function is a generic interface to the garbage collector. It performs different functions according to its first argument, opt:

* "collect": performs a full garbage-collection cycle. This is the default option.
* "stop": stops automatic execution of the garbage collector. The collector will run only when explicitly invoked, until a call to restart it.
* "restart": restarts automatic execution of the garbage collector.
* "count": returns the total memory in use by Lua (in Kbytes) and a second value with the total memory in bytes modulo 1024. The first value has a fractional part, so the following equality is always true:
     k, b = collectgarbage("count")
     assert(k*1024 == math.floor(k)*1024 + b)
(The second result is useful when Lua is compiled with a non floating-point type for numbers.)

* "step": performs a garbage-collection step. The step "size" is controlled by arg (larger values mean more steps) in a non-specified way. If you want to control the step size you must experimentally tune the value of arg. Returns true if the step finished a collection cycle.
* "setpause": sets arg as the new value for the pause of the collector (see §2.5). Returns the previous value for pause.
* "setstepmul": sets arg as the new value for the step multiplier of the collector (see §2.5). Returns the previous value for step.
* "isrunning": returns a boolean that tells whether the collector is running (i.e., not stopped).
* "generational": changes the collector to generational mode. This is an experimental feature (see §2.5).
* "incremental": changes the collector to incremental mode. This is the default mode.
--]]
function collectgarbage( opt, arg ) end

--[[
Opens the named file and executes its contents as a Lua chunk. 
When called without arguments, dofile executes the contents of the standard input (stdin). 
Returns all values returned by the chunk. 
In case of errors, dofile propagates the error to its caller (that is, dofile does not run in protected mode).
--]]
function dofile( filename ) end

--[[
Terminates the last protected function called and returns message as the error message. Function error never returns.
Usually, error adds some information about the error position at the beginning of the message, if the message is a string. The level argument specifies how to get the error position. With level 1 (the default), the error position is where the error function was called. Level 2 points the error to where the function that called error was called; and so on. Passing a level 0 avoids the addition of error position information to the message.
--]]
function error( message, level ) end

--[[
A global variable (not a function) that holds the global environment. Lua itself does not use this variable; changing its value does not affect any environment, nor vice-versa.
--]]
_G = {}

--[[
If object does not have a metatable, returns nil. Otherwise, if the object's metatable has a "__metatable" field, returns the associated value. Otherwise, returns the metatable of the given object.
--]]
function getmetatable( object ) end

--[[
If t has a metamethod __ipairs, calls it with t as argument and returns the first three results from the call.

Otherwise, returns three values: an iterator function, the table t, and 0, so that the construction

     for i,v in ipairs(t) do body end
will iterate over the pairs (1,t[1]), (2,t[2]), ..., up to the first integer key absent from the table.
--]]
function ipairs( table ) end

--[[
Loads a chunk.

If ld is a string, the chunk is this string. If ld is a function, load calls it repeatedly to get the chunk pieces. Each call to ld must return a string that concatenates with previous results. A return of an empty string, nil, or no value signals the end of the chunk.

If there are no syntactic errors, returns the compiled chunk as a function; otherwise, returns nil plus the error message.

If the resulting function has upvalues, the first upvalue is set to the value of the global environment or to env, if that parameter is given. When loading main chunks, the first upvalue will be the _ENV variable (see §2.2).

source is used as the source of the chunk for error messages and debug information (see §4.9). When absent, it defaults to ld, if ld is a string, or to "=(load)" otherwise.

The string mode controls whether the chunk can be text or binary (that is, a precompiled chunk). It may be the string "b" (only binary chunks), "t" (only text chunks), or "bt" (both binary and text). The default is "bt".
--]]
function load( ld, source, mode, env ) end

--[[
Similar to load, but gets the chunk from file filename or from the standard input, if no file name is given.
--]]
function loadfile( filename, mode, env ) end

--[[
Allows a program to traverse all fields of a table. Its first argument is a table and its second argument is an index in this table. next returns the next index of the table and its associated value. When called with nil as its second argument, next returns an initial index and its associated value. When called with the last index, or with nil in an empty table, next returns nil. If the second argument is absent, then it is interpreted as nil. In particular, you can use next(t) to check whether a table is empty.

The order in which the indices are enumerated is not specified, even for numeric indices. (To traverse a table in numeric order, use a numerical for.)

The behavior of next is undefined if, during the traversal, you assign any value to a non-existent field in the table. You may however modify existing fields. In particular, you may clear existing fields.
--]]
function next( table, index ) end

--[[
If t has a metamethod __pairs, calls it with t as argument and returns the first three results from the call.

Otherwise, returns three values: the next function, the table t, and nil, so that the construction

     for k,v in pairs(t) do body end
will iterate over all key–value pairs of table t.

See function next for the caveats of modifying the table during its traversal.
--]]
function pairs( t ) end

--[[
Calls function f with the given arguments in protected mode. This means that any error inside f is not propagated; instead, pcall catches the error and returns a status code. Its first result is the status code (a boolean), which is true if the call succeeds without errors. In such case, pcall also returns all results from the call, after this first result. In case of any error, pcall returns false plus the error message.
--]]
function pcall( func, ... ) end

--[[
Receives any number of arguments and prints their values to stdout, using the tostring function to convert each argument to a string. print is not intended for formatted output, but only as a quick way to show a value, for instance for debugging. For complete control over the output, use string.format and io.write.
--]]
function print( ... ) end

--[[
Checks whether v1 is equal to v2, without invoking any metamethod. Returns a boolean.
--]]
function rawequal( v1, v2 ) end

--[[
Gets the real value of table[index], without invoking any metamethod. table must be a table; index may be any value.
--]]
function rawget( table, index) end

--[[
Returns the length of the object v, which must be a table or a string, without invoking any metamethod. Returns an integer number.
--]]
function rawlen( v ) end

--[[
Sets the real value of table[index] to value, without invoking any metamethod. table must be a table, index any value different from nil and NaN, and value any Lua value.
This function returns table.
--]]
function rawset( table, index, value ) end

--[[
If index is a number, returns all arguments after argument number index; a negative number indexes from the end (-1 is the last argument). Otherwise, index must be the string "#", and select returns the total number of extra arguments it received.
--]]
function select( index, ... ) end

--[[
Sets the metatable for the given table. (You cannot change the metatable of other types from Lua, only from C.) If metatable is nil, removes the metatable of the given table. If the original metatable has a "__metatable" field, raises an error.

This function returns table.
--]]
function setmetatable( table, metatable ) end

--[[
When called with no base, tonumber tries to convert its argument to a number. If the argument is already a number or a string convertible to a numbe, then tonumber returns this number; otherwise, it returns nil.

When called with base, then e should be a string to be interpreted as an integer numeral in that base. The base may be any integer between 2 and 36, inclusive. In bases above 10, the letter 'A' (in either upper or lower case) represents 10, 'B' represents 11, and so forth, with 'Z' representing 35. If the string e is not a valid numeral in the given base, the function returns nil.
--]]
function tonumber( e, base ) end

--[[
Receives a value of any type and converts it to a string in a reasonable format. (For complete control of how numbers are converted, use string.format.)
If the metatable of v has a "__tostring" field, then tostring calls the corresponding value with v as argument, and uses the result of the call as its result.
--]]
function tostring( v ) end

--[[
A global variable (not a function) that holds a string containing the current interpreter version. The current contents of this variable is "Lua 5.2".
--]]
_VERSION = __std.String

--[[
This function is similar to pcall, except that it sets a new message handler msgh.
--]]
function xpcall( f, msgh, arg1, ... ) end

--[[
Returns the type of the variable var.
--]]
function type( var ) return __std.String end

--[[
Returns the environment of object o.
--]]
function getfenv( o ) end

--[[
Sets the environment of the given object to the given table. Returns object.
--]]
function setfenv( object, table ) end
