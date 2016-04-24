using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using CommonControls;

namespace PseudoGame
{
    class MainViewModel
    {
        public ICommand LoadMagFilterCommand { get; private set; }
        public ICommand UnloadMagFilterCommand { get; private set; }
        public ICommand StartupMagFilterCommand { get; private set; }
        public ICommand ShutdownMagFilterCommand { get; private set; }
        // dynamic AppDomain objects
        private AppDomain _appDomain;
        private Assembly _magFilterAssembly;
        private Type _magFilterCoreType;
        private object _magFilterObject;
        // static AppDomain objects
        private MagFilter.FilterCore _magFilterCore;

        public MainViewModel()
        {
            LoadMagFilterCommand = new DelegateCommand(LoadMagFilter);
            UnloadMagFilterCommand = new DelegateCommand(UnloadMagFilter);
            StartupMagFilterCommand = new DelegateCommand(StartupMagFilter);
            ShutdownMagFilterCommand = new DelegateCommand(ShutdownMagFilter);
        }
        public void LoadMagFilter()
        {
            if (_appDomain != null) { ErrorMsg("AppDomain already loaded"); return; }
            AppDomainSetup domainInfo = new AppDomainSetup();
            domainInfo.ApplicationBase = System.Environment.CurrentDirectory;
            System.Security.Policy.Evidence evidence = AppDomain.CurrentDomain.Evidence;
            _appDomain = AppDomain.CreateDomain("Mag-Filter-AppDomain", evidence, domainInfo);

            Type proxyType = typeof(ProxyDomain);
            // Wound up getting exceptions from this when called from launcher
            var value = (ProxyDomain)_appDomain.CreateInstanceAndUnwrap(
                proxyType.Assembly.FullName,
                proxyType.FullName
                );

            string magFilterFilepath = @"..\..\..\Mag-Filter\bin\Debug\MagFilter.dll";
            var absolutePath = System.IO.Path.GetFullPath(magFilterFilepath);


            _magFilterAssembly = value.GetAssembly(absolutePath);
            _magFilterCoreType = _magFilterAssembly.GetType("MagFilter.FilterCore");
            _magFilterObject = Activator.CreateInstance(_magFilterCoreType);
            MethodInfo startupMethod = _magFilterCoreType.GetMethod("ExternalStartup");
            startupMethod.Invoke(_magFilterObject, null);
            
            /*

            string magFilterFilepath = @"..\..\..\Mag-Filter\bin\Debug\MagFilter.dll";
            _magFilterAssembly = _appDomain.Load(magFilterFilepath);
            _magFilterCoreType = _magFilterAssembly.GetType("MagFilter.FilterCore");
            _magFilterObject = Activator.CreateInstance(_magFilterCoreType);
            MethodInfo startupMethod = _magFilterCoreType.GetMethod("ExternalStartup");
            startupMethod.Invoke(_magFilterObject, null);
             * 
             * */
        }
        public void UnloadMagFilter()
        {
            if (_appDomain == null) { ErrorMsg("AppDomain not loaded"); return; }
            if (_magFilterCoreType != null && _magFilterObject != null)
            {
                MethodInfo shutdownMethod = _magFilterCoreType.GetMethod("ExternalShutdown");
                shutdownMethod.Invoke(_magFilterObject, null);
            }
            AppDomain.Unload(_appDomain);

            GC.Collect();
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        public void StartupMagFilter()
        {
            if (_magFilterCore != null) { ErrorMsg("_magFilterCore not null"); return; }

            _magFilterCore = new MagFilter.FilterCore();
            _magFilterCore.ExternalStartup();
        }
        public void ShutdownMagFilter()
        {
            if (_magFilterCore == null) { ErrorMsg("_magFilterCore is null"); return; }

            _magFilterCore.ExternalShutdown();
            _magFilterCore = null;
        }

        private void ErrorMsg(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }

        public class ProxyDomain : MarshalByRefObject
        {
            public Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFile(assemblyPath);
                }
                catch (Exception exc)
                {
                    throw new Exception("Failed Assembly.LoadFile", exc);
                }
            }
        }

    }
}
