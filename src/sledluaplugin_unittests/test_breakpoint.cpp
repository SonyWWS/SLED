/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledluaplugin/luautils.h"
#include "../sleddebugger/sleddebugger.h"
#include "../sleddebugger/utilities.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

namespace sce { namespace Sled { namespace
{
	struct Fixture
	{
		Fixture()
		{
		}
	};

	TEST_FIXTURE(Fixture, Breakpoint_CreateDefault)
	{
		Breakpoint bp;

		CHECK_EQUAL(true, Utilities::areStringsEqual(bp.getFile(), ""));
		CHECK_EQUAL(true, Utilities::areStringsEqual(bp.getFile(), "\0"));
		CHECK_EQUAL(true, Utilities::areStringsEqual(bp.getCondition(), ""));
		CHECK_EQUAL(true, Utilities::areStringsEqual(bp.getCondition(), "\0"));
		CHECK_EQUAL(true, bp.getLine() == 0);
		CHECK_EQUAL(true, bp.getHash() == 0);
		CHECK_EQUAL(true, bp.getResult());

		CHECK_EQUAL(false, bp.hasCondition());
	}
	TEST_FIXTURE(Fixture, Breakpoint_CreateWithValues)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const char *pszCondition = "self.ammo > 5";
		const int32_t iLine = 32;
		const int32_t iHash = -2341;
		const bool bResult = false;

		Breakpoint bp(pszFile, iLine, iHash, pszCondition, bResult);

		CHECK_EQUAL(true, Utilities::areStringsEqual(bp.getFile(), pszFile));
		CHECK_EQUAL(true, Utilities::areStringsEqual(bp.getCondition(), pszCondition));
		CHECK_EQUAL(true, bp.getLine() == iLine);
		CHECK_EQUAL(true, bp.getHash() == iHash);
		CHECK_EQUAL(true, bp.getResult() == bResult);

		CHECK_EQUAL(true, bp.hasCondition());
	}

	TEST_FIXTURE(Fixture, Breakpoint_CheckCopyConstructor)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const char *pszCondition = "self.ammo > 5";
		const int32_t iLine = 32;
		const int32_t iHash = -2341;
		const bool bResult = false;

		Breakpoint bp1(pszFile, iLine, iHash, pszCondition, bResult);
		Breakpoint bp2(bp1);

		CHECK_EQUAL(true, Utilities::areStringsEqual(bp1.getFile(), bp2.getFile()));
		CHECK_EQUAL(true, Utilities::areStringsEqual(bp1.getCondition(), bp2.getCondition()));
		CHECK_EQUAL(true, bp1.getLine() == bp2.getLine());
		CHECK_EQUAL(true, bp1.getHash() == bp2.getHash());
		CHECK_EQUAL(true, bp1.getResult() == bp2.getResult());
	}

	TEST_FIXTURE(Fixture, Breakpoint_GetAndSetCondition)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const int32_t iLine = 32;

		int32_t iHash = 0;
		CHECK_EQUAL(0, debuggerGenerateHash(pszFile, iLine, &iHash));

		Breakpoint bp(pszFile, iLine, iHash);

		CHECK_EQUAL(false, bp.hasCondition());

		const char *pszCondition = "self.ammo > 5";

		bp.setCondition(pszCondition);
		CHECK_EQUAL(true, bp.hasCondition());
		CHECK_EQUAL(true, Utilities::areStringsEqual(pszCondition, bp.getCondition()));

		bp.setCondition(0);
		CHECK_EQUAL(false, bp.hasCondition());
		CHECK_EQUAL(true, Utilities::areStringsEqual("", bp.getCondition()));
		CHECK_EQUAL(true, Utilities::areStringsEqual("\0", bp.getCondition()));

		bp.setCondition(pszCondition);
		CHECK_EQUAL(true, bp.hasCondition());
		CHECK_EQUAL(true, Utilities::areStringsEqual(pszCondition, bp.getCondition()));
	}

	TEST_FIXTURE(Fixture, Breakpoint_GetAndSetResult)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const int32_t iLine = 32;

		int32_t iHash = 0;
		CHECK_EQUAL(0, debuggerGenerateHash(pszFile, iLine, &iHash));

		Breakpoint bp(pszFile, iLine, iHash);

		CHECK_EQUAL(true, bp.getResult());
		bp.setResult(false);
		CHECK_EQUAL(false, bp.getResult());
		bp.setResult(true);
		CHECK_EQUAL(true, bp.getResult());
	}

	TEST_FIXTURE(Fixture, Breakpoint_Equality_CheckSledDebuggerGenerateHashesUsingSameFileAndLine)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const int32_t iLine = 32;

		int32_t iHash = 0;
		CHECK_EQUAL(0, debuggerGenerateHash(pszFile, iLine, &iHash));

		Breakpoint bp1(pszFile, iLine, iHash);
		Breakpoint bp2(pszFile, iLine, iHash);

		CHECK_EQUAL(true, bp1.getHash() == bp2.getHash());
		CHECK_EQUAL(true, bp1 == bp2);
	}

	TEST_FIXTURE(Fixture, Breakpoint_Equality_SameHashesAndSameLinesAndSameFiles)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const int32_t iLine = 32;
		const int32_t iHash = 23413;

		Breakpoint bp1(pszFile, iLine, iHash);
		Breakpoint bp2(pszFile, iLine, iHash);

		CHECK_EQUAL(true, bp1 == bp2);
	}

	TEST_FIXTURE(Fixture, Breakpoint_Equality_SameHashesAndDifferentLinesAndSameFiles)
	{
		const char *pszFile = "/app_home/game/assets/scripts/gun.lua";
		const int32_t iLine1 = 32;
		const int32_t iLine2 = 64;
		const int32_t iHash = 23413;

		Breakpoint bp1(pszFile, iLine1, iHash);
		Breakpoint bp2(pszFile, iLine2, iHash);

		CHECK_EQUAL(false, bp1 == bp2);
	}

	TEST_FIXTURE(Fixture, Breakpoint_Equality_SameHashesAndSameLinesAndDifferentFiles)
	{
		const char *pszFile1 = "/app_home/game/assets/scripts/gun.lua";
		const char *pszFile2 = "/app_home/game/assets/scripts/npc.lua";
		const int32_t iLine = 32;
		const int32_t iHash = 23413;

		Breakpoint bp1(pszFile1, iLine, iHash);
		Breakpoint bp2(pszFile2, iLine, iHash);

		CHECK_EQUAL(false, bp1 == bp2);
	}

	TEST_FIXTURE(Fixture, Breakpoint_LessThanAndGreaterThan)
	{
		const char *pszFile1 = "/app_home/game/assets/scripts/gun.lua";
		const int32_t iLine1 = 64;
		const int32_t iHash1 = -5234;

		Breakpoint bp1(pszFile1, iLine1, iHash1);

		const char *pszFile2 = "/app_home/game/assets/scripts/npc.lua";
		const int32_t iLine2 = 32;
		const int32_t iHash2 = 1324;

		Breakpoint bp2(pszFile2, iLine2, iHash2);

		CHECK_EQUAL(true, bp1 < bp2);
		CHECK_EQUAL(false, bp2 < bp1);
		CHECK_EQUAL(true, bp2 > bp1);
		CHECK_EQUAL(false, bp1 > bp2);
	}
}}}
