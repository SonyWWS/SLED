/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_FILE_UTILITIES_H__
#define __SCE_FILE_UTILITIES_H__

namespace Examples { namespace FileUtil
{
	bool LoadFile(const char *pszFile, char *pBuffer, long lBufSize, long& lOutSize);

	class FileLoadItems
	{
	public:
		const static long kBufSize = 1024 * 16; // test target compiled script is ~9k, allow some extra for user testing
		const static int kStringLen = 256;
	public:
		FileLoadItems(const char *pszRelFilePath);
		~FileLoadItems() {}
	private:
		FileLoadItems(const FileLoadItems&);
		FileLoadItems& operator=(const FileLoadItems&);
	public:
		bool IsLoading() const { return m_bFileLoadInProgress; }
		bool IsSuccessful() const { return m_bFileLoadSuccessful; }
	public:
		const char *GetRelFilePath() const { return m_szRelPath; }
		const char *GetFileContents() const { return m_szFileBuf; }
		long GetFileSize() const { return m_lFileSize; }
	public:
		void SetFileContentsAndSize(const char *pszFileContents, const long& lFileSize);
		void SetLoading(const bool& bLoading) { m_bFileLoadInProgress = bLoading; }
		void SetSuccessful(const bool& bSuccessful) { m_bFileLoadSuccessful = bSuccessful; }
	private:
		char m_szRelPath[kStringLen];
		char m_szFileBuf[kBufSize];	
		volatile bool m_bFileLoadInProgress;
		volatile bool m_bFileLoadSuccessful;
		long m_lFileSize;
	};

	void FileOpen(FileLoadItems& hFileLoadItems/*, int iAdditionalStackSize*/);

	// Callbacks for SledDebugger to use
	const char *OpenFile(const char *pszFilePath, void *pUserData);
	void OpenFileFinish(const char *pszFilePath, void *pUserData);

	struct FileBuffer
	{
		FileBuffer() : pBuffer(0), lBufSize(0) {}
		~FileBuffer()
		{
			if (pBuffer)
			{
				delete [] pBuffer;
				pBuffer = 0;
			}

			lBufSize = 0;
		}

		char *pBuffer;
		long lBufSize;
	};

	void ValidateWorkingDirectory();
}}

#endif // __SCE_FILE_UTILITIES_H__
