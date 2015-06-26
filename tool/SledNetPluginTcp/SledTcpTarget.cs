/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Net;

using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Net.Tcp
{
    internal class SledTcpTarget : SledTargetBase
    {
        public SledTcpTarget(string name, IPEndPoint endPoint, ISledNetworkPlugin plugin, bool imported)
            : base(name, endPoint, plugin)
        {
            Imported = imported;
        }

        #region ICloneable Interface

        public override object Clone()
        {
            var target =
                new SledTcpTarget(
                    Name.Clone() as string,
                    new IPEndPoint(EndPoint.Address, EndPoint.Port),
                    Plugin,
                    Imported);

            return target;
        }

        #endregion
    }

}
