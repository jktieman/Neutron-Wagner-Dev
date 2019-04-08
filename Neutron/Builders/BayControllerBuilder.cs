using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Neutron.Builders.Interfaces;
using Neutron.Builders.Rules.BayControllerRules;
using Neutron.Models;
using NeutronEvents;

namespace Neutron.Builders
{
    public class BayControllerBuilder : IBayControllerBuilder
    {
        private readonly List<IBayControllerRule> _bayControllerRules;

        public BayControllerBuilder()
        {
            _bayControllerRules = new List<IBayControllerRule>
            {
                new BayControllerRule_01(),
                //new BayControllerRule_02(),
                //new BayControllerRule_03(),
                //new BayControllerRule_04(),
                //new BayControllerRule_05(),
                //new BayControllerRule_06(),
                //new BayControllerRule_07(),
                new BayControllerRule_14(),
                new BayControllerRule_17(),
                //new BayControllerRule_30(),
                new BayControllerRule_27(),
                new BayControllerRule_33(),
                new BayControllerRule_39(),
                //new BayControllerRule_Unknown()
            };
        }

        public void BuildBayController(string response, ResponseInfo responseInfo)
        {
            try
            {
                _bayControllerRules.First(c => c.IsMatch(response.Substring(3, 2), responseInfo))
                    .BuildInfo(response, responseInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BayController Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
    }
}