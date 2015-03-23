namespace Server
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mdnsServiceInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.mdnsInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // mdnsServiceInstaller
            // 
            this.mdnsServiceInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.mdnsServiceInstaller.Password = null;
            this.mdnsServiceInstaller.Username = null;
            // 
            // mdnsInstaller
            // 
            this.mdnsInstaller.Description = "A fully-managed multicast DNS server service.";
            this.mdnsInstaller.DisplayName = "Multicast DNS Server";
            this.mdnsInstaller.ServiceName = "MulticastServer";
            this.mdnsInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.mdnsServiceInstaller,
            this.mdnsInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller mdnsServiceInstaller;
        private System.ServiceProcess.ServiceInstaller mdnsInstaller;
    }
}