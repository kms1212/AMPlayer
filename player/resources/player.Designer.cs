//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AMPlayer.resources {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class player : global::System.Configuration.ApplicationSettingsBase {
        
        private static player defaultInstance = ((player)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new player())));
        
        public static player Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("international")]
        public string Locale {
            get {
                return ((string)(this["Locale"]));
            }
            set {
                this["Locale"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("default")]
        public string Theme {
            get {
                return ((string)(this["Theme"]));
            }
            set {
                this["Theme"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\mplayer\\mplayer.exe ")]
        public string MediaPlayer {
            get {
                return ((string)(this["MediaPlayer"]));
            }
            set {
                this["MediaPlayer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-osdlevel 0 -nofs -identify -slave -nomouseinput -sub-fuzziness 1 -vo direct3d, -" +
            "ao dsound -wid $(OutputHandle) \"$(Path)\"")]
        public string Arguments {
            get {
                return ((string)(this["Arguments"]));
            }
            set {
                this["Arguments"] = value;
            }
        }
    }
}
