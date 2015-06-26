file = {}

--[[
Closes file. Note that files are automatically closed when their handles are garbage collected, but that takes an unpredictable amount of time to happen.
--]]
function file:close() end

--[[
Saves any written data to file.
--]]
function file:flush() end

--[[
Returns an iterator function that, each time it is called, returns a new line from the file. Therefore, the construction

     for line in file:lines() do body end
will iterate over all lines of the file. (Unlike io.lines, this function does not close the file when the loop ends.)
--]]
function file:lines() end

--[[
Reads the file file, according to the given formats, which specify what to read. For each format, the function returns a string (or a number) with the characters read, or nil if it cannot read data with the specified format. When called without formats, it uses a default format that reads the entire next line (see below).

The available formats are

"*n": reads a number; this is the only format that returns a number instead of a string.
"*a": reads the whole file, starting at the current position. On end of file, it returns the empty string.
"*l": reads the next line (skipping the end of line), returning nil on end of file. This is the default format.
number: reads a string with up to this number of characters, returning nil on end of file. If number is zero, it reads nothing and returns an empty string, or nil on end of file.
--]]
function file:read( ... ) end

--[[
Sets and gets the file position, measured from the beginning of the file, to the position given by offset plus a base specified by the string whence, as follows:

"set": base is position 0 (beginning of the file);
"cur": base is current position;
"end": base is end of file;
In case of success, function seek returns the final file position, measured in bytes from the beginning of the file. If this function fails, it returns nil, plus a string describing the error.

The default value for whence is "cur", and for offset is 0. Therefore, the call file:seek() returns the current file position, without changing it; the call file:seek("set") sets the position to the beginning of the file (and returns 0); and the call file:seek("end") sets the position to the end of the file, and returns its size.
--]]
function file:seek( whence, offset ) end

--[[
Sets the buffering mode for an output file. There are three available modes:

"no": no buffering; the result of any output operation appears immediately.
"full": full buffering; output operation is performed only when the buffer is full (or when you explicitly flush the file (see io.flush)).
"line": line buffering; output is buffered until a newline is output or there is any input from some special files (such as a terminal device).
For the last two cases, size specifies the size of the buffer, in bytes. The default is an appropriate size.
--]]
function file:setvbuf( mode, size ) end

--[[
Writes the value of each of its arguments to the file. The arguments must be strings or numbers. To write other values, use tostring or string.format before write.
--]]
function file:write( ... ) end
