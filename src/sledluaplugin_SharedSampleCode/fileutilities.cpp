/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "fileutilities.h"

#include "../sleddebugger/assert.h"
#include "../sleddebugger/utilities.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#define WIN32_LEAN_AND_MEAN
		#include <windows.h>
	#endif
#endif

#include <cstring>
#include <cstdio>

namespace Examples { namespace FileUtil
{
	namespace Encoding
	{
		enum Enum
		{
			kAscii,
			kUTF8,
			kUTF16,
			kUTF16BE,
			kUTF32,
			kUTF32BE
		};
	}

	bool GetEncoding(std::FILE *pFile, Encoding::Enum& encoding)
	{
		encoding = Encoding::kAscii;

		if (!pFile)
			return false;

		unsigned char bom[4];
		const std::size_t uTotal = (sizeof(unsigned char) * 4);

		const std::size_t uRead = std::fread(bom, sizeof(unsigned char), uTotal, pFile);
		if (uRead != uTotal)
			return false;

		// Try and detect encoding from the bom
		if ((bom[0] == 0xFF) && (bom[1] == 0xFE))
			encoding = Encoding::kUTF16;
		else if ((bom[0] == 0xFE) && (bom[1] == 0xFF))
			encoding = Encoding::kUTF16BE;
		else if ((bom[0] == 0xFF) && (bom[1] == 0xFE) && (bom[2] == 0x00) && (bom[3] == 0x00))
			encoding = Encoding::kUTF32;
		else if ((bom[0] == 0x00) && (bom[1] == 0x00) && (bom[2] == 0xFE) && (bom[3] == 0xFF))
			encoding = Encoding::kUTF32BE;
		else if ((bom[0] == 0xEF) && (bom[1] == 0xBB) && (bom[2] == 0xBF))
			encoding = Encoding::kUTF8;

		return true;
	}

	bool LoadFile(const char *pszFile, char *pBuffer, long lBufSize, long& lOutSize)
	{
		bool bRetval = false;

		long lFileSize = 0;
		long lOffset = 0;
		lOutSize = 0;

		Encoding::Enum encoding = Encoding::kAscii;

		std::FILE *pFile = std::fopen(pszFile, "rb");
		if (!pFile)
			goto end;

		// Find out the file encoding so we can skip the byte order mark (if any)
		if (!GetEncoding(pFile, encoding))
		{
			// Unable to find encoding for some reason
			goto end;
		}

		// If not ascii then set up an offset to skip the bom
		if (encoding != Encoding::kAscii)
		{
			int iSkip = 2;
			switch (encoding)
			{
			case Encoding::kAscii:
				break;
			case Encoding::kUTF16:
				break;
			case Encoding::kUTF16BE:
				break;
			case Encoding::kUTF32:
			case Encoding::kUTF32BE:
				iSkip = 4;
				break;
			case Encoding::kUTF8:
				iSkip = 3;
				break;
			}

			// Skip over the bom
			lOffset = (sizeof(unsigned char) * iSkip);
		}

		// Seek to the end to get the total length
		if (std::fseek(pFile, 0, SEEK_END) != 0)
			goto end;

		lFileSize = std::ftell(pFile);

		// Seek back to the 'beginning'
		if (std::fseek(pFile, lOffset, SEEK_SET) != 0)
			goto end;

		// Check if buffer is big enough
		if ((lFileSize + 1) > lBufSize)
			goto end;

		lOutSize = (long)std::fread((char*)pBuffer, 1, lFileSize, pFile);
		pBuffer[lOutSize] = 0;

		bRetval = true;
	end:
		if (pFile)
			std::fclose(pFile);
		pFile = 0;

		return bRetval;
	}

	FileLoadItems::FileLoadItems(const char *pszRelFilePath)
		: m_bFileLoadInProgress(true)
		, m_bFileLoadSuccessful(false)
		, m_lFileSize(0)
	{
		SCE_SLED_ASSERT(pszRelFilePath != NULL);
		SCE_SLED_ASSERT((int)std::strlen(pszRelFilePath) > 0);

		sce::Sled::Utilities::copyString(m_szRelPath, kStringLen, pszRelFilePath);	
	}

	void FileLoadItems::SetFileContentsAndSize(const char *pszFileContents, const long& lFileSize)
	{
		SCE_SLED_ASSERT(pszFileContents != NULL);
		SCE_SLED_ASSERT(lFileSize > 0);

		m_lFileSize = lFileSize;
		std::memcpy(m_szFileBuf, pszFileContents, lFileSize);
	}

	void FileLoadThread(void *pVoid)
	{
		SCE_SLED_ASSERT(pVoid != NULL);

		FileLoadItems *pItems = static_cast<FileLoadItems*>(pVoid);
		SCE_SLED_ASSERT(pItems != NULL);

		std::printf("FileLoadThread: Attempting to open file: %s\n", pItems->GetRelFilePath());	

		// Try and open the script file
		if (sce::Sled::Utilities::openFileCallback() != NULL)
		{
			FileBuffer hBuffer;

			const char *pszFileContents = sce::Sled::Utilities::openFileCallback()(pItems->GetRelFilePath(), &hBuffer);
			if (pszFileContents != NULL)
			{
				pItems->SetFileContentsAndSize(pszFileContents, hBuffer.lBufSize);
				pItems->SetSuccessful(true);

				std::printf("FileLoadThread: Opened file %s successfully!\n", pItems->GetRelFilePath());
			}
			else
			{
				std::printf("FileLoadThread: Failed to open file: %s\n", pItems->GetRelFilePath());
			}

			if (sce::Sled::Utilities::openFileFinishCallback() != NULL)
				sce::Sled::Utilities::openFileFinishCallback()(pItems->GetRelFilePath(), &hBuffer);
		}
		else
		{
			std::printf("FileLoadThread: No open file callback set; failed to open file: %s\n", pItems->GetRelFilePath());
		}

		pItems->SetLoading(false);
	}

	void FileOpen(FileLoadItems& hFileLoadItems/*, int iAdditionalStackSize*/)
	{
		FileLoadThread(&hFileLoadItems);
	}

	const char *OpenFile(const char *pszFilePath, void *pUserData)
	{
		SCE_SLED_ASSERT(pszFilePath != NULL);
		SCE_SLED_ASSERT(pUserData != NULL);

		FileBuffer *pFileBuffer = static_cast<FileBuffer*>(pUserData);
		pFileBuffer->pBuffer = new char[FileLoadItems::kBufSize];
		pFileBuffer->lBufSize = 0;

		const char *pszPathPrefix = "";

		char szAbsPath[FileLoadItems::kStringLen];
		sce::Sled::Utilities::copyString(szAbsPath, FileLoadItems::kStringLen, pszPathPrefix);
		sce::Sled::Utilities::appendString(szAbsPath, FileLoadItems::kStringLen, pszFilePath);

		if (!LoadFile(szAbsPath, pFileBuffer->pBuffer, FileLoadItems::kBufSize, pFileBuffer->lBufSize))
		{
			pFileBuffer->lBufSize = 0;

			delete [] pFileBuffer->pBuffer;
			pFileBuffer->pBuffer = NULL;

			return NULL;
		}

		return pFileBuffer->pBuffer;
	}

	void OpenFileFinish(const char *pszFilePath, void *pUserData)
	{
		SCE_SLEDUNUSED(pszFilePath);
		SCE_SLED_ASSERT(pUserData != NULL);

		FileBuffer *pFileBuffer = static_cast<FileBuffer*>(pUserData);
		if (pFileBuffer->pBuffer != NULL)
		{
			delete [] pFileBuffer->pBuffer;
			pFileBuffer->pBuffer = NULL;
		}

		pFileBuffer->lBufSize = 0;
	}

	void ValidateWorkingDirectory()
	{
#if SCE_SLEDTARGET_OS_WINDOWS
		// Ensure the working directory is the .exe directory
		// to avoid path mismatches when debugging and using
		// things like "edit and reload"	

		char exeDir[MAX_PATH + 1];
		::GetModuleFileNameA(NULL, exeDir, MAX_PATH + 1);

		{
			const std::size_t len = std::strlen(exeDir);
			std::size_t pos = len;
			while (((exeDir[pos] != '/') && (exeDir[pos] != '\\')) && (pos > 0))
				--pos;

			exeDir[pos + 1] = '\0';
		}	

		char curDir[MAX_PATH + 2];
		::GetCurrentDirectory(MAX_PATH, curDir);

		{
			const std::size_t len = std::strlen(curDir);
			if ((curDir[len - 1] != '/') && (curDir[len - 1] != '\\'))
			{
				curDir[len] = '\\';
				curDir[len + 1] = '\0';
			}
		}

		if (strcmp(exeDir, curDir) != 0)
		{
			if (::SetCurrentDirectory(exeDir))
				std::printf("FileUtil: Changed working directory to .exe directory!\n");
			else
				std::printf("FileUtil: Failed to change working directory to .exe directory - SLED/target file mismatches may occur!\n");
		}	
#endif // SCE_SLEDTARGET_OS_WINDOWS
	}
}}
