﻿#pragma checksum "..\..\..\..\ui\settings\UISettings.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "06885A0C1040AA25F187CC7EDB88CC11"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.18444
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace com.newsarea.search.ui.settings {
    
    
    /// <summary>
    /// UISettings
    /// </summary>
    public partial class UISettings : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\..\..\ui\settings\UISettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCancel;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\..\..\ui\settings\UISettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSave;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\..\ui\settings\UISettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid1;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\..\ui\settings\UISettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeView tvSettings;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\..\ui\settings\UISettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lTitle;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\..\ui\settings\UISettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel spSettings;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/search-wpf;component/ui/settings/uisettings.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\ui\settings\UISettings.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            return;
            case 2:
            this.btnSave = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.grid1 = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.tvSettings = ((System.Windows.Controls.TreeView)(target));
            return;
            case 5:
            this.lTitle = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.spSettings = ((System.Windows.Controls.StackPanel)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

