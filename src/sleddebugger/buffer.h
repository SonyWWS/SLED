/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_BUFFER_H__
#define __SCE_LIBSLEDDEBUGGER_BUFFER_H__

#include "../sledcore/base_types.h"
#include <cstdio>

#include "common.h"

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
#ifndef DOXYGEN_IGNORE
	/// Network buffer configuration structure.
	/// @brief
	/// Network buffer configuration struct.
	struct SCE_SLED_LINKAGE NetworkBufferConfig
	{
		uint32_t	maxSize;	///< Maximum size to allocate for buffer

		/// <c>NetworkBufferConfig</c> constructor.
		/// @brief
		/// Constructor.
		NetworkBufferConfig() : maxSize(1024) {}
	};

	// Forward declaration.
	class NetworkBufferPacker;
	class NetworkBufferReader;
	class ISequentialAllocator;

	/// Class for network buffer.
	/// @brief
	/// Network buffer class.
	class SCE_SLED_LINKAGE NetworkBuffer
	{
	public:
		/// Create a NetworkBuffer instance.
		/// @brief
		/// Create NetworkBuffer instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param netBufferConfig Configuration structure that details the settings to use
		/// @param pLocation Location in memory in which to place the NetworkBuffer instance.
		/// It needs to be as big as the value returned by <c>requiredMemory()</c>.
		/// @param ppBuffer NetworkBuffer instance that is created
		///
		/// @retval 0										Success
		/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION		Invalid value in the configuration structure
		///
		/// @see
		/// <c>requiredMemory</c>, <c>shutdown</c>
		static int32_t create(const NetworkBufferConfig& netBufferConfig, void *pLocation, NetworkBuffer **ppBuffer);

		/// Calculate the size in bytes required for a NetworkBuffer instance, based on a configuration structure.
		/// @brief
		/// Calculate size in bytes required for NetworkBuffer instance, based on configuration struct.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param netBufferConfig Configuration structure that details the settings to use
		/// @param iRequiredMemory The amount of memory that is needed for the NetworkBuffer instance
		///
		/// @retval 0										Success
		/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION		Invalid value in the configuration structure
		///
		/// @see
		/// <c>create</c>
		static int32_t requiredMemory(const NetworkBufferConfig& netBufferConfig, std::size_t *iRequiredMemory);

#ifndef DOXYGEN_IGNORE
		static int32_t requiredMemoryHelper(const NetworkBufferConfig& netBufferConfig, ISequentialAllocator *pAllocator, void **ppThis);
#endif // DOXYGEN_IGNORE

		/// Shut down a NetworkBuffer instance.
		/// @brief
		/// Shut down NetworkBuffer instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pBuffer The NetworkBuffer instance to shut down
		///
		/// @see
		/// <c>create</c>
		static void shutdown(NetworkBuffer *pBuffer);
	private:
		NetworkBuffer(const NetworkBufferConfig& netBufferConfig, void *pMemoryPool);
		~NetworkBuffer() {}
		NetworkBuffer(const NetworkBuffer&);
		NetworkBuffer& operator=(const NetworkBuffer&);
	public:
		/// Get the size of data in the buffer.
		/// @brief
		/// Get size of data in buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Size of data in the buffer
		///
		/// @see
		/// <c>getMaxSize</c>
		inline uint32_t getSize() const { return m_iSize; }

		/// Get the maximum size of the buffer.
		/// @brief
		/// Get maximum size of buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Maximum size of the buffer
		///
		/// @see
		/// <c>getSize</c>
		inline uint32_t getMaxSize() const { return m_iMaxSize; }

		/// Get the data in the buffer.
		/// @brief
		/// Get data in buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data in the buffer
		///
		/// @see
		/// <c>reset</c>, <c>append</c>, <c>shuffle</c>
		inline uint8_t *getData() const { return m_pBuffer; }

		/// Clear the contents of the buffer.
		/// @brief
		/// Clear contents of buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @see
		/// <c>getData</c>, <c>append</c>, <c>shuffle</c>
		inline void reset() { m_iSize = 0; }

		/// Add data to the buffer.
		/// @brief
		/// Add data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pData Data to add
		/// @param iSize Size of the data to add
		///
		/// @return True if the data was added to buffer; false if it was not
		///
		/// @see
		/// <c>getData</c>, <c>reset</c>, <c>shuffle</c>
		bool append(const uint8_t *pData, const int32_t& iSize);

		/// Remove a specific amount of data from the buffer.
		/// @brief
		/// Remove specific amount of data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param iHowMuch Size of the chunk of data to remove
		///
		/// @see
		/// <c>getData</c>, <c>reset</c>, <c>append</c>
		void shuffle(const uint32_t& iHowMuch);
	private:
		uint32_t		m_iMaxSize;
		uint32_t		m_iSize;
		uint8_t*		m_pBuffer;
	private:
		friend class NetworkBufferPacker;
	};

	/// Network buffer packer class. This class is used to help add data to an underlying NetworkBuffer.
	/// @brief
	/// Network buffer packer class.
	class SCE_SLED_LINKAGE NetworkBufferPacker
	{
	public:
		/// <c>NetworkBufferPacker</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param pBuffer The NetworkBuffer to use
		NetworkBufferPacker(NetworkBuffer *pBuffer);
	private:
		NetworkBufferPacker(const NetworkBufferPacker&);
		NetworkBufferPacker& operator=(const NetworkBufferPacker&);
	public:
		/// Pack or add unsigned 8 bit int data to the buffer.
		/// @brief
		/// Pack or add unsigned 8 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packUInt16_t</c>, <c>packUInt32_t</c>, <c>packUInt64_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>readUInt8_t</c>
		bool packUInt8_t(const uint8_t& value);

		/// Pack or add unsigned 16 bit int data to the buffer.
		/// @brief
		/// Pack or add unsigned 16 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packUInt8_t</c>, <c>packUInt32_t</c>, <c>packUInt64_t</c>, <c>packInt16_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>readUInt16_t</c>
		bool packUInt16_t(const uint16_t& value);

		/// Pack or add unsigned 32 bit int data to the buffer.
		/// @brief
		/// Pack or add unsigned 32 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer.
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packUInt8_t</c>, <c>packUInt16_t</c>, <c>packUInt64_t</c>, <c>packInt32_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>readUInt32_t</c>
		bool packUInt32_t(const uint32_t& value);

		/// Pack or add unsigned 64 bit int data to the buffer.
		/// @brief
		/// Pack or add unsigned 64 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packUInt8_t</c>, <c>packUInt16_t</c>, <c>packUInt32_t</c>, <c>packInt64_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>readUInt64_t</c>
		bool packUInt64_t(const uint64_t& value);

		/// Pack or add 16 bit int data to the buffer.
		/// @brief
		/// Pack or add 16 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packInt32_t</c>, <c>packInt64_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>packUInt16_t</c>, <c>readInt16_t</c>
		bool packInt16_t(const int16_t& value);

		/// Pack or add 32 bit int data to the buffer.
		/// @brief
		/// Pack or add 32 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packInt16_t</c>, <c>packInt64_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>packUInt32_t</c>, <c>readInt32_t</c>
		bool packInt32_t(const int32_t& value);

		/// Pack or add 64 bit int data to the buffer.
		/// @brief
		/// Pack or add 64 bit int data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packInt16_t</c>, <c>packInt32_t</c>, <c>packFloat</c>, <c>packString</c>, <c>packDouble</c>, <c>packUInt64_t</c>, <c>readInt64_t</c>
		bool packInt64_t(const int64_t& value);

		/// Pack or add float data to the buffer.
		/// @brief
		/// Pack or add float data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packDouble</c>, <c>packInt16_t</c>, <c>packInt32_t</c>, <c>packInt64_t</c>, <c>packString</c>, <c>packDouble</c>, <c>packDouble</c>, <c>readFloat</c>
		bool packFloat(const float& value);

		/// Pack or add double data to the buffer.
		/// @brief
		/// Pack or add double data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packInt16_t</c>, <c>packInt32_t</c>, <c>packInt64_t</c>, <c>packFloat</c>, <c>packString</c>, <c>readDouble</c>
		bool packDouble(const double& value);

		/// Pack or add char* data to the buffer.
		/// @brief
		/// Pack or add char* data to buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param value Data to add to the buffer
		///
		/// @return True if the value could be added; false if the value could not be added
		///
		/// @see
		/// <c>packInt16_t</c>, <c>packInt32_t</c>, <c>packInt64_t</c>, <c>packFloat</c>, <c>packDouble</c>, <c>readString</c>
		bool packString(const char *value);
	private:
		NetworkBuffer *m_pNetBuffer;
	};

	/// Network buffer reader class. This class is used to help get data out of a data stream.
	/// @brief
	/// Network buffer reader class.
	class SCE_SLED_LINKAGE NetworkBufferReader
	{
	public:
		/// <c>NetworkBufferReader</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param pData The data stream to use
		/// @param iSize Size of the data stream
		NetworkBufferReader(const uint8_t *pData, const uint32_t& iSize);
	private:
		NetworkBufferReader(const NetworkBufferReader&);
		NetworkBufferReader& operator=(const NetworkBufferReader&);
	public:
		/// Read unsigned 8 bit int data from the buffer.
		/// @brief
		/// Read unsigned 8 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readUInt16_t</c>, <c>readUInt32_t</c>, <c>readUInt64_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packUInt8_t</c>
		uint8_t readUInt8_t();

		/// Read unsigned 16 bit int data from the buffer.
		/// @brief
		/// Read unsigned 16 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readInt16_t</c>, <c>readUInt32_t</c>, <c>readUInt64_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packUInt16_t</c>
		uint16_t readUInt16_t();

		/// Read unsigned 32 bit int data from the buffer.
		/// @brief
		/// Read unsigned 32 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readInt32_t</c>, <c>readUInt16_t</c>, <c>readUInt64_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packUInt32_t</c>
		uint32_t readUInt32_t();

		/// Read unsigned 64 bit int data from the buffer.
		/// @brief
		/// Read unsigned 64 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readInt64_t</c>, <c>readUInt16_t</c>, <c>readUInt32_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packUInt32_t</c>
		uint64_t readUInt64_t();

		/// Read 16 bit int data from the buffer.
		/// @brief
		/// Read 16 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readUInt16_t</c>, <c>readInt32_t</c>, <c>readInt64_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packInt16_t</c>
		int16_t readInt16_t();

		/// Read 32 bit int data from the buffer.
		/// @brief
		/// Read 32 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readUInt32_t</c>, <c>readInt16_t</c>, <c>readInt64_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packInt32_t</c>
		int32_t readInt32_t();

		/// Read 64 bit int data from the buffer.
		/// @brief
		/// Read 64 bit int data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readUInt64_t</c>, <c>readInt16_t</c>, <c>readInt32_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>readString</c>, <c>packInt64_t</c>
		int64_t readInt64_t();

		/// Read float data from the buffer.
		/// @brief
		/// Read float data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readInt16_t</c>, <c>readInt32_t</c>, <c>readInt64_t</c>, <c>readDouble</c>, <c>readString</c>, <c>packFloat</c>
		float readFloat();

		/// Read double data from the buffer.
		/// @brief
		/// Read double data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Data
		///
		/// @see
		/// <c>readInt16_t</c>, <c>readInt32_t</c>, <c>readInt64_t</c>, <c>readFloat</c>, <c>readString</c>, <c>packDouble</c>
		double readDouble();

		/// Assumes that the next item in the buffer is a string and returns the length of the string without modifying the buffer.
		/// @brief
		/// Assumes that next item in the buffer is string and returns length of string without modifying buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Length of the string (includes 1 extra space for '\0')
		///
		/// @see
		/// <c>readString</c>, <c>packString</c>
		uint16_t peekStringLen() const;

		/// Read char* data from the buffer.
		/// @brief
		/// Read char* data from buffer.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pBuffer Buffer to copy the string to
		/// @param iBufLen Size of the buffer to copy the string to
		///
		/// @see
		/// <c>peekStringLen</c>, <c>readInt16_t</c>, <c>readInt32_t</c>, <c>readInt64_t</c>, <c>readFloat</c>, <c>readDouble</c>, <c>packString</c>
		void readString(char *pBuffer, const uint16_t& iBufLen);
	private:
		const uint8_t *m_pBuffer;
		const uint32_t m_iMaxSize;
		uint32_t m_iOffset;
	};
#endif //DOXYGEN_IGNORE
}}

#endif // __SCE_LIBSLEDDEBUGGER_BUFFER_H__
