/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace sce { namespace Sled
{
	class ScopedNetwork
	{
	public:
		ScopedNetwork();
		~ScopedNetwork();
	public:
		bool IsValid();
	};
}}
