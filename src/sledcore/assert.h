/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDASSERT_H
#define SCE_SLEDASSERT_H

#include    "target_macros.h"

/*E Internal macros for string concatenation. */
#define SCE_SLEDASSERT_CONCATENATE(s1,s2)               SCE_SLEDSTRING_CONCATENATE(s1,s2)

/*E Macros for compile-time control of assert enable/disable. */
#ifndef SCE_SLEDASSERTS_ENABLED
#define SCE_SLEDASSERTS_ENABLED   1
#endif

#undef WWS_STATIC_ASSERT

#if SCE_SLEDHOST_COMPILER_MSVC100 || SCE_SLEDHOST_COMPILER_MSVC110 || SCE_SLEDHOST_COMPILER_MSVC120
#define WWS_STATIC_ASSERT(test)	static_assert((test), "Static assertion failed. File: " __FILE__ " Line: " SCE_SLEDTOSTRING(__LINE__) )
#endif  //  #if SCE_SLEDHOST_COMPILER_MSVC100

// Runtime assertion handler
    #define SCE_SLEDASSERT_ATTRIBUTE_PRINTF(i)


typedef bool (*SceSledAssertionHandlerPtr)(const char *pFile, int line, bool stop, const char *pMessage, ...) SCE_SLEDASSERT_ATTRIBUTE_PRINTF(4);

SCE_SLEDCOMMON_INLINE bool sceDefaultAssertionHandler(const char *pFile, int line, bool stop, const char *pMessage, ...)
{
    char outputBuffer[ 512 ];
    bool formatResult;

    va_list argList;

    sceStrFormat( outputBuffer, sizeof( outputBuffer ), "An assertion fired in: %s @ line: %d\n", pFile, line ); 

    sceRawOutput( outputBuffer );

    va_start( argList, pMessage );

    formatResult = sceStrFormatArgs( outputBuffer, sizeof( outputBuffer ), pMessage, argList );
	
	SCE_SLEDUNUSED(formatResult);

    va_end( argList );

    sceRawOutput( outputBuffer );

    return stop;
}

SCE_SLEDWEAK_VAR_BEG SceSledAssertionHandlerPtr g_pSceSledAssertionHandler SCE_SLEDWEAK_VAR_END = sceDefaultAssertionHandler;

#ifndef SCE_SLEDASSERT_PREFIX			///<	Optional message prefix for the SCE_SLEDASSERT_XXX macros
#define SCE_SLEDASSERT_PREFIX
#endif


#if SCE_SLEDHOST_COMPILER_MSVC

// Low-level assertion macro, used by all the macros below.
#define SCE_SLEDASSERT_PRIVATE(test,stop,stopaction,format,...)                  \
    SCE_SLEDMACRO_BEGIN                                                          \
    {                                                                        \
		__pragma(warning(push))											     \
		__pragma(warning(disable:4127))									     \
		if (SCE_SLEDUNLIKELY(!(test)))                                           \
		__pragma(warning(pop))											     \
		{                                                                    \
            bool _stop = (stop);											 \
            bool _result = g_pSceSledAssertionHandler(__FILE__,__LINE__,_stop, \
                        SCE_SLEDASSERT_PREFIX format,##__VA_ARGS__);			 \
            if (_stop && _result) stopaction;								 \
        }                                                                    \
    }                                                                        \
    SCE_SLEDMACRO_END

#define WWS_SIMPLE_ASSERT_PRIVATE(test)                                  \
    SCE_SLEDMACRO_BEGIN                                                      \
    {                                                                    \
		__pragma(warning(push))											 \
		__pragma(warning(disable:4127))									 \
		if (SCE_SLEDUNLIKELY(!(test)))                                       \
		__pragma(warning(pop))											 \
		{                                                                \
            SCE_SLEDBREAK();                                                 \
        }                                                                \
    }                                                                    \
    SCE_SLEDMACRO_END

#endif	//	#if SCE_SLEDHOST_COMPILER_MSVC



#if SCE_SLEDASSERTS_ENABLED

/**	An assert macro. A simple message will be output and execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@note			This assert will be removed at compile-time if the value of <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero.
					The eventual intention is for the user to be able to resume immediately in the event
					of this assert firing.
**/
#define SCE_SLEDASSERT(test)                SCE_SLEDASSERT_PRIVATE(test,true,SCE_SLEDBREAK(),"Assertion failed: %s\n",#test)

/**	An assert macro. A simple message will be output and execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@note			This assert will be removed at compile-time if the value of <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero.
					The eventual intention is that the user must manually move the program counter to be able
					to resume execution in the event of this assert firing. This depends on OS / debugger support.
**/
#define SCE_SLEDSTOP_ASSERT(test)           SCE_SLEDASSERT_PRIVATE(test,true,SCE_SLEDNORETURN_STOP(),"Assertion failed: %s\n",#test)

/**	An assert macro. A warning message will be output if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances
	@note			This assert will be removed at compile-time if the value of <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero.
					This assert will not result in a break in execution.
**/
#define WWS_WARN_ASSERT(test)           SCE_SLEDASSERT_PRIVATE(test,false,(void)0,"Warning - Assertion failed: %s\n",#test)

/**	An assert message macro. A user-supplied message will be output and execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@param msg		A message string in the 'printf-style' for the output message
					(e.g. "The binding named: %s has invalid value: %d").
	@param ...		A variable number of variable parameters which will be inserted
					into the format string.
					The number and types of these parameters should match those
					specified in the format string.
	@note			This assert will be removed at compile-time if the value of <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero.
**/
#define SCE_SLEDASSERT_MSG(test,msg,...)    SCE_SLEDASSERT_PRIVATE(test,true,SCE_SLEDBREAK(),msg,##__VA_ARGS__)

/**	A simple assert macro. Execution will be halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@note			This assert will be removed at compile-time if the value of <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero.
					No message will be output by WWS SDK code upon hitting this assert.
**/
#define WWS_SIMPLE_ASSERT(test)			WWS_SIMPLE_ASSERT_PRIVATE(test)

#else   //  #if SCE_SLEDASSERTS_ENABLED

#define SCE_SLEDASSERT(test)                SCE_SLEDMACRO_BEGIN { (void)sizeof((test)); } SCE_SLEDMACRO_END
#define SCE_SLEDSTOP_ASSERT(test)           SCE_SLEDMACRO_BEGIN { (void)sizeof((test)); } SCE_SLEDMACRO_END
#define WWS_WARN_ASSERT(test)           SCE_SLEDMACRO_BEGIN { (void)sizeof((test)); } SCE_SLEDMACRO_END

#define SCE_SLEDASSERT_MSG(test,msg,...)    SCE_SLEDMACRO_BEGIN { (void)sizeof((test)); } SCE_SLEDMACRO_END

#define WWS_SIMPLE_ASSERT(test)			SCE_SLEDMACRO_BEGIN { (void)sizeof((test)); } SCE_SLEDMACRO_END

#endif  //  #if SCE_SLEDASSERTS_ENABLED
	
/**	An assert always macro. Execution will be halted if the condition specified by 'test' evaluates to false regardless of the value of <c>SCE_SLEDASSERTS_ENABLED</c>.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@note			No message will be output by WWS SDK code upon hitting this assert.
**/
#define SCE_SLEDASSERT_ALWAYS(test)			SCE_SLEDASSERT_PRIVATE(test,true,SCE_SLEDBREAK(),"Assertion failed: %s\n",#test)


// Verify macros
// WWS_VERIFY is like SCE_SLEDASSERT, but it always evaluates the condition -- even when SCE_SLEDASSERTS_ENABLED is 0.
#if SCE_SLEDASSERTS_ENABLED

/**	A verify macro. A simple message will be output and execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@note			The test condition will still be evaluated at compile-time even if <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero. However, in that case the assert will not fire.
**/
#define WWS_VERIFY(test)                SCE_SLEDASSERT_PRIVATE(test,true,SCE_SLEDBREAK(),"Assertion failed: %s\n",#test)

/**	A verify message macro. A user-supplied message will be output and execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by 'test' evaluates to false.

	@param test		A test condition to evaluate. This is assumed to be true in normal circumstances.
	@param msg		A message string in the 'printf-style' for the output message
					(e.g. "The binding named: %s has invalid value: %d").
	@param ...		A variable number of variable parameters which will be inserted
					into the format string.
					The number and types of these parameters should match those
					specified in the format string.
	@note			The test condition will still be evaluated at compile-time even if <c>SCE_SLEDASSERTS_ENABLED</c>
					is zero. However, in that case the assert will not fire.
					The eventual intention is for the user to be able to resume immediately in the event
					of this assert firing.
**/
#define WWS_VERIFY_MSG(test,msg,...)    SCE_SLEDASSERT_PRIVATE(test,true,SCE_SLEDBREAK(),msg,##__VA_ARGS__)

#else   //  #if SCE_SLEDASSERTS_ENABLED

#define WWS_VERIFY(test)                ((void)(test))
#define WWS_VERIFY_MSG(test,msg,...)    ((void)(test))

#endif  //  #if SCE_SLEDASSERTS_ENABLED



/*E  Internal low-level relational assertion macro, used by SCE_SLEDASSERT_RELATION etc. below. */
#define SCE_SLEDASSERT_RELATION_PRIVATE(stop,stopaction,a,op,b)   \
    SCE_SLEDASSERT_PRIVATE((a) op (b), stop, stopaction, "%s: %s %s %s (%lld is not %s %lld)\n", \
        stop ? "WWS Assertion failed":"Warning - Assertion failed", \
        #a, #op, #b, (long long)a, #op, (long long)b)

#if SCE_SLEDASSERTS_ENABLED

/**	An relational assert macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by "'a' 'op' 'b'" evaluates to false.

	@param a		An operand in the test expression 'a' 'op' 'b'.
	@param op		An operator in the test expression 'a' 'op' 'b'.
	@param b		An operand in the test expression 'a' 'op' 'b'.
	@note			The expression 'a' 'op' 'b' must be a valid C++ expression (i.e. the types must be compatible considering the operator).
					The eventual intention is for the user to be able to resume immediately in the event
					of this assert firing.
	**/
#define SCE_SLEDASSERT_RELATION(a,op,b)       SCE_SLEDASSERT_RELATION_PRIVATE(true,SCE_SLEDBREAK(),a,op,b)

/**	An assert equality macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and 'a' is not equal to 'b'.

	@param a		An operand in the test expression 'a' == 'b'.
	@param b		An operand in the test expression 'a' == 'b'.
	@note			The expression 'a' == 'b' must be a valid C++ expression (i.e. the types must be compatible with an equality operator).
					The eventual intention is for the user to be able to resume immediately in the event
					of this assert firing.
	**/
#define SCE_SLEDASSERT_EQUAL(a,b)             SCE_SLEDASSERT_RELATION_PRIVATE(true,SCE_SLEDBREAK(),a,==,b)

/**	An assert inequality macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and 'a' is equal to 'b'.

	@param a		An operand in the test expression 'a' != 'b'.
	@param b		An operand in the test expression 'a' != 'b'.
	@note			The expression 'a' != 'b' must be a valid C++ expression (i.e. the types must be compatible with an inequality operator).
					The eventual intention is for the user to be able to resume immediately in the event
					of this assert firing.
	**/
#define SCE_SLEDASSERT_NOT_EQUAL(a,b)         SCE_SLEDASSERT_RELATION_PRIVATE(true,SCE_SLEDBREAK(),a,!=,b)

/**	An relational assert macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by "'a' 'op' 'b'" evaluates to false.

	@param a		An operand in the test expression 'a' 'op' 'b'.
	@param op		An operator in the test expression 'a' 'op' 'b'.
	@param b		An operand in the test expression 'a' 'op' 'b'.
	@note			The expression 'a' 'op' 'b' must be a valid C++ expression (i.e. the types must be compatible considering the operator).
					The eventual intention is that the user must manually move the program counter to be able
					to resume execution in the event of this assert firing. This depends on OS / debugger support.
	**/
#define SCE_SLEDSTOP_ASSERT_RELATION(a,op,b)  SCE_SLEDASSERT_RELATION_PRIVATE(true,SCE_SLEDNORETURN_STOP(),a,op,b)

/**	An assert equality macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and 'a' is not equal to 'b'.

	@param a		An operand in the test expression 'a' == 'b'.
	@param b		An operand in the test expression 'a' == 'b'.
	@note			The expression 'a' == 'b' must be a valid C++ expression (i.e. the types must be compatible with an equality operator).
					The eventual intention is that the user must manually move the program counter to be able
					to resume execution in the event of this assert firing. This depends on OS / debugger support.
	**/
#define SCE_SLEDSTOP_ASSERT_EQUAL(a,b)        SCE_SLEDASSERT_RELATION_PRIVATE(true,SCE_SLEDNORETURN_STOP(),a,==,b)

/**	An assert inequality macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and 'a' is equal to 'b'.

	@param a		An operand in the test expression 'a' != 'b'.
	@param b		An operand in the test expression 'a' != 'b'.
	@note			The expression 'a' != 'b' must be a valid C++ expression (i.e. the types must be compatible with an inequality operator).
					The eventual intention is that the user must manually move the program counter to be able
					to resume execution in the event of this assert firing. This depends on OS / debugger support.
	**/
#define SCE_SLEDSTOP_ASSERT_NOT_EQUAL(a,b)    SCE_SLEDASSERT_RELATION_PRIVATE(true,SCE_SLEDNORETURN_STOP(),a,!=,b)

/**	An relational assert macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and the condition specified by "'a' 'op' 'b'" evaluates to false.

	@param a		An operand in the test expression 'a' 'op' 'b'.
	@param op		An operator in the test expression 'a' 'op' 'b'.
	@param b		An operand in the test expression 'a' 'op' 'b'.
	@note			The expression 'a' 'op' 'b' must be a valid C++ expression (i.e. the types must be compatible considering the operator).
					This assert will not result in a break in execution.
	**/
#define WWS_WARN_ASSERT_RELATION(a,op,b)  SCE_SLEDASSERT_RELATION_PRIVATE(false,(void)0,a,op,b)

/**	An assert equality macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and 'a' is not equal to 'b'.

	@param a		An operand in the test expression 'a' == 'b'.
	@param b		An operand in the test expression 'a' == 'b'.
	@note			The expression 'a' == 'b' must be a valid C++ expression (i.e. the types must be compatible with an equality operator).
					This assert will not result in a break in execution.
	**/
#define WWS_WARN_ASSERT_EQUAL(a,b)        SCE_SLEDASSERT_RELATION_PRIVATE(false,(void)0,a,==,b)

/**	An assert inequality macro. Execution will be halted if execution halted if <c>SCE_SLEDASSERTS_ENABLED</c> evaluates to true and 'a' is equal to 'b'.

	@param a		An operand in the test expression 'a' != 'b'.
	@param b		An operand in the test expression 'a' != 'b'.
	@note			The expression 'a' != 'b' must be a valid C++ expression (i.e. the types must be compatible with an inequality operator).
					This assert will not result in a break in execution.
	**/
#define WWS_WARN_ASSERT_NOT_EQUAL(a,b)    SCE_SLEDASSERT_RELATION_PRIVATE(false,(void)0,a,!=,b)

#else   //  #if SCE_SLEDASSERTS_ENABLED

#define SCE_SLEDASSERT_RELATION(a,op,b)       SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END
#define SCE_SLEDASSERT_EQUAL(a,b)             SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END
#define SCE_SLEDASSERT_NOT_EQUAL(a,b)         SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END

#define SCE_SLEDSTOP_ASSERT_RELATION(a,op,b)  SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END
#define SCE_SLEDSTOP_ASSERT_EQUAL(a,b)        SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END
#define SCE_SLEDSTOP_ASSERT_NOT_EQUAL(a,b)    SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END

#define WWS_WARN_ASSERT_RELATION(a,op,b)  SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END
#define WWS_WARN_ASSERT_EQUAL(a,b)        SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END
#define WWS_WARN_ASSERT_NOT_EQUAL(a,b)    SCE_SLEDMACRO_BEGIN { (void)(a); (void)(b); } SCE_SLEDMACRO_END

#endif  //  #if SCE_SLEDASSERTS_ENABLED

#endif  //  #ifndef SCE_SLEDASSERT_H
