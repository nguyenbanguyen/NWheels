﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using NWheels.Hosting;

namespace NWheels.Globalization.Core
{
    public abstract class LocalizationProviderBase : LifecycleEventListenerBase, ILocalizationProvider, ICoreLocalizationProvider
    {
        private readonly object _initSyncRoot = new object();
        private readonly ILocale _fallbackLocale = new VoidLocalizationProvider();
        private ImmutableDictionary<string, ILocale> _localeByCultureName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ILocalizationProvider

        public virtual ILocale GetDefaultLocale()
        {
            EnsureInitialized();

            ILocale english;

            if (_localeByCultureName.TryGetValue("en-US", out english) || _localeByCultureName.TryGetValue("en", out english))
            {
                return english;
            }

            return (_localeByCultureName.Values.FirstOrDefault() ?? _fallbackLocale);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILocale GetCurrentLocale()
        {
            EnsureInitialized();
            return _localeByCultureName.GetValueOrDefault(Thread.CurrentThread.CurrentUICulture.Name, _fallbackLocale);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILocale[] GetAllSupportedLocales()
        {
            EnsureInitialized();
            return _localeByCultureName.Values.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILocale GetLocale(CultureInfo culture)
        {
            EnsureInitialized();
            return _localeByCultureName.GetValueOrDefault(culture.Name, _fallbackLocale);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILocale GetLocale(string isoCode)
        {
            EnsureInitialized();

            ILocale locale;

            if (_localeByCultureName.TryGetValue(isoCode, out locale))
            {
                return locale;
            }

            locale = _localeByCultureName
                .Where(kvp => kvp.Key.StartsWith(isoCode + "-", StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Value).FirstOrDefault();
            
            return (locale ?? _fallbackLocale);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ICoreLocalizationProvider

        public virtual ICoreLocale GetCoreLocale(CultureInfo culture)
        {
            return (ICoreLocale)GetLocale(culture);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Refresh()
        {
            if (!Monitor.TryEnter(_initSyncRoot, 30000))
            {
                throw new TimeoutException("LocalizationProviderBase.Refresh failed to acquire lock within allotted timeout.");
            }

            try
            {
                Initialize(out _localeByCultureName);
            }
            finally
            {
                Monitor.Exit(_initSyncRoot);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void Initialize(out ImmutableDictionary<string, ILocale> localeByCultureName);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void EnsureInitialized()
        {
            if (_localeByCultureName == null)
            {
                if (!Monitor.TryEnter(_initSyncRoot, 30000))
                {
                    throw new TimeoutException("LocalizationProviderBase.EnsureInitialized failed to acquire lock within allotted timeout.");
                }

                try
                {
                    if (_localeByCultureName == null)
                    {
                        Initialize(out _localeByCultureName);
                    }
                }
                finally
                {
                    Monitor.Exit(_initSyncRoot);
                }
            }
        }
    }
}