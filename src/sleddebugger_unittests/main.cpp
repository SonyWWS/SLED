/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledcore/target_macros.h"

#include "../sleddebugger/assert.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>
#include <unittest-cpp/UnitTest++/CompositeTestReporter.h>
#include <unittest-cpp/UnitTest++/TestReporterStdout.h>
#include <unittest-cpp/UnitTest++/ReportAssert.h>

#include "scoped_network.h"

#if SCE_SLEDTARGET_OS_WIN32
	#define SCE_SLED_WRITE_XML_TESTS_FILE 1
#endif

#ifndef SCE_SLED_WRITE_XML_TESTS_FILE
	#define SCE_SLED_WRITE_XML_TESTS_FILE 0
#endif

sce::Sled::Assert::FailureBehavior FailTestOnAssertHandler(const char *condition, const char *file, const int& line, const char *msg)
{
	UnitTest::ReportAssert(condition ? condition : (msg ? msg : "Fail"), file, line);
	return sce::Sled::Assert::kContinue;
}

void FailTestOnLogErrorHandler(sce::Sled::Logging::Level lvl, const char *file, const int& line, const char *msg)
{
	// Trap warnings & errors by default
	if (lvl > sce::Sled::Logging::kInfo)
		UnitTest::ReportAssert(msg ? msg : "Fail", file, line);
}

int main()
{
	sce::Sled::Assert::setAssertHandler(FailTestOnAssertHandler);
	sce::Sled::Logging::setLogHandler(FailTestOnLogErrorHandler);

	sce::Sled::ScopedNetwork sn;
	if (!sn.IsValid())
		UnitTest::ReportAssert("Networking failed!", __FILE__, __LINE__);

#ifdef UNITTEST_XML_NAME
#define UNITTEST_DIRECT_TO_STRING(arg) #arg
#define UNITTEST_INDIRECT_TO_STRING(arg) UNITTEST_DIRECT_TO_STRING(arg)

	UnitTest::TestReporterStdout stdoutReporter;

	UnitTest::CompositeTestReporter reporter;
    reporter.AddReporter(&stdoutReporter);

	UnitTest::TestRunner runner(reporter);
	return runner.RunTestsIf(UnitTest::Test::GetTestList(), NULL, UnitTest::True(), 0);
#else
    #error "UNITTEST_XML_NAME must be defined and set to the basename of the executable."
#endif
}
