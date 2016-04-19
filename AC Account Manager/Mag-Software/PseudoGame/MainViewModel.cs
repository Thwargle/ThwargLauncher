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
        public ICommand ShutdownMagFilterCommand { get; private set; }
        private AppDomain _appDomain;
        private Assembly _magFilterAssembly;
        private Type _magFilterCoreType;
        private object _magFilterObject;

        public MainViewModel()
        {
            LoadMagFilterCommand = new DelegateCommand(LoadMagFilter);
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
        public void ShutdownMagFilter()
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
