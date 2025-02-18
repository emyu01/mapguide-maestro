﻿#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using Maestro.Editors;
using Maestro.Editors.Common;
using Maestro.Editors.FeatureSource;
using Maestro.Editors.Generic;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels.Capabilities;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using System;
using System.Windows.Forms;

namespace Maestro.Base.Editor
{
    internal partial class FsEditorOptionPanel : EditorBindableCollapsiblePanel
    {
        public FsEditorOptionPanel()
        {
            InitializeComponent();
        }

        private IFeatureSource _fs;
        private IFdoProviderCapabilities _caps;
        private IFeatureService _fsvc;
        private IResourceService _rsvc;
        private IEditorService _edsvc;

        public override void Bind(IEditorService service)
        {
            //Only available on MGOS 2.2 and above
            this.LocalPreviewEnabled = service.SiteVersion >= new Version(2, 2);
            _fs = (IFeatureSource)service.GetEditedResource();
            _fsvc = service.CurrentConnection.FeatureService;
            _caps = _fsvc.GetProviderCapabilities(_fs.Provider);
            _rsvc = service.CurrentConnection.ResourceService;
            _edsvc = service;
            _edsvc.RegisterCustomNotifier(this);
            this.ConfigEnabled = _caps.Connection.SupportsConfiguration;
        }

        public bool LocalPreviewEnabled
        {
            get { return btnLocalPreview.Enabled; }
            set { btnLocalPreview.Enabled = value; }
        }

        public bool ConfigEnabled
        {
            get { return btnEditConfiguration.Enabled; }
            set { btnEditConfiguration.Enabled = value; }
        }

        private void btnLocalPreview_Click(object sender, EventArgs e)
        {
            //TODO: We really want to do this as an external process

            var dlg = new MaestroFsPreview.MainForm(_edsvc);
            dlg.FeatureSourceID = _fs.ResourceID;
            dlg.ShowDialog();

            /*
            string exe = PropertyService.Get(ConfigProperties.LocalFsPreviewPath, string.Empty);

            if (!File.Exists(exe))
            {
                using (var dlg = DialogFactory.OpenFile())
                {
                    dlg.Title = string.Format(Strings.LocateExecutable, "MaestroFsPreview.exe"); //NOXLATE
                    dlg.Filter = Strings.FilterExecutables;
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        exe = dlg.FileName;
                        PropertyService.Set(ConfigProperties.LocalFsPreviewPath, exe);
                    }
                }
            }

            var procInfo = new ProcessStartInfo(exe);
            procInfo.WorkingDirectory = Path.GetDirectoryName(exe);
            var conn = _fs.CurrentConnection;
            var clonep = conn.CloneParameters;

            List<string> args = new List<string>();
            foreach (string key in clonep.Keys)
            {
                args.Add("-" + key + ":" + clonep[key]); //NOXLATE
            }
            procInfo.Arguments = string.Join(" ", args.ToArray()); //NOXLATE
            var proc = Process.Start(procInfo);
             */
        }

        private void btnEditConfiguration_Click(object sender, EventArgs e)
        {
            var content = _fs.GetConfigurationContent(_edsvc.CurrentConnection);
            var dlg = new XmlEditorDialog(_edsvc);
            dlg.OnlyValidateWellFormedness = true;
            dlg.XmlContent = content;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                content = dlg.XmlContent;
                _fs.SetConfigurationContent(_edsvc.CurrentConnection, content);
                OnResourceChanged();
            }
        }

        private void btnSpatialContexts_Click(object sender, EventArgs e)
        {
            new SpatialContextsDialog(_fs.ResourceID, _edsvc.CurrentConnection.FeatureService).ShowDialog();
        }
    }
}