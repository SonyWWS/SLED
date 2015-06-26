/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LUASTUFF_H__
#define __SCE_LUASTUFF_H__

#include "../sledcore/base_types.h"

#include <cstddef>

// Forward declaration
struct lua_State;

namespace LuaStuff
{
	void *Allocate(void *ud, void *ptr, std::size_t oldSize, std::size_t newSize);

	int CFunc1(lua_State *pLuaState);
	int CFunc2(lua_State *pLuaState);

	class Actor
	{
	public:
		Actor() { Init(); }
		~Actor() {}

		inline void Init() { m_uAnimIndex = 0; m_uItem1Index = 1; m_uItem2Index = 2; m_szName[0] = 0; }

		inline const char *GetName() const { return m_szName; }
		inline void SetName(const char *pszName);

		inline uint16_t GetAnimIndex() const { return m_uAnimIndex; }
		inline void SetAnimIndex(const uint16_t& uAnimIndex) { m_uAnimIndex = uAnimIndex; }
		inline const char *GetAnimName() const;

		inline uint16_t GetItem1Index() const { return m_uItem1Index; }
		inline void SetItem1Index(const uint16_t& uItem1Index) { m_uItem1Index = uItem1Index; }
		inline const char *GetItem1Name() const;

		inline uint16_t GetItem2Index() const { return m_uItem2Index; }
		inline void SetItem2Index(const uint16_t& uItem2Index) { m_uItem2Index = uItem2Index; }
		inline const char *GetItem2Name() const;
	private:
		uint16_t		m_uAnimIndex;
		uint16_t		m_uItem1Index;
		uint16_t		m_uItem2Index;
		char	m_szName[16];
	};

	int luaopen_Actor(lua_State *pLuaState);
	int LookupTypeVal(lua_State *pLuaState, const int& iIndex, char *pType, char *pValue, bool bPop = true);
	void ShowStack(lua_State *pLuaState, const char *pszMsg);
	void DefaultTTYHandler(const char *pszMessage);
	void OpenLibs(lua_State *pLuaState);
	void RunFunctionWith3Args(lua_State *pLuaState, const char *pszFunction, int iArg1, int iArg2, int iArg3, int errorHandlerAbsIndex);
	void RunThreadFunctionWith3Args(lua_State *pLuaState, const char *pszFunction, int iArg1, int iArg2, int iArg3);
	int LoadFileContentsIntoLuaState(lua_State *pLuaState, const char *pszFileContents, const int& iFileLen, const char *pszBufferName);
	const char *CustomUserDataCallback(void *pLuaUserData, void *pUserData);
	void CustomUserDataFinishCallback(void *pLuaUserData, void *pUserData);

	class StackReconciler
	{
	public:
		StackReconciler(lua_State *pLuaState);
		~StackReconciler();
	private:
		StackReconciler(const StackReconciler&);
		StackReconciler& operator=(const StackReconciler&);
	private:
		lua_State *m_pLuaState;
		const int m_iCount1;
	};
}

#endif // __SCE_LUASTUFF_H__
