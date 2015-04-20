using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Chutzpah.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Chutzpah.VS2012.TestAdapter
{
    [FileExtension(Chutzpah.Constants.CoffeeScriptExtension)]
    [FileExtension(Chutzpah.Constants.TypeScriptExtension)]
    [FileExtension(Chutzpah.Constants.JavaScriptExtension)]
    [FileExtension(Chutzpah.Constants.HtmlScriptExtension)]
    [FileExtension(Chutzpah.Constants.HtmScriptExtension)]
    [FileExtension(Chutzpah.Constants.CshtmlScriptExtension)]
    [FileExtension(Chutzpah.Constants.JsonExtension)]
    [DefaultExecutorUri(AdapterConstants.ExecutorUriString)]
    public class ChutzpahTestDiscoverer : ITestDiscoverer
    {
        private readonly ITestRunner testRunner;

        public ChutzpahTestDiscoverer()
        {
            testRunner = TestRunner.Create();
        }

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            ChutzpahTracer.TraceInformation("Begin Test Adapter Discover Tests");
            var settingsProvider = discoveryContext.RunSettings.GetSettings(AdapterConstants.SettingsName) as ChutzpahAdapterSettingsProvider;
            var settings = settingsProvider != null ? settingsProvider.Settings : new ChutzpahAdapterSettings();

            ChutzpahTracingHelper.Toggle(settings.EnabledTracing);

            var testOptions = new TestOptions
            {
                MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism,
                ChutzpahSettingsFileEnvironments = new ChutzpahSettingsFileEnvironments(settings.ChutzpahSettingsFileEnvironments)
            };

            IList<TestError> errors;
            var testCases = testRunner.DiscoverTests(sources, testOptions, out errors);

            ChutzpahTracer.TraceInformation("Sending discovered tests to test case discovery sink");

            foreach (var testCase in testCases)
            {
                var vsTestCase = testCase.ToVsTestCase();
                discoverySink.SendTestCase(vsTestCase);
            }

            foreach (var error in errors)
            {
                logger.SendMessage(TestMessageLevel.Error, RunnerCallback.FormatFileErrorMessage(error));
            }

            ChutzpahTracer.TraceInformation("End Test Adapter Discover Tests");

        }
    }
}