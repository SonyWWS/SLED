/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Windows.Forms;

using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;

namespace Sce.Sled.Lua
{
    public partial class SledLuaUserEnteredWatchedVariablePickTypeForm : Form
    {
        public SledLuaUserEnteredWatchedVariablePickTypeForm()
        {
            InitializeComponent();
        }

        public DomNodeType VariableType
        {
            get
            {
                if (m_rdoGlobal.Checked)
                    return SledLuaSchema.SledLuaVarGlobalType.Type;

                if (m_rdoLocal.Checked)
                    return SledLuaSchema.SledLuaVarLocalType.Type;

                if (m_rdoUpvalue.Checked)
                    return SledLuaSchema.SledLuaVarUpvalueType.Type;

                return SledLuaSchema.SledLuaVarEnvType.Type;
            }
        }
    }
}
